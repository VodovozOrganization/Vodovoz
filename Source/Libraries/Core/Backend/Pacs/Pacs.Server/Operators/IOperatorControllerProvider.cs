namespace Pacs.Server.Operators
{
	public interface IOperatorControllerProvider
	{
		OperatorController GetOperatorController(int operatorId);
		OperatorController GetOperatorController(string phoneNumber);
	}
}
