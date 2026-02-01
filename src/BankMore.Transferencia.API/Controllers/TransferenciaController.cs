using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace BankMore.Transferencia.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferenciaController : ControllerBase
    {
        private readonly ILogger<TransferenciaController> _logger;
        private readonly string _connectionString;

        public TransferenciaController(
            ILogger<TransferenciaController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = "Data Source=C:\\Projetos\\BankMore\\src\\BankMore.API\\bankmore.db";
            CriarColunaIdRequisicaoSeNecessario();
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] TransferenciaRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
        {
            var transferenciaId = Guid.NewGuid().ToString();
            var idRequisicao = idempotencyKey ?? Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Iniciando transferência ID: {Id}", transferenciaId);

                await using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    var existente = await BuscarTransferenciaPorIdRequisicao(connection, idempotencyKey);
                    if (existente != null)
                    {
                        return Ok(new
                        {
                            Success = true,
                            Mensagem = "Transferência já processada anteriormente",
                            Dados = existente,
                            IdempotencyKey = idempotencyKey
                        });
                    }
                }

                var contaOrigemId = await BuscarContaPorNumero(connection, "000001");
                if (string.IsNullOrEmpty(contaOrigemId))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Mensagem = "Conta origem 000001 não encontrada"
                    });
                }

                var contaDestinoId = await BuscarContaPorNumero(connection, request.NumeroContaDestino);
                if (string.IsNullOrEmpty(contaDestinoId))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Mensagem = $"Conta destino {request.NumeroContaDestino} não encontrada"
                    });
                }

                var sqlTransferencia = @"
                    INSERT INTO Transferencias 
                    (Id, ContaOrigemId, ContaDestinoId, Valor, DataTransferencia, Status, IdRequisicao) 
                    VALUES 
                    (@Id, @ContaOrigemId, @ContaDestinoId, @Valor, datetime('now'), 'CONCLUIDA', @IdRequisicao)";

                await using var cmdTransferencia = new SqliteCommand(sqlTransferencia, connection);
                cmdTransferencia.Parameters.AddWithValue("@Id", transferenciaId);
                cmdTransferencia.Parameters.AddWithValue("@ContaOrigemId", contaOrigemId);
                cmdTransferencia.Parameters.AddWithValue("@ContaDestinoId", contaDestinoId);
                cmdTransferencia.Parameters.AddWithValue("@Valor", request.Valor);
                cmdTransferencia.Parameters.AddWithValue("@IdRequisicao", idRequisicao);

                await cmdTransferencia.ExecuteNonQueryAsync();
                _logger.LogInformation("✅ Transferência registrada");

                // REGISTRA MOVIMENTAÇÕES
                await RegistrarMovimentacoes(
                    connection,
                    contaOrigemId,
                    contaDestinoId,
                    request.Valor,
                    request.Descricao);

                var transferencia = await BuscarTransferenciaPorId(connection, transferenciaId);

                return Ok(new
                {
                    Success = true,
                    Mensagem = $"Transferência de R$ {request.Valor:F2} realizada",
                    Dados = transferencia
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na transferência {Id}", transferenciaId);
                return StatusCode(500, new
                {
                    Success = false,
                    Mensagem = "Erro ao processar transferência",
                    TransferenciaId = transferenciaId
                });
            }
        }

        private async Task RegistrarMovimentacoes(
            SqliteConnection connection,
            string contaOrigemId,
            string contaDestinoId,
            decimal valor,
            string? descricao)
        {
            try
            {
                // Busca números das contas
                var numeroOrigem = await BuscarNumeroContaPorId(connection, contaOrigemId);
                var numeroDestino = await BuscarNumeroContaPorId(connection, contaDestinoId);

                // Tipo 'D' para DÉBITO (saída)**
                var sqlDebito = @"
            INSERT INTO Movimentacoes 
            (Id, ContaId, Tipo, Valor, DataMovimentacao, Descricao) 
            VALUES 
            (@Id, @ContaId, 'D', @Valor, datetime('now'), @Descricao)";

                await using var cmdDebito = new SqliteCommand(sqlDebito, connection);
                cmdDebito.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                cmdDebito.Parameters.AddWithValue("@ContaId", contaOrigemId);
                cmdDebito.Parameters.AddWithValue("@Valor", valor);
                cmdDebito.Parameters.AddWithValue("@Descricao",
                    $"Transferência para conta {numeroDestino}" +
                    (string.IsNullOrEmpty(descricao) ? "" : $" - {descricao}"));

                await cmdDebito.ExecuteNonQueryAsync();

                // Tipo 'C' para CRÉDITO (entrada)**
                var sqlCredito = @"
            INSERT INTO Movimentacoes 
            (Id, ContaId, Tipo, Valor, DataMovimentacao, Descricao) 
            VALUES 
            (@Id, @ContaId, 'C', @Valor, datetime('now'), @Descricao)";

                await using var cmdCredito = new SqliteCommand(sqlCredito, connection);
                cmdCredito.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                cmdCredito.Parameters.AddWithValue("@ContaId", contaDestinoId);
                cmdCredito.Parameters.AddWithValue("@Valor", valor);
                cmdCredito.Parameters.AddWithValue("@Descricao",
                    $"Transferência da conta {numeroOrigem}" +
                    (string.IsNullOrEmpty(descricao) ? "" : $" - {descricao}"));

                await cmdCredito.ExecuteNonQueryAsync();

                _logger.LogInformation("Movimentações registradas: D({Origem}) e C({Destino})",
                    contaOrigemId, contaDestinoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar movimentações");
                throw;
            }
        }

        private async Task<object?> BuscarTransferenciaPorIdRequisicao(SqliteConnection connection, string idRequisicao)
        {
            try
            {
                var sql = @"
                    SELECT Id, Valor, DataTransferencia, Status 
                    FROM Transferencias 
                    WHERE IdRequisicao = @IdRequisicao";

                await using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@IdRequisicao", idRequisicao);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new
                    {
                        Id = reader.GetString(0),
                        Valor = reader.GetDecimal(1),
                        DataTransferencia = reader.GetString(2),
                        Status = reader.GetString(3),
                        IdRequisicao = idRequisicao
                    };
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                _logger.LogWarning("Coluna IdRequisicao ainda não existe: {Erro}", ex.Message);
            }

            return null;
        }

        private async Task<object> BuscarTransferenciaPorId(SqliteConnection connection, string transferenciaId)
        {
            var sql = @"
                SELECT Id, Valor, DataTransferencia, Status, IdRequisicao 
                FROM Transferencias 
                WHERE Id = @Id";

            await using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", transferenciaId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new
                {
                    Id = reader.GetString(0),
                    Valor = reader.GetDecimal(1),
                    DataTransferencia = reader.GetString(2),
                    Status = reader.GetString(3),
                    IdRequisicao = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }

            throw new Exception("Transferência não encontrada");
        }

        private async Task<string?> BuscarContaPorNumero(SqliteConnection connection, string numeroConta)
        {
            var sql = "SELECT Id FROM ContasCorrentes WHERE NumeroConta = @NumeroConta";
            await using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@NumeroConta", numeroConta);

            var resultado = await cmd.ExecuteScalarAsync();
            return resultado?.ToString();
        }

        private async Task<string?> BuscarNumeroContaPorId(SqliteConnection connection, string contaId)
        {
            var sql = "SELECT NumeroConta FROM ContasCorrentes WHERE Id = @Id";
            await using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", contaId);

            var resultado = await cmd.ExecuteScalarAsync();
            return resultado?.ToString() ?? contaId;
        }

        private async void CriarColunaIdRequisicaoSeNecessario()
        {
            try
            {
                await using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var sqlVerifica = "SELECT COUNT(*) FROM pragma_table_info('Transferencias') WHERE name = 'IdRequisicao'";
                await using var cmdVerifica = new SqliteCommand(sqlVerifica, connection);
                var colunaExiste = Convert.ToInt32(await cmdVerifica.ExecuteScalarAsync()) > 0;

                if (!colunaExiste)
                {
                    _logger.LogInformation("Criando coluna IdRequisicao...");

                    var sqlCria = "ALTER TABLE Transferencias ADD COLUMN IdRequisicao TEXT";
                    await using var cmdCria = new SqliteCommand(sqlCria, connection);
                    await cmdCria.ExecuteNonQueryAsync();

                    _logger.LogInformation("✅ Coluna IdRequisicao criada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar/criar coluna IdRequisicao");
            }
        }
    }

    public class TransferenciaRequest
    {
        public string NumeroContaDestino { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Descricao { get; set; }
    }
}