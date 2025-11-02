using Pacs.Core.Messages.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operators.Client
{
	public interface IOperatorClient
	{
		int? OperatorId { get; }

		event EventHandler<OperatorStateEvent> StateChanged;
		Task<GlobalBreakAvailabilityEvent> GetGlobalBreakAvailability();
		Task<OperatorsOnBreakEvent> GetOperatorsOnBreak();

		Task<OperatorStateEvent> Connect(CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> Disconnect(CancellationToken cancellationToken = default);
		Task KeepAlive(CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> StartWorkShift(string phoneNumber);
		Task<OperatorStateEvent> EndWorkShift(string reason = null);
		Task<OperatorStateEvent> ChangeNumber(string phoneNumber);
		Task<OperatorStateEvent> StartBreak(OperatorBreakType breakType, CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> EndBreak(CancellationToken cancellationToken = default);
		Task<OperatorBreakAvailability> GetOperatorBreakAvailability(int operatorId);
	}
}
