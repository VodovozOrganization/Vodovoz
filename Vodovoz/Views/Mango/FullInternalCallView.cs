using System;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class FullInternalCallView : DialogViewBase<FullInternalCallViewModel>
	{
		public FullInternalCallView(FullInternalCallViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			foreach(var item in ViewModel.GetWidgetPages()) {
				var label = new Gtk.Label(item.Key);
				WidgetPlace.AppendPage(item.Value, label);
				WidgetPlace.ShowAll();
			}
			WidgetPlace.ChangeCurrentPage += ChangeCurrentPage_WidgetPlace;
		}
		public void Refresh()
		{
		}
		#region Events
		private void ChangeCurrentPage_WidgetPlace(object sender,EventArgs e)
		{
			//WidgetPlace.Page
		}
		#endregion
	}
}
