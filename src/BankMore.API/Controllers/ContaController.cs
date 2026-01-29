using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BankMore.API.Services;
using BankMore.Infrastructure.Data;
using BankMore.Domain.Entities;

namespace BankMore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContaController : ControllerBase
{
    private readonly BankMoreDbContext _context;
    private readonly CryptoService _cryptoService;

    public ContaController(BankMoreDbContext context)
    {
        _context = context;
        _cryptoService = new CryptoService();
    }

    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarContaRequest request)
    {
        if (string.IsNullOrEmpty(request.Cpf) || request.Cpf.Length != 11)
            return BadRequest(new { message = "CPF inválido", errorType = "INVALID_DOCUMENT" });

        if (string.IsNullOrEmpty(request.Senha))
            return BadRequest(new { message = "Senha é obrigatória", errorType = "INVALID_PASSWORD" });

        var cpfCriptografado = _cryptoService.Encrypt(request.Cpf);

        var contaExistente = await _context.ContasCorrente
            .FirstOrDefaultAsync(c => c.CpfCriptografado == cpfCriptografado);

        if (contaExistente != null)
            return BadRequest(new { message = "CPF já cadastrado", errorType = "DUPLICATE_DOCUMENT" });

        var numeroConta = new Random().Next(100000, 999999).ToString();

        var conta = new ContaCorrente
        {
            CpfCriptografado = cpfCriptografado,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            NumeroConta = numeroConta,
            NomeTitular = $"Cliente {numeroConta}",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        _context.ContasCorrente.Add(conta);
        await _context.SaveChangesAsync();

        return Ok(new { numeroConta });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var contas = await _context.ContasCorrente.ToListAsync();

        ContaCorrente conta = null;

        conta = contas.FirstOrDefault(c => c.NumeroConta == request.Identificador);

        if (conta == null)
        {
            foreach (var c in contas)
            {
                var cpfDescriptografado = _cryptoService.Decrypt(c.CpfCriptografado);
                if (cpfDescriptografado == request.Identificador)
                {
                    conta = c;
                    break;
                }
            }
        }

        if (conta == null)
            return Unauthorized(new { message = "Conta não encontrada", errorType = "USER_UNAUTHORIZED" });

        var senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, conta.SenhaHash);

        if (!senhaValida)
            return Unauthorized(new { message = "Senha inválida", errorType = "USER_UNAUTHORIZED" });

        if (!conta.Ativo)
            return Unauthorized(new { message = "Conta inativa", errorType = "INACTIVE_ACCOUNT" });

        var cpfDoUsuario = _cryptoService.Decrypt(conta.CpfCriptografado);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("MinhaChaveSecretaSuperSegura1234567890");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim("contaId", conta.Id.ToString()),
            new Claim("numeroConta", conta.NumeroConta),
            new Claim("cpf", cpfDoUsuario)
        }),
            Expires = DateTime.UtcNow.AddHours(3),
            Issuer = "BankMore",
            Audience = "BankMoreUsers",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { token = tokenString });
    }

    [HttpGet("saldo")]
    [Authorize]
    public async Task<IActionResult> ConsultarSaldo()
    {
        var contaId = User.FindFirst("contaId")?.Value;

        if (string.IsNullOrEmpty(contaId))
            return Unauthorized(new { message = "Token inválido", errorType = "INVALID_TOKEN" });

        var conta = await _context.ContasCorrente
            .FirstOrDefaultAsync(c => c.Id == int.Parse(contaId));

        if (conta == null)
            return BadRequest(new { message = "Conta não encontrada", errorType = "INVALID_ACCOUNT" });

        if (!conta.Ativo)
            return BadRequest(new { message = "Conta inativa", errorType = "INACTIVE_ACCOUNT" });

        var movimentos = await _context.Movimentos
            .Where(m => m.ContaCorrenteId == conta.Id)
            .ToListAsync();

        var creditos = movimentos
            .Where(m => m.Tipo == "C")
            .Sum(m => m.Valor);

        var debitos = movimentos
            .Where(m => m.Tipo == "D")
            .Sum(m => m.Valor);

        var saldo = creditos - debitos;

        return Ok(new
        {
            numeroConta = conta.NumeroConta,
            nomeTitular = conta.NomeTitular,
            dataConsulta = DateTime.UtcNow,
            saldo = saldo
        });
    }
}

public class CadastrarContaRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Identificador { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}