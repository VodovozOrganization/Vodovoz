namespace Pacs.Server
{
	public interface IOperatorAgentFactory
	{
		OperatorServerAgent CreateOperatorAgent(int operatorId);
	}
}