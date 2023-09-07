using EmailPrepareWorker.SendEmailMessageBuilders;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmailPrepareWorker.Prepares
{
	public class EmailSendMessagePreparer : IEmailSendMessagePreparer
	{
		public async Task<byte[]> PrepareMessage(SendEmailMessageBuilder builder)
		{
			builder.BuildFromContact();
			builder.BuildToContact();
			builder.BuildTemplate();
			await builder.BuildAttachment();
			builder.BuildPayload();

			var serializedMessage = JsonSerializer.Serialize(builder.ResultSendEmailMessage);
			var sendingBody = Encoding.UTF8.GetBytes(serializedMessage);

			return sendingBody;
		}
	}
}
