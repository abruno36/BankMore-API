using BankMore.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankMore.Infrastructure.Services
{
    public class HttpContaCorrenteService : IHttpContaCorrenteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpContaCorrenteService> _logger;

        public HttpContaCorrenteService(HttpClient httpClient, ILogger<HttpContaCorrenteService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> RealizarMovimentacaoAsync(
            string token,
            string idempotencyKey,
            string contaId,  
            decimal valor,
            string tipoMovimentacao)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

                var request = new
                {
                    ContaId = contaId,  
                    Valor = valor,
                    TipoMovimentacao = tipoMovimentacao
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "api/movimentacao",
                    request);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar movimentação");
                return false;
            }
        }
    }
}