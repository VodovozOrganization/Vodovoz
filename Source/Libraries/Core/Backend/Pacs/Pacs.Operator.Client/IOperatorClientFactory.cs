namespace Pacs.Operator.Client
{
	public interface IOperatorClientFactory
	{
		IOperatorClient CreateOperatorClient(int operatorId);
	}
}
