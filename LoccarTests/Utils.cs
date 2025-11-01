using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoccarTests
{
    /// <summary>
    /// Handler HTTP fake simples para testes.
    /// </summary>
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        private readonly List<HttpRequestMessage> _requests = new();

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public IReadOnlyList<HttpRequestMessage> Requests => _requests;

        public HttpRequestMessage? LastRequest => _requests.LastOrDefault();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requests.Add(request);
            return Task.FromResult(_response);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _response?.Dispose();
                foreach (var request in _requests)
                {
                    request?.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Factory simples para criar HttpClients com mocks.
    /// </summary>
    public static class MockHttpClientFactory
    {
        /// <summary>
        /// Cria um HttpClient que sempre retorna sucesso (201 Created).
        /// </summary>
        /// <returns>HttpClient configurado para retornar sucesso.</returns>
        public static HttpClient CreateSuccessClient()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{\"status\":\"success\"}", Encoding.UTF8, "application/json"),
            };
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler);
        }

        /// <summary>
        /// Cria um HttpClient que sempre retorna erro (400 BadRequest).
        /// </summary>
        /// <returns>HttpClient configurado para retornar erro.</returns>
        public static HttpClient CreateErrorClient()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"Bad Request\"}", Encoding.UTF8, "application/json"),
            };
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler);
        }

        /// <summary>
        /// Cria um HttpClient para testes de registro (sucesso ou falha).
        /// </summary>
        /// <param name="success">Se deve retornar sucesso ou falha.</param>
        /// <returns>HttpClient configurado conforme especificado.</returns>
        public static HttpClient CreateCustomerRegisterClient(bool success = true)
        {
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
            var content = success
                ? "{\"message\":\"Customer registered successfully\"}"
                : "{\"error\":\"Registration failed\"}";

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
            };
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler);
        }
    }
}
