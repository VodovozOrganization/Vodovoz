namespace Pacs.Server.Operators
{
	public interface IOperatorStateService
	{
		OperatorController GetOperatorController(int operatorId);
		OperatorController GetOperatorController(string phoneNumber);
	}
}
