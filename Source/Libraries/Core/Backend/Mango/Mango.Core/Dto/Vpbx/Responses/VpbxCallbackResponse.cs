using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ ВАТС на команду обратного звонка (POST /vpbx/commands/callback)
	/// </summary>
	public class VpbxCallbackResponse : VpbxResponseBase
	{
		/// <summary>
		/// Идентификатор команды, переданный в запросе.
		/// Сейчас не используется: команда отправляется без ожидания результата звонка.
		/// Понадобится, если появится обработка асинхронного уведомления
		/// о результате команды, где звонок сопоставляется с командой по этому полю
		/// </summary>
		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }
	}
}
