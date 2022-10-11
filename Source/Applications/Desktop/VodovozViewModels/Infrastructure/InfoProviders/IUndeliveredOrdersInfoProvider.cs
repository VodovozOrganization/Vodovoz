using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.ViewModels.Infrastructure.InfoProviders
{
	public interface IUndeliveredOrdersInfoProvider : IInfoProvider
	{
		UndeliveredOrdersFilterViewModel UndeliveredOrdersFilterViewModel { get; }
	}
}
