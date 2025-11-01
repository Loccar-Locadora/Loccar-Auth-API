using System.Net;
using System.Net.Http;
using System.Text;

namespace LoccarTests
{
    /// <summary>
    /// Factory simples para criar HttpClients com mocks.
    /// </summary>
    internal static class MockHttpClientFactory
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
            
            // O handler será disposto quando o HttpClient for disposto
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler, disposeHandler: true);
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
            
            // O handler será disposto quando o HttpClient for disposto
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler, disposeHandler: true);
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
            
            // O handler será disposto quando o HttpClient for disposto
            var handler = new FakeHttpMessageHandler(response);
            return new HttpClient(handler, disposeHandler: true);
        }
    }
}
