using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorAgentFactory
	{
		OperatorServerStateMachine CreateOperatorAgent(int operatorId);
	}
}
