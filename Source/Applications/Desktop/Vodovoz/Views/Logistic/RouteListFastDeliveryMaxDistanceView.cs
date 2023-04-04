using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Sale;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListFastDeliveryMaxDistanceView : TabViewBase<RouteListFastDeliveryMaxDistanceViewModel>
	{
		public RouteListFastDeliveryMaxDistanceView(RouteListFastDeliveryMaxDistanceViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yspinbuttonDistance.Binding.AddBinding(ViewModel, e => e.FastDeliveryMaxDistance, w => w.ValueAsDecimal).InitializeFromSource();

			yTreeViewDistanceHistory.ColumnsConfig = ColumnsConfigFactory.Create<RouteListFastDeliveryMaxDistance>()
				.AddColumn("Дата начала:").AddTextRenderer(d => d.StartDate.ToShortDateString())
				.AddColumn("Дата окончания").AddTextRenderer (d => (d.EndDate.HasValue) ? d.EndDate.Value.ToShortDateString() : "-")
				.AddColumn("Радиус, км").AddTextRenderer(d => $"{d.Distance:N1}")
				.AddColumn("")
				.Finish();
			yTreeViewDistanceHistory.ItemsDataSource = ViewModel.Entity.FastDeliveryMaxDistanceItems;

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
