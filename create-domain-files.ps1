# create-domain-files.ps1
Write-Host "Criando arquivos do Domain Layer..." -ForegroundColor Green

# ========== COMMON ==========
Write-Host "Criando Common..." -ForegroundColor Cyan

@"
namespace BankMore.Domain.Common;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !Equals(left, right);
    }
}
"@ | Out-File src/BankMore.Domain/Common/ValueObject.cs -Encoding UTF8

@"
namespace BankMore.Domain.Common;

public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
"@ | Out-File src/BankMore.Domain/Common/Entity.cs -Encoding UTF8

@"
namespace BankMore.Domain.Common;

public abstract class AggregateRoot : Entity
{
    // Aggregate Root específico pode ter lógica adicional
}
"@ | Out-File src/BankMore.Domain/Common/AggregateRoot.cs -Encoding UTF8

@"
namespace BankMore.Domain.Common;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    
    public DomainException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
    
    public DomainException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
"@ | Out-File src/BankMore.Domain/Common/DomainException.cs -Encoding UTF8

# ========== VALUE OBJECTS ==========
Write-Host "Criando ValueObjects..." -ForegroundColor Cyan

@"
using BankMore.Domain.Common;

namespace BankMore.Domain.ValueObjects;

public sealed class CPF : ValueObject
{
    public string Numero { get; }
    
    private CPF(string numero)
    {
        if (!Validar(numero))
            throw new DomainException(""CPF inválido"", ""INVALID_DOCUMENT"");
        
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
"@ | Out-File src/BankMore.Domain/ValueObjects/CPF.cs -Encoding UTF8

@"
using BankMore.Domain.Common;

namespace BankMore.Domain.ValueObjects;

public sealed class Dinheiro : ValueObject
{
    public decimal Valor { get; }
    public string Moeda { get; } = ""BRL"";
    
    private Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new DomainException(""Valor monetário não pode ser negativo"", ""INVALID_VALUE"");
        
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
        return Valor.ToString(""N2"");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor;
        yield return Moeda;
    }
}
"@ | Out-File src/BankMore.Domain/ValueObjects/Dinheiro.cs -Encoding UTF8

# ========== ENUMS ==========
Write-Host "Criando Enums..." -ForegroundColor Cyan

@"
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
            _ => throw new ArgumentException($""Tipo de movimento inválido: {c}"")
        };
    }
}
"@ | Out-File src/BankMore.Domain/Enums/TipoMovimento.cs -Encoding UTF8

# ========== EVENTS ==========
Write-Host "Criando Events..." -ForegroundColor Cyan

@"
namespace BankMore.Domain.Events;

public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
"@ | Out-File src/BankMore.Domain/Events/DomainEvent.cs -Encoding UTF8

@"
namespace BankMore.Domain.Events;

public class ContaCriadaEvent : DomainEvent
{
    public Guid ContaCorrenteId { get; }
    public int NumeroConta { get; }
    
    public ContaCriadaEvent(Guid contaCorrenteId, int numeroConta)
    {
        ContaCorrenteId = contaCorrenteId;
        NumeroConta = numeroConta;
    }
}
"@ | Out-File src/BankMore.Domain/Events/ContaCriadaEvent.cs -Encoding UTF8

@"
namespace BankMore.Domain.Events;

public class ContaInativadaEvent : DomainEvent
{
    public Guid ContaCorrenteId { get; }
    
    public ContaInativadaEvent(Guid contaCorrenteId)
    {
        ContaCorrenteId = contaCorrenteId;
    }
}
"@ | Out-File src/BankMore.Domain/Events/ContaInativadaEvent.cs -Encoding UTF8

@"
namespace BankMore.Domain.Events;

public class MovimentoRealizadoEvent : DomainEvent
{
    public Guid ContaCorrenteId { get; }
    public decimal Valor { get; }
    public TipoMovimento Tipo { get; }
    
    public MovimentoRealizadoEvent(Guid contaCorrenteId, decimal valor, TipoMovimento tipo)
    {
        ContaCorrenteId = contaCorrenteId;
        Valor = valor;
        Tipo = tipo;
    }
}
"@ | Out-File src/BankMore.Domain/Events/MovimentoRealizadoEvent.cs -Encoding UTF8

@"
namespace BankMore.Domain.Events;

public class TransferenciaRealizadaEvent : DomainEvent
{
    public Guid TransferenciaId { get; }
    public Guid ContaOrigemId { get; }
    public Guid ContaDestinoId { get; }
    public decimal Valor { get; }
    
    public TransferenciaRealizadaEvent(Guid transferenciaId, Guid contaOrigemId, Guid contaDestinoId, decimal valor)
    {
        TransferenciaId = transferenciaId;
        ContaOrigemId = contaOrigemId;
        ContaDestinoId = contaDestinoId;
        Valor = valor;
    }
}
"@ | Out-File src/BankMore.Domain/Events/TransferenciaRealizadaEvent.cs -Encoding UTF8

# ========== ENTITIES ==========
Write-Host "Criando Entities..." -ForegroundColor Cyan

@"
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
            throw new DomainException(""Senha inválida"", ""INVALID_PASSWORD"");
        
        Ativo = false;
        AddDomainEvent(new ContaInativadaEvent(Id));
    }
    
    public void AdicionarMovimento(Movimento movimento)
    {
        if (!Ativo)
            throw new DomainException(""Conta inativa"", ""INACTIVE_ACCOUNT"");
        
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
"@ | Out-File src/BankMore.Domain/Entities/ContaCorrente.cs -Encoding UTF8

@"
using BankMore.Domain.Common;
using BankMore.Domain.ValueObjects;
using BankMore.Domain.Enums;

namespace BankMore.Domain.Entities;

public class Movimento : Entity
{
    public Guid Id { get; private set; }
    public Guid ContaCorrenteId { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public TipoMovimento Tipo { get; private set; }
    public Dinheiro Valor { get; private set; }
    public string IdRequisicao { get; private set; }
    
    private Movimento() { }
    
    public Movimento(Guid contaCorrenteId, Dinheiro valor, TipoMovimento tipo, string idRequisicao)
    {
        Id = Guid.NewGuid();
        ContaCorrenteId = contaCorrenteId;
        DataMovimento = DateTime.UtcNow;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        Tipo = tipo;
        IdRequisicao = idRequisicao ?? throw new ArgumentNullException(nameof(idRequisicao));
    }
    
    public static Movimento CriarCredito(Guid contaCorrenteId, decimal valor, string idRequisicao)
    {
        return new Movimento(contaCorrenteId, Dinheiro.Criar(valor), TipoMovimento.Credito, idRequisicao);
    }
    
    public static Movimento CriarDebito(Guid contaCorrenteId, decimal valor, string idRequisicao)
    {
        return new Movimento(contaCorrenteId, Dinheiro.Criar(valor), TipoMovimento.Debito, idRequisicao);
    }
}
"@ | Out-File src/BankMore.Domain/Entities/Movimento.cs -Encoding UTF8

@"
using BankMore.Domain.Common;
using BankMore.Domain.ValueObjects;

namespace BankMore.Domain.Entities;

public class Transferencia : Entity
{
    public Guid Id { get; private set; }
    public Guid ContaCorrenteOrigemId { get; private set; }
    public Guid ContaCorrenteDestinoId { get; private set; }
    public DateTime DataMovimento { get; private set; }
    public Dinheiro Valor { get; private set; }
    public string IdRequisicao { get; private set; }
    
    private Transferencia() { }
    
    public Transferencia(Guid contaOrigemId, Guid contaDestinoId, Dinheiro valor, string idRequisicao)
    {
        Id = Guid.NewGuid();
        ContaCorrenteOrigemId = contaOrigemId;
        ContaCorrenteDestinoId = contaDestinoId;
        DataMovimento = DateTime.UtcNow;
        Valor = valor ?? throw new ArgumentNullException(nameof(valor));
        IdRequisicao = idRequisicao ?? throw new ArgumentNullException(nameof(idRequisicao));
    }
    
    public static Transferencia Criar(Guid contaOrigemId, Guid contaDestinoId, decimal valor, string idRequisicao)
    {
        return new Transferencia(contaOrigemId, contaDestinoId, Dinheiro.Criar(valor), idRequisicao);
    }
}
"@ | Out-File src/BankMore.Domain/Entities/Transferencia.cs -Encoding UTF8

# ========== REMOVER ARQUIVOS DESNECESSÁRIOS ==========
Write-Host "Removendo arquivos padrão..." -ForegroundColor Yellow
Remove-Item src/BankMore.Domain/Class1.cs -Force -ErrorAction SilentlyContinue
Remove-Item src/BankMore.Application/Class1.cs -Force -ErrorAction SilentlyContinue
Remove-Item src/BankMore.Infrastructure/Class1.cs -Force -ErrorAction SilentlyContinue

Write-Host "Domain Layer criado com sucesso!" -ForegroundColor Green
Write-Host "Compilando projeto..." -ForegroundColor Cyan
dotnet build src/BankMore.Domain

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilação bem-sucedida! Agora recarregue o projeto no VS." -ForegroundColor Green
} else {
    Write-Host "Erro na compilação. Verifique os arquivos." -ForegroundColor Red
}