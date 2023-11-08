namespace Pacs.Server
{
	public interface IOperatorControllerFactory
	{
		OperatorController CreateOperatorController(int operatorId);
	}
}
