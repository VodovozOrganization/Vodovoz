namespace Vodovoz.Settings.Employee
{
	public interface IEmployeeSettings
	{
		int DefaultEmployeeRegistrationVersionId { get; }
		int WorkingClothesFineTemplateId { get; }
		int MaxDaysForNewbieDriver { get; }
		int DefaultEmployeeForCallTask { get; }
		int DefaultEmployeeForDepositReturnTask { get; }
	}
}
