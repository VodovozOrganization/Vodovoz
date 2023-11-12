using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operator.Client
{
	public interface IOperatorClient
	{
		int OperatorId { get; }

		event EventHandler<OperatorState> StateChanged;
		Task<OperatorState> GetState();

		Task<OperatorState> StartWorkShift(string phoneNumber);
		Task<OperatorState> EndWorkShift();
		Task<OperatorState> ChangeNumber(string phoneNumber);
		Task<OperatorState> StartBreak(CancellationToken cancellationToken = default);
		Task<OperatorState> EndBreak(CancellationToken cancellationToken = default);
	}
}
