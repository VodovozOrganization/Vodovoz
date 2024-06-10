namespace Pacs.Server.Operators
{
	public interface IOperatorControllerFactory
	{
		OperatorController CreateOperatorController(int operatorId);
	}
}
