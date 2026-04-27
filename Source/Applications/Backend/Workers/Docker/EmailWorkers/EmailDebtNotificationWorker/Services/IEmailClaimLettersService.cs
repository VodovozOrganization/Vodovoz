using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services
{
	public interface IEmailClaimLettersService
	{
		Task SendClaimLetters(CancellationToken cancellationToken);
	}
}