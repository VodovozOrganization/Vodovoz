using Gtk;
using QS.ViewModels;

namespace Vodovoz.Core
{
	public interface IWidgetResolver
	{
		Widget Resolve(WidgetViewModelBase viewModel);
	}
}