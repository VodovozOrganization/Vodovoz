using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Идентификаторы статуса позиции в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartLineStatusIds
	{
		/// <summary>
		/// Внешний идентификатор статуса
		/// </summary>
		[JsonPropertyName("externalId")]
		public string ExternalId { get; set; }
	}
}
