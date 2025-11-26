using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	/// <summary>
	/// Описание отчета об обратной связи (Feedback Loop Report)
	/// </summary>
	public class FblReportDto
	{
		/// <summary>
		/// Дата получения FBL-отчета
		/// </summary>
		[JsonPropertyName("date")]
		public string Date { get; set; }

		/// <summary>
		/// Домен, от которого поступил FBL-отчет
		/// </summary>
		[JsonPropertyName("domain")]
		public string Domain { get; set; }
	}
}
