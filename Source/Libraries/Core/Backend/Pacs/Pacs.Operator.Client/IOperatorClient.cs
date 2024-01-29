using Pacs.Core.Messages.Events;
using Pacs.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pacs.Operators.Client
{
	public interface IOperatorClient
	{
		int OperatorId { get; }

		event EventHandler<OperatorStateEvent> StateChanged;
		Task<GlobalBreakAvailability> GetGlobalBreakAvailability();
		Task<OperatorsOnBreakEvent> GetOperatorsOnBreak();

		Task<OperatorStateEvent> Connect(CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> Disconnect(CancellationToken cancellationToken = default);
		Task KeepAlive(CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> StartWorkShift(string phoneNumber);
		Task<OperatorStateEvent> EndWorkShift(string reason = null);
		Task<OperatorStateEvent> ChangeNumber(string phoneNumber);
		Task<OperatorStateEvent> StartBreak(OperatorBreakType breakType, CancellationToken cancellationToken = default);
		Task<OperatorStateEvent> EndBreak(CancellationToken cancellationToken = default);
	}
}
