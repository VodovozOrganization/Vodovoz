namespace Pacs.Server
{
	public interface IOperatorAgentFactory
	{
		OperatorAgent CreateOperatorAgent(int operatorId);
	}
}