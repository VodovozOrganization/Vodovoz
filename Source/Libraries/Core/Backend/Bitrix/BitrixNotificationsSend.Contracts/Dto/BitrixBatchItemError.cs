using System;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Ошибка выполнения одной из команд пакетного запроса в Битрикс24
	/// </summary>
	public class BitrixBatchItemError
	{
		/// <summary>
		/// Код ошибки превышения операционного лимита Битрикс24 из документации REST API Битрикс24
		/// </summary>
		private const string _operationTimeLimitErrorCode = "OPERATION_TIME_LIMIT";

		/// <summary>
		/// Ключ команды в пакете
		/// </summary>
		public string CommandKey { get; set; }

		/// <summary>
		/// Код ошибки из ответа Битрикс24
		/// </summary>
		public string ErrorCode { get; set; }

		/// <summary>
		/// Описание ошибки из ответа Битрикс24
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Признак того, что команда не выполнена из-за превышения операционного лимита Битрикс24
		/// (суммарного времени выполнения метода в 10-минутном окне).
		/// Такие команды можно отправить повторно после освобождения бюджета
		/// </summary>
		public bool IsOperatingLimitError =>
			string.Equals(ErrorCode, _operationTimeLimitErrorCode, StringComparison.OrdinalIgnoreCase);
	}
}
