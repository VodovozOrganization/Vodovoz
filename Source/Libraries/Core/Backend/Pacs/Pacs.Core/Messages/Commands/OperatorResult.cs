using Pacs.Core.Messages.Events;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Commands
{
	public class OperatorResult : CommandResult
	{
		public OperatorResult()
		{
		}

		public OperatorResult(OperatorStateEvent actualState)
		{
			OperatorState = actualState;
			Result = Result.Success;
		}

		public OperatorResult(OperatorStateEvent actualState, string failureDescription)
		{
			OperatorState = actualState;
			Result = Result.Failure;
			FailureDescription = failureDescription;
		}

		public OperatorStateEvent OperatorState { get; set; }
	}
}
