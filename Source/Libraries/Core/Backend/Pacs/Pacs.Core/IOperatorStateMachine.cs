using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core
{
	public interface IOperatorStateMachine
	{
		OperatorState OperatorState { get; set; }

		bool CanChangePhone { get; }
		bool CanEndBreak { get; }
		bool CanEndWorkShift { get; }
		bool CanStartBreak { get; }
		bool CanStartWorkShift { get; }
		bool OnWorkshift { get; }
	}
}
