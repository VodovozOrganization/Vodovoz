using CustomerOrders.Contracts.V8.Sale.Templates;

namespace CustomerOrdersApi.Library.V8.Factories
{
	public interface ITemplateActionsGenerator
	{
		OrderTemplateActionsDto Generate(TemplateStatusDto status);
	}
}
