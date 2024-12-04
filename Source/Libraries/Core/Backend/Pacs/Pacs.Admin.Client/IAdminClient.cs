using Pacs.Core.Messages.Events;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Client
{
	public interface IAdminClient
	{
		Task<OperatorStateEvent> EndBreak(int operatorId, string reason, CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> EndWorkShift(int operatorId, string reason, CancellationToken cancellationToken = default);
		Task<DomainSettings> GetSettings();
		Task SetSettings(DomainSettings settings);
		Task<OperatorStateEvent> StartBreak(int operatorId, string reason, OperatorBreakType breakType, CancellationToken cancellationToken = default);
	}
}
