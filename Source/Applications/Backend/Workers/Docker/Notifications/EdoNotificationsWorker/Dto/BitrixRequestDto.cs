using System.Text.Json.Serialization;

namespace EdoNotificationsWorker.Dto
{
	/// <summary>
	/// Dto для отпраки в Битрикс <see href="https://apidocs.bitrix24.ru/api-reference/chats/messages/im-message-add.html">Документация</see>.
	/// </summary>
	public class BitrixRequestDto
	{
		/// <summary>
		/// Идентификатор чата в формате:
		/// chatXXX — чат
		/// sgXXX — чат группы или проекта
		/// XXX — идентификатор пользователя личного чата
		/// </summary>
		[JsonPropertyName("DIALOG_ID")]
		public string DialogId { get; set; }

		/// <summary>
		/// Текст сообщения (поддерживает форматирование, см. документацию)
		/// </summary>
		[JsonPropertyName("MESSAGE")]
		public string Message { get; set; }
	}
}
