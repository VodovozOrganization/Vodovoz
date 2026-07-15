using System;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Ошибка создания сделки по одной из команд пакетного запроса в Битрикс24
	/// </summary>
	public class PlannedOrderDealCreationError
	{
		/// <summary>
		/// Код ошибки превышения операционного лимита Битрикс24,
		/// см. раздел "Статусы и коды системных ошибок" документации REST API
		/// </summary>
		private const string _operationTimeLimitErrorCode = "OPERATION_TIME_LIMIT";

		/// <summary>
		/// Ключ команды в пакете, содержит id контрагента и точки доставки (или self для самовывоза)
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
		/// Признак того, что сделка не создана из-за превышения операционного лимита Битрикс24
		/// (суммарного времени выполнения метода в 10-минутном окне).
		/// Такие сделки можно отправить повторно после освобождения бюджета
		/// </summary>
		public bool IsOperatingLimitError =>
			string.Equals(ErrorCode, _operationTimeLimitErrorCode, StringComparison.OrdinalIgnoreCase);
	}
}
