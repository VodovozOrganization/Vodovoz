using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public interface IOperatorAgentFactory
	{
		OperatorServerAgent CreateOperatorAgent(Operator @operator);
	}
}
