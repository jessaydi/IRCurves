namespace TermStructure.Tests
{
    public class FakeHttpHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}