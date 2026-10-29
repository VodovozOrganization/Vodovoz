using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Xunit;

namespace EdoServices.Tests
{
	public class TrueMarkApiClientTests
	{
		[Fact]
		public async Task GetDocumentInfo_WhenStatusCheckedOk_ReturnsOk()
		{
			var client = CreateClient(HttpStatusCode.OK, "{\"status\":\"CHECKED_OK\",\"errors\":null}");

			var result = await client.GetDocumentInfo(Guid.NewGuid(), "1234567890", CancellationToken.None);

			Assert.Equal(TrueMarkDocumentStatus.Ok, result.Status);
		}

		[Fact]
		public async Task GetDocumentInfo_WhenStatusInProgress_ReturnsPending()
		{
			var client = CreateClient(HttpStatusCode.OK, "{\"status\":\"IN_PROGRESS\",\"errors\":null}");

			var result = await client.GetDocumentInfo(Guid.NewGuid(), "1234567890", CancellationToken.None);

			Assert.Equal(TrueMarkDocumentStatus.Pending, result.Status);
			Assert.Null(result.ErrorMessage);
		}

		[Fact]
		public async Task GetDocumentInfo_WhenStatusCheckedNotOk_ReturnsError()
		{
			var client = CreateClient(
				HttpStatusCode.OK,
				"{\"status\":\"CHECKED_NOT_OK\",\"errors\":[\"Document rejected\"]}");

			var result = await client.GetDocumentInfo(Guid.NewGuid(), "1234567890", CancellationToken.None);

			Assert.Equal(TrueMarkDocumentStatus.Error, result.Status);
			Assert.Equal("Document rejected", result.ErrorMessage);
		}

		[Fact]
		public async Task GetDocumentInfo_WhenApiReturnsServerError_ReturnsPending()
		{
			var client = CreateClient(HttpStatusCode.InternalServerError, string.Empty);

			var result = await client.GetDocumentInfo(Guid.NewGuid(), "1234567890", CancellationToken.None);

			Assert.Equal(TrueMarkDocumentStatus.Pending, result.Status);
			Assert.NotNull(result.ErrorMessage);
		}

		private static TrueMarkApiClient CreateClient(HttpStatusCode statusCode, string responseBody)
		{
			var handler = new StubHttpMessageHandler(statusCode, responseBody);
			var httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("http://localhost/")
			};

			return new TrueMarkApiClient(httpClient);
		}

		private class StubHttpMessageHandler : HttpMessageHandler
		{
			private readonly HttpStatusCode _statusCode;
			private readonly string _responseBody;

			public StubHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
			{
				_statusCode = statusCode;
				_responseBody = responseBody;
			}

			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(_statusCode)
				{
					Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
				});
			}
		}
	}
}
