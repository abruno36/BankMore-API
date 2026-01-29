# fix-domain-files.ps1
Write-Host "Corrigindo arquivos do Domain Layer..." -ForegroundColor Green

# ========== CORRIGIR ENUMS ==========
Write-Host "Corrigindo Enums..." -ForegroundColor Cyan

$tipoMovimentoContent = @'
namespace BankMore.Domain.Enums;

public enum TipoMovimento
{
    Credito = 'C',
    Debito = 'D'
}

public static class TipoMovimentoExtensions
{
    public static char ToChar(this TipoMovimento tipo)
    {
        return (char)tipo;
    }
    
    public static TipoMovimento FromChar(char c)
    {
        return c switch
        {
            'C' => TipoMovimento.Credito,
            'D' => TipoMovimento.Debito,
            _ => throw new ArgumentException($"Tipo de movimento inválido: {c}")
        };
    }
}
'@

[System.IO.File]::WriteAllText("src/BankMore.Domain/Enums/TipoMovimento.cs", $tipoMovimentoContent, [System.Text.Encoding]::UTF8)

# ========== CORRIGIR ENTITIES ==========
Write-Host "Corrigindo Entities..." -ForegroundColor Cyan

# ContaCorrente.cs
$contaCorrenteContent = @'
using BankMore.Domain.Common;
using BankMore.Domain.Events;
using BankMore.Domain.ValueObjects;
using BankMore.Domain.Enums;

namespace BankMore.Domain.Entities;

public class ContaCorrente : AggregateRoot
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; }
    public bool Ativo { get; private set; }
    public string SenhaHash { get; private set; }
    public string Salt { get; private set; }
    public DateTime DataCriacao { get; private set; }
    
    private string _cpfCriptografado;
    public string CpfCriptografado => _cpfCriptografado;
    
    private readonly List<Movimento> _movimentos = new();
    public IReadOnlyCollection<Movimento> Movimentos => _movimentos.AsReadOnly();
    
    // EF Core precisa deste construtor
    private ContaCorrente() { }
    
    public ContaCorrente(int numero, string nome, string cpfCriptografado, string senhaHash, string salt)
    {
        Id = Guid.NewGuid();
        Numero = numero;
        Nome = nome ?? throw new ArgumentNullException(nameof(nome));
        _cpfCriptografado = cpfCriptografado ?? throw new ArgumentNullException(nameof(cpfCriptografado));
        SenhaHash = senhaHash ?? throw new ArgumentNullException(nameof(senhaHash));
        Salt = salt ?? throw new ArgumentNullException(nameof(salt));
        Ativo = true;
        DataCriacao = DateTime.UtcNow;
        
        AddDomainEvent(new ContaCriadaEvent(Id, numero));
    }
    
    public void Inativar(string senha, Func<string, string, string, bool> validarSenha)
    {
        if (!validarSenha(senha, SenhaHash, Salt))
            throw new DomainException("Senha inválida", "INVALID_PASSWORD");
        
        Ativo = false;
        AddDomainEvent(new ContaInativadaEvent(Id));
    }
    
    public void AdicionarMovimento(Movimento movimento)
    {
        if (!Ativo)
            throw new DomainException("Conta inativa", "INACTIVE_ACCOUNT");
        
        _movimentos.Add(movimento);
        AddDomainEvent(new MovimentoRealizadoEvent(Id, movimento.Valor.Valor, movimento.Tipo));
    }
    
    public Dinheiro CalcularSaldo()
    {
        var saldo = Dinheiro.Zero;
        
        foreach (var movimento in _movimentos)
        {
            saldo = movimento.Tipo == TipoMovimento.Credito 
                ? saldo.Somar(movimento.Valor)
                : saldo.Subtrair(movimento.Valor);
        }
        
        return saldo;
    }
    
    public bool TemSaldoSuficiente(Dinheiro valor)
    {
        return CalcularSaldo().MaiorQue(valor) || CalcularSaldo().Valor == valor.Valor;
    }
}
'@

[System.IO.File]::WriteAllText("src/BankMore.Domain/Entities/ContaCorrente.cs", $contaCorrenteContent, [System.Text.Encoding]::UTF8)

# ========== CORRIGIR VALUE OBJECTS ==========
Write-Host "Corrigindo Value Objects..." -ForegroundColor Cyan

# CPF.cs
$cpfContent = @'
using BankMore.Domain.Common;

namespace BankMore.Domain.ValueObjects;

public sealed class CPF : ValueObject
{
    public string Numero { get; }
    
    private CPF(string numero)
    {
        if (!Validar(numero))
            throw new DomainException("CPF inválido", "INVALID_DOCUMENT");
        
        Numero = LimparFormatacao(numero);
    }
    
    public static CPF Criar(string numero)
    {
        return new CPF(numero);
    }
    
    private static string LimparFormatacao(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }
    
    private static bool Validar(string cpf)
    {
        cpf = LimparFormatacao(cpf);
        
        if (cpf.Length != 11)
            return false;
        
        if (cpf.Distinct().Count() == 1)
            return false;
        
        // Cálculo do primeiro dígito verificador
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (cpf[i] - '0') * (10 - i);
        
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        
        if (cpf[9] - '0' != digito1)
            return false;
        
        // Cálculo do segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (cpf[i] - '0') * (11 - i);
        
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;
        
        return cpf[10] - '0' == digito2;
    }
    
    public string Formatado()
    {
        return Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Numero;
    }
}
'@

[System.IO.File]::WriteAllText("src/BankMore.Domain/ValueObjects/CPF.cs", $cpfContent, [System.Text.Encoding]::UTF8)

# Dinheiro.cs
$dinheiroContent = @'
using BankMore.Domain.Common;

namespace BankMore.Domain.ValueObjects;

public sealed class Dinheiro : ValueObject
{
    public decimal Valor { get; }
    public string Moeda { get; } = "BRL";
    
    private Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new DomainException("Valor monetário não pode ser negativo", "INVALID_VALUE");
        
        Valor = Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
    
    public static Dinheiro Criar(decimal valor)
    {
        return new Dinheiro(valor);
    }
    
    public static Dinheiro Zero => new(0m);
    
    public Dinheiro Somar(Dinheiro outro)
    {
        return Criar(Valor + outro.Valor);
    }
    
    public Dinheiro Subtrair(Dinheiro outro)
    {
        return Criar(Valor - outro.Valor);
    }
    
    public bool MaiorQue(Dinheiro outro) => Valor > outro.Valor;
    public bool MaiorQueZero => Valor > 0;
    public bool MenorOuIgualA(Dinheiro outro) => Valor <= outro.Valor;
    
    public override string ToString()
    {
        return Valor.ToString("N2");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
        yield return Moeda;
    }
}
'@

[System.IO.File]::WriteAllText("src/BankMore.Domain/ValueObjects/Dinheiro.cs", $dinheiroContent, [System.Text.Encoding]::UTF8)

Write-Host "Arquivos corrigidos! Compilando..." -ForegroundColor Green
dotnet build src/BankMore.Domain