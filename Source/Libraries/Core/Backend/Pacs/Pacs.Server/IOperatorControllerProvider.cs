namespace Pacs.Server
{
	public interface IOperatorControllerProvider
	{
		OperatorController GetOperatorController(int operatorId);
		OperatorController GetOperatorController(string phoneNumber);
	}
}
