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

			// 1. Транспортные и сетевые сбои
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

			// 2. SSL ошибки
			if(failureMessage.IndexOf("SSL connection could not be established", StringComparison.OrdinalIgnoreCase) >= 0
				|| (failureMessage.IndexOf("SSL", StringComparison.OrdinalIgnoreCase) >= 0
					&& failureMessage.IndexOf("certificate", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				return typeof(ReceiptSendSslError);
			}

			// 3. Ошибки параметров запроса (400)
			if(failureMessage.IndexOf("HTTP Code: 400", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("400 BadRequest", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptSendHttpBadRequest);
			}

			// 4. Документ не найден на кассе (404)
			if(failureMessage.IndexOf("HTTP Code: 404", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("404 NotFound", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Не удалось получить актуальный статус чека", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptSendDocumentStatusNotFound);
			}

			// 5. Ошибки фискализации и Честного Знака (точный статус кассы)
			if(failureMessage.IndexOf("MarkingCheckViolated", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Ожидание фискализации превысило", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Status: FAILED", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Статус: FAILED", StringComparison.OrdinalIgnoreCase) >= 0
				|| failureMessage.IndexOf("Фискализация отклонена", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return typeof(ReceiptFiscalizationFailed);
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
