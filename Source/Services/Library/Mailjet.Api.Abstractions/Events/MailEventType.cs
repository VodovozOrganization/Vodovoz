using System.Text.Json.Serialization;

namespace Mailjet.Api.Abstractions.Events
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum MailEventType
	{
		sent,
		open,
		click,
		bounce,
		blocked,
		spam,
		unsub
	}
}
