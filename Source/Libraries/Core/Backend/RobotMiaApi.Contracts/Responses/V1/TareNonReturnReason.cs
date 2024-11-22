using System.Text.Json.Serialization;

namespace RobotMiaApi.Contracts.Responses.V1
{
	/// <summary>
	/// Причина не возврата тары
	/// </summary>
	public class TareNonReturnReasonDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }
	}
}
