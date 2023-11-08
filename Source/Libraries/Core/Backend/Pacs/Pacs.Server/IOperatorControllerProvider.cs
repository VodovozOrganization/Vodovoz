namespace Pacs.Server
{
	public interface IOperatorControllerProvider
	{
		OperatorController GetOperatorController(int operatorId);
	}
}
