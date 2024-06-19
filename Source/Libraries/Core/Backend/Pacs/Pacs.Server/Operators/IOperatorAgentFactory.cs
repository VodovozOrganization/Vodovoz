using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorAgentFactory
	{
		OperatorServerAgent CreateOperatorAgent(int operatorId);
	}
}
