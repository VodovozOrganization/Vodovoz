using System;
using Edo.Problems.Custom.Sources;
using Edo.Receipt.Sender;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	/// <summary>
	/// Тесты для классификатора ошибок отправки чека <see cref="ReceiptSendFailureClassifier"/>.
	/// </summary>
	public class ReceiptSendFailureClassifierTests
	{
		[Theory]
		[InlineData("HttpClient.Timeout", typeof(ReceiptSendTransportError))]
		[InlineData("Connection refused by host", typeof(ReceiptSendTransportError))]
		[InlineData("HTTP Code: 502 Bad Gateway", typeof(ReceiptSendTransportError))]
		[InlineData("HTTP Code: 504 Gateway Timeout", typeof(ReceiptSendTransportError))]
		[InlineData("BadGateway error occurred", typeof(ReceiptSendTransportError))]
		[InlineData("GatewayTimeout reached", typeof(ReceiptSendTransportError))]
		[InlineData("An error occurred while sending the request.", typeof(ReceiptSendTransportError))]
		public void Classify_WhenTransportError_ReturnsReceiptSendTransportError(string message, Type expectedType)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(expectedType, result);
		}

		[Theory]
		[InlineData("SSL connection could not be established", typeof(ReceiptSendSslError))]
		[InlineData("The SSL connection could not be established, see inner exception. The remote certificate is invalid", typeof(ReceiptSendSslError))]
		[InlineData("SSL certificate validation failed on remote endpoint", typeof(ReceiptSendSslError))]
		public void Classify_WhenSslError_ReturnsReceiptSendSslError(string message, Type expectedType)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(expectedType, result);
		}

		[Theory]
		[InlineData("HTTP Code: 400", typeof(ReceiptSendHttpBadRequest))]
		[InlineData("400 BadRequest: Invalid parameters", typeof(ReceiptSendHttpBadRequest))]
		public void Classify_WhenBadRequest_ReturnsReceiptSendHttpBadRequest(string message, Type expectedType)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(expectedType, result);
		}

		[Theory]
		[InlineData("HTTP Code: 404", typeof(ReceiptSendDocumentStatusNotFound))]
		[InlineData("404 NotFound", typeof(ReceiptSendDocumentStatusNotFound))]
		[InlineData("Не удалось получить актуальный статус чека", typeof(ReceiptSendDocumentStatusNotFound))]
		public void Classify_WhenDocumentNotFound_ReturnsReceiptSendDocumentStatusNotFound(string message, Type expectedType)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(expectedType, result);
		}

		[Theory]
		[InlineData("Status: FAILED. Ошибка ККТ", typeof(ReceiptFiscalizationFailed))]
		[InlineData("Статус: FAILED. Ошибка ФН", typeof(ReceiptFiscalizationFailed))]
		[InlineData("Фискализация отклонена кассой", typeof(ReceiptFiscalizationFailed))]
		[InlineData("MarkingCheckViolated: Код не прошел проверку", typeof(ReceiptFiscalizationFailed))]
		[InlineData("Ожидание фискализации превысило лимит", typeof(ReceiptFiscalizationFailed))]
		public void Classify_WhenConfirmedFiscalizationFailure_ReturnsReceiptFiscalizationFailed(string message, Type expectedType)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(expectedType, result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("Some random unknown unhandled error")]
		public void Classify_WhenUnknownOrEmpty_ReturnsDefaultReceiptSendingFailed(string message)
		{
			// Act
			var result = ReceiptSendFailureClassifier.Classify(message);

			// Assert
			Assert.Equal(typeof(ReceiptSendingFailed), result);
		}

		[Fact]
		public void Classify_WhenSslOrNetworkErrorMentionsFailed_DoesNotClassifyAsFiscalizationFailed()
		{
			// Arrange
			var sslFailedMessage = "SSL connection failed due to certificate expiry";
			var networkFailedMessage = "HttpClient.Timeout: request failed to connect";

			// Act
			var sslResult = ReceiptSendFailureClassifier.Classify(sslFailedMessage);
			var networkResult = ReceiptSendFailureClassifier.Classify(networkFailedMessage);

			// Assert
			Assert.Equal(typeof(ReceiptSendSslError), sslResult);
			Assert.Equal(typeof(ReceiptSendTransportError), networkResult);
			Assert.NotEqual(typeof(ReceiptFiscalizationFailed), sslResult);
			Assert.NotEqual(typeof(ReceiptFiscalizationFailed), networkResult);
		}

		[Fact]
		public void GetSourceName_ReturnsValidName()
		{
			// Act
			var name = ReceiptSendFailureClassifier.GetSourceName(typeof(ReceiptFiscalizationFailed));

			// Assert
			Assert.False(string.IsNullOrWhiteSpace(name));
			Assert.Equal(new ReceiptFiscalizationFailed().Name, name);
		}
	}
}
