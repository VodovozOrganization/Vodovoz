using System.Collections.Generic;
using DriverApi.Contracts.V6;

namespace DriverApi.Contracts.V6.Responses
{
	/// <summary>
	/// Ответ на запрос проверки кода Честного Знака
	/// </summary>
	public class CheckCodeResultResponse
	{
		/// <summary>
		/// Результат обработки запроса
		/// </summary>
		public RequestProcessingResultTypeDto Result { get; set; }

		/// <summary>
		/// Ошибка при обработке запроса
		/// </summary>
		public string Error { get; set; }

		/// <summary>
		/// Список кодов Честного Знака
		/// </summary>
		public List<TrueMarkCodeDto> Codes { get; set; }
	}
}
