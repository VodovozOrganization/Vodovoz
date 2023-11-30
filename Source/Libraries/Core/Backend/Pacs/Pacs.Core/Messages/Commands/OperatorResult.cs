using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Commands
{
	public class OperatorResult : CommandResult
	{
		public OperatorResult()
		{
		}

		public OperatorResult(OperatorState actualState)
		{
			OperatorState = actualState;
			Result = Result.Success;
		}

		public OperatorResult(OperatorState actualState, string failureDescription)
		{
			OperatorState = actualState;
			Result = Result.Failure;
			FailureDescription = failureDescription;
		}

		public OperatorState OperatorState { get; set; }
	}
}
