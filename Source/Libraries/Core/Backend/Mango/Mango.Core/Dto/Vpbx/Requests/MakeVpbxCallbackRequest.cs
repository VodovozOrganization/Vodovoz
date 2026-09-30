using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// DTO запроса команды обратного звонка
	/// ВАТС сначала дозванивается инициатору <see cref="From"/>,
	/// а после ответа соединяет его с <see cref="ToNumber"/>
	/// </summary>
	public class MakeVpbxCallbackRequest
	{
		/// <summary>
		/// Идентификатор команды. Обязательное поле, не длиннее 128 символов
		/// </summary>
		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }

		/// <summary>
		/// Инициатор звонка
		/// </summary>
		[JsonPropertyName("from")]
		public VpbxCallbackFrom From { get; set; }

		/// <summary>
		/// Номер вызываемого абонента в формате 7XXXXXXXXXX (11 цифр, начиная с 7)
		/// </summary>
		[JsonPropertyName("to_number")]
		public string ToNumber { get; set; }
	}
}
