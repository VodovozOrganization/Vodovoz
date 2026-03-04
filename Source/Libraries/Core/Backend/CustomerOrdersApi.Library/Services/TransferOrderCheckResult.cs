namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Результат операции переноса заказа
	/// </summary>
	/// <param name="IsSuccess"> Успешно ли выполнена операция </param>
	/// <param name="StatusCode"> HTTP статус код </param>
	/// <param name="Title"> Сообщение результата </param>
	/// <param name="DetailMessage"> Детальное описание ошибки </param>
	public record TransferOrderResult(bool IsSuccess, int StatusCode, string Title, string DetailMessage);
}
