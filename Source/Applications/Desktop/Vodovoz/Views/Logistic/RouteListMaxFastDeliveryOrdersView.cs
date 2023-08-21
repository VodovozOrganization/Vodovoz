using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;

using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMaxFastDeliveryOrdersView : TabViewBase<RouteListMaxFastDeliveryOrdersViewModel>
	{
		public RouteListMaxFastDeliveryOrdersView(RouteListMaxFastDeliveryOrdersViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yspinbuttonMaxOrders.Binding.AddBinding(ViewModel, e => e.MaxFastDeliveryOrders, w => w.ValueAsInt)
				.InitializeFromSource();

			yTreeViewMaxOrdersHistory.ColumnsConfig = ColumnsConfigFactory.Create<RouteListMaxFastDeliveryOrders>()
				.AddColumn("Макс. кол-во заказов ДЗЧ").AddNumericRenderer(m => m.MaxOrders)
				.AddColumn("Дата начала").AddTextRenderer(m => $"{m.StartDate:dd.MM.yyyy HH:mm} ")
				.AddColumn("Дата окончания").AddTextRenderer(m => (m.EndDate.HasValue) ? $"{m.EndDate.Value:dd.MM.yyyy HH:mm}" : "-")
				.AddColumn("")
				.Finish();
			yTreeViewMaxOrdersHistory.ItemsDataSource = ViewModel.Entity.MaxFastDeliveryOrdersItems;

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}
	}
}
