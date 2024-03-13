namespace Vodovoz.FirebaseCloudMessaging.Client.Responses
{
	/// <summary>
	/// Ответ на запрос об отправке сообщения
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#resource:-message"/>
	/// </summary>
	public class MessageResponse
	{
		/// <summary>
		/// Идентификатор отправленного сообщения в формате projects/*/messages/{message_id} .
		/// </summary>
		public string Name { get; set; }
	}
}
