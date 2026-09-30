using System;
using Edo.Problems.Custom;
using Edo.Problems.Custom.Sources;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// Классифицирует причину ошибки отправки чека по тексту FailureMessage.
	/// </summary>
	public static class ReceiptSendFailureClassifier
	{
		public static Type Classify(string failureMessage)
		{
			if(string.IsNullOrWhiteSpace(failureMessage))
			{
				return typeof(ReceiptSendingFailed);
			}

			if(failureMessage.IndexOf("HTTP Code: 400", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("400 BadRequest", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptSendHttpBadRequest);
			}

			if(failureMessage.IndexOf("HTTP Code: 404", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("404 NotFound", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Не удалось получить актуальный статус чека", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptSendDocumentStatusNotFound);
			}

			if(failureMessage.IndexOf("SSL connection could not be established", StringComparison.OrdinalIgnoreCase) >= 0
				|| (failureMessage.IndexOf("SSL", StringComparison.OrdinalIgnoreCase) >= 0
					&& failureMessage.IndexOf("certificate", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				return typeof(ReceiptSendSslError);
			}

			if(failureMessage.IndexOf("HttpClient.Timeout", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Connection refused", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("HTTP Code: 502", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("HTTP Code: 504", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("BadGateway", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("GatewayTimeout", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("An error occurred while sending the request", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptSendTransportError);
			}

			return typeof(ReceiptSendingFailed);
		}

		public static string GetSourceName(Type problemSourceType)
		{
			if(problemSourceType == null)
			{
				throw new ArgumentNullException(nameof(problemSourceType));
			}

			if(!typeof(EdoTaskProblemCustomSource).IsAssignableFrom(problemSourceType)
				|| problemSourceType.IsAbstract)
			{
				throw new ArgumentException(
					$"Тип {problemSourceType.Name} должен быть наследником {nameof(EdoTaskProblemCustomSource)}",
					nameof(problemSourceType));
			}

			var instance = (EdoTaskProblemCustomSource)Activator.CreateInstance(problemSourceType);
			return instance.Name;
		}
	}
}
