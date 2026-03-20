using System.Xml.Serialization;
using Gamma.Utilities;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Инфа о возврате денежных средств по платежу
	/// </summary>
	public class ReverseTicketResponseDTO
	{
		/// <summary>
		/// Пустой конструктор
		/// </summary>
		public ReverseTicketResponseDTO()
		{
		}

		/// <summary>
		/// Конструктор, принимающий статус ответа апи банка
		/// </summary>
		/// <param name="status">статус ответа</param>
		public ReverseTicketResponseDTO(ResponseStatus status)
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
		public ReverseTicketResponseDTO(string errorMessage)
		{
			ResponseStatus = ResponseStatus.UnknownError;
			ErrorMessage = errorMessage;
		}

		/// <summary>
		/// Сообщение ответа от банка (для XML-десериализации)
		/// </summary>
		[XmlElement("response_message")]
		public string ResponseMessage { get; set; }

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
