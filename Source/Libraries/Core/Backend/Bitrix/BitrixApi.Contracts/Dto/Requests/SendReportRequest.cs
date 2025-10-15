using System.Text.Json.Serialization;

namespace BitrixApi.Contracts.Dto.Requests
{
	/// <summary>
	/// Dto запроса на отправку отчета контрагенту
	/// </summary>
	public class SendReportRequest
	{
		/// <summary>
		/// ИНН контрагента
		/// </summary>
		[JsonPropertyName("counterpartyInn")]
		[JsonRequired]
		public string CounterpartyInn { get; set; }
		/// <summary>
		/// Организация Id
		/// </summary>
		[JsonPropertyName("organization")]
		public int OrganizationId { get; set; }
		/// <summary>
		/// Адрес электронной почты получателя
		/// </summary>
		[JsonPropertyName("emailAdress")]
		public string EmailAdress { get; set; }
		/// <summary>
		/// Тип отчета
		/// </summary>
		[JsonPropertyName("reportType")]
		public ReportTypeDto ReportType { get; set; }
	}
}
