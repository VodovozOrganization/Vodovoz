using System.Threading.Tasks;
using EmailPrepareWorker.SendEmailMessageBuilders;

namespace EmailPrepareWorker.Prepares
{
	public interface IEmailSendMessagePreparer
	{
		byte[] PrepareMessage(SendEmailMessageBuilder builder);
	}
}
