namespace Vodovoz.Parameters
{
	public interface IExpenseParametersProvider
	{
		int DefaultChangeOrganizationId { get; }
		int ChangeCategoryId { get; }
		int DefaultExpenseOrganizationId { get; }
	}
}
