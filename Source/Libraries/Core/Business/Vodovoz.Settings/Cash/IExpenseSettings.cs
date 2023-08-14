namespace Vodovoz.Settings.Cash
{
	public interface IExpenseSettings
	{
		int DefaultChangeOrganizationId { get; }
		int DefaultExpenseOrganizationId { get; }
	}
}
