namespace TrueMark.Api.Contracts.Requests
{
	/// <summary>
	/// Отправка документа в ЧЗ
	/// </summary>
	public class SendDocumentDataRequest
	{
		/// <summary>
		/// Документ
		/// </summary>
		public string Document { get; set; }
		/// <summary>
		/// Инн организации
		/// </summary>
		public string Inn { get; set; }
	}
}
