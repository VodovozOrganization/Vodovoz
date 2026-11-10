using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Статус позиции в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartLineStatus
	{
		/// <summary>
		/// Идентификаторы статуса
		/// </summary>
		[JsonPropertyName("ids")]
		public MindboxAuthorizedCartLineStatusIds Ids { get; set; }
	}
}
