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
			Operator = actualState;
			Result = Result.Success;
		}

		public OperatorResult(OperatorState actualState, string failureDescription)
		{
			Operator = actualState;
			Result = Result.Failure;
			FailureDescription = failureDescription;
		}

		public OperatorState Operator { get; set; }
	}
}
