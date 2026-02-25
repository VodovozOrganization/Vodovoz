using Gamma.Utilities;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Инфа об отмене платежа
	/// </summary>
	public class CancelTicketResponseDTO
	{
		/// <summary>
		/// Пустой конструктор
		/// </summary>
		public CancelTicketResponseDTO()
		{
		}

		/// <summary>
		/// Конструктор, принимающий статус ответа апи банка
		/// </summary>
		/// <param name="status">статус ответа</param>
		public CancelTicketResponseDTO(ResponseStatus status)
		{
			ResponseStatus = status;
			if(status != ResponseStatus.Success)
			{
				ErrorMessage = status.GetEnumTitle();
			}
		}

		/// <summary>
		/// Регистрация неизвестной ошибки с описанием
		/// </summary>
		/// <param name="errorMessage">сообщение об ошибке</param>
		public CancelTicketResponseDTO(string errorMessage)
		{
			ResponseStatus = ResponseStatus.UnknownError;
			ErrorMessage = errorMessage;
		}

		/// <summary>
		/// Статус ответа
		/// </summary>
		public ResponseStatus ResponseStatus { get; }
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; }
	}
}
