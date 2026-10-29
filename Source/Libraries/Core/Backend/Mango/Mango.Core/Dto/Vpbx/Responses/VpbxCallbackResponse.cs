using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ ВАТС на команду обратного звонка
	/// </summary>
	public class VpbxCallbackResponse : VpbxResponseBase
	{
		/// <summary>
		/// Идентификатор команды, переданный в запросе
		/// </summary>
		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }
	}
}
