using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Calculon.Services
{
    public class HttpRetryHandler : DelegatingHandler
    {
        private readonly int MaxRetries;

        public HttpRetryHandler(HttpMessageHandler innerHandler, int maxRetries)
            : base(innerHandler)
        {
            MaxRetries = maxRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (var i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode ||
                    IsClientFailure(response.StatusCode) ||
                    cancellationToken.IsCancellationRequested)
                {
                    return response;
                }
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), cancellationToken);
            }

            return response;
        }

        private bool IsClientFailure(HttpStatusCode statusCode)
        {
            var statusCodeValue = (int)statusCode;
            return statusCodeValue >= 400 && statusCodeValue < 500;
        }
    }
}