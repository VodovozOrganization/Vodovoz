namespace Vodovoz.Parameters
{
	public interface IExpenseParametersProvider
	{
		string ChangeCategoryName { get; }
		int DefaultChangeOrganizationId { get; }
	}
}
