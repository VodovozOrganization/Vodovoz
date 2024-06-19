using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorServerStateMachineFactory
	{
		OperatorServerStateMachine CreateOperatorAgent(int operatorId);
	}
}
