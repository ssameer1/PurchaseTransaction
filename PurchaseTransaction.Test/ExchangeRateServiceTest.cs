using Castle.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework.Legacy;
using PurchaseTransaction.Services;

namespace PurchaseTransaction.Test
{

    public class ExchangeRateServiceTest
    {

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public HttpRequestMessage? LastRequestMessage { get; private set; }
            public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder ?? throw new ArgumentNullException(nameof(responder));
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequestMessage = request;
                return Task.FromResult(_responder(request));
            }
        }

        private static HttpClient CreateMockHttpClient(MockHttpMessageHandler handler)
        {
            var client = new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = new Uri("https://fiscal.treasury.gov/")
            };
            return client;
        }

        [Test]
        public async Task Returns_rate_when_api_returs_valid_data()
        {
            var jsonResponse = @"
            {
                ""data"": [
                    {
                        ""record_date"": ""2025-06-15"",
                        ""currency"": ""Canada-Dollar"",
                        ""exchange_rate"": ""1.3456""
                    }
                ]
            }";
            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });
            var httpClient = CreateMockHttpClient(handler);
            var logger = NullLogger<ExchangeRateService>.Instance;
            var ers = new ExchangeRateService(httpClient, logger);

            var purchaseDate = new DateOnly(2023, 10, 15);

            var resutl = await ers.GetRateOnOrBeforeAsync("Canada-Dollar", purchaseDate);


            Assert.That(resutl!.RateDate, Is.EqualTo(new DateOnly(2025, 6, 15)));
            Assert.That(resutl.RatePerUsd, Is.EqualTo(1.3456m));
            Assert.That(handler.LastRequestMessage, Is.Not.Null);
        }

        [Test]
        public async Task Returns_null_when_no_data_found()
        {
            var jsonResponse = @"
            {
                ""data"": []
            }";
            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            var httpClient = CreateMockHttpClient(handler);
            var logger = NullLogger<ExchangeRateService>.Instance;
            var ers = new ExchangeRateService(httpClient, logger);

            var purchaseDate = new DateOnly(2025, 10, 15);

            var resutl = await ers.GetRateOnOrBeforeAsync("Canada-Dollar", purchaseDate);

            Assert.That(resutl, Is.Null);
        }

        [Test]
        public async Task Returns_Six_Month_Message()
        {
            var jsonResponse = """
            {
             "data" : [ {
                "record_date": "2025-07-01",
                 "country": "Canada",
                 "currency": "Dollar",
                 "country_currency_desc": "Canada-Dollar",
                 "exchange_rate": "67.33",
                 }]
            """;

            var handler = new MockHttpMessageHandler(_ =>
                new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            var httpClient = CreateMockHttpClient(handler);
            var logger = NullLogger<ExchangeRateService>.Instance;
            var ers = new ExchangeRateService(httpClient, logger);

            var purchaseDate = new DateOnly(2025, 10, 01);
            var expectedStartDate = "record_date:lte:2025-10-01";
            var expectedEndDate =  "record_date:gte:2025-04-01"; // 6 months before purchase date

            var resutl = await ers.GetRateOnOrBeforeAsync("Canada-Dollar", purchaseDate);

            var uri = handler.LastRequestMessage!.RequestUri!.ToString();
            Assert.That(uri, Does.Contain("filter=country_currency_desc:eq:Canada-Dollar"));
            Assert.That(uri, Does.Contain(expectedStartDate));
            Assert.That(uri, Does.Contain(expectedEndDate));
        }
    }
}
