using System.Threading.Tasks;
using EmailPrepareWorker.SendEmailMessageBuilders;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailSendMessagePreparer
	{
		Task<byte[]> PrepareMessage(SendEmailMessageBuilder builder);
	}
}
