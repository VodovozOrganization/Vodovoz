namespace Pacs.Operators.Client
{
	public interface IOperatorClientFactory
	{
		IOperatorClient CreateOperatorClient(int operatorId);
	}
}
