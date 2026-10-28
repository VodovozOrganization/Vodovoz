using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Ответ Mindbox на операцию
	/// </summary>
	public sealed class MindboxResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }

		[JsonPropertyName("errorId")]
		public string ErrorId { get; set; }

		[JsonPropertyName("httpStatusCode")]
		public string HttpStatusCode { get; set; }

		[JsonPropertyName("validationMessages")]
		public List<MindboxValidationMessage> ValidationMessages { get; set; }
	}
}
