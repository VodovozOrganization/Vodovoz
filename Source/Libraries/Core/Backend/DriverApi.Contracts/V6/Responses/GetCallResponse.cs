namespace DriverApi.Contracts.V6.Responses
{
	/// <summary>
	/// Ответ на запрос звонка клиенту. Содержит таймаут ожидания звонка водителем в секундах
	/// </summary>
	public class GetCallResponse
	{
		/// <summary>
		/// Таймаут ожидания звонка водителем в секундах
		/// На это время должна блокироваться кнопка "Позвонить" в приложении водителя
		/// </summary>
		public int TimeOut { get; set; }
	}
}
