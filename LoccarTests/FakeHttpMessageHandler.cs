using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoccarTests
{
    /// <summary>
    /// Handler HTTP fake simples para testes.
    /// </summary>
    internal class FakeHttpMessageHandler : HttpMessageHandler
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
}
