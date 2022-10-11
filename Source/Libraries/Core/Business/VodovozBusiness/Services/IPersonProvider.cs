namespace Vodovoz.Services
{
	public interface IPersonProvider
	{
		int GetDefaultEmployeeForCallTask();

		int GetDefaultEmployeeForDepositReturnTask();
	}
}
