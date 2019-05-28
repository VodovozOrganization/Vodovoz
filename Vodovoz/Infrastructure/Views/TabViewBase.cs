using System;
using QS.Dialog.GtkUI;
using QS.Tdi;
using Vodovoz.Infrastructure.ViewModels;

namespace Vodovoz.Infrastructure.Views
{
	public abstract class TabViewBase : Gtk.Bin, ITabView
	{
		public virtual ITdiTab Tab { get; }
	}

	public abstract class TabViewBase<TViewModel> : TabViewBase
		where TViewModel : TabViewModelBase
	{
		public TViewModel ViewModel { get; set; }

		public sealed override ITdiTab Tab => ViewModel;

		public TabViewBase(TViewModel viewModel)
		{
			ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
		}
	}
}
