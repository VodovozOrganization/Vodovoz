using System.Text.Json.Serialization;

namespace Vodovoz.Mindbox.Contracts
{
	/// <summary>
	/// Сообщение валидации Mindbox
	/// </summary>
	public sealed class MindboxValidationMessage
	{
		[JsonPropertyName("message")]
		public string Message { get; set; }

		[JsonPropertyName("location")]
		public string Location { get; set; }
	}
}
