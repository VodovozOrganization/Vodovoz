using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Команда обратного звонка (POST /vpbx/commands/callback).
	/// ВАТС сначала дозванивается инициатору <see cref="From"/>,
	/// а после ответа соединяет его с <see cref="ToNumber"/>
	/// </summary>
	public class MakeVpbxCallbackRequest
	{
		/// <summary>
		/// Идентификатор команды. Обязательное поле, не длиннее 128 символов.
		/// Возвращается в ответе и в асинхронном уведомлении о результате команды
		/// </summary>
		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }

		/// <summary>
		/// Инициатор звонка. Обязательное поле
		/// </summary>
		[JsonPropertyName("from")]
		public VpbxCallbackFrom From { get; set; }

		/// <summary>
		/// Номер вызываемого абонента. Обязательное поле, не длиннее 128 символов
		/// </summary>
		[JsonPropertyName("to_number")]
		public string ToNumber { get; set; }
	}
}
