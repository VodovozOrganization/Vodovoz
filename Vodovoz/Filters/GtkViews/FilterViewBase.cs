using System;
namespace Vodovoz.Filters.GtkViews
{
	public class FilterViewBase<TFilter> : Gtk.Bin
		where TFilter : FilterViewModelBase<TFilter>
	{
		protected TFilter ViewModel { get; set; }

		public FilterViewBase()
		{
		}

		public override void Destroy()
		{
			if(ViewModel != null) {
				ViewModel.Dispose();
			}
			base.Destroy();
		}
	}
}
