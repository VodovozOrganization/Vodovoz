using Gamma.GtkWidgets;
using Gtk;
using QS.Views;
using QS.Views.Dialog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class RouteListMileageDistributionView : DialogViewBase<RouteListMileageDistributionViewModel>
	{
		public RouteListMileageDistributionView(RouteListMileageDistributionViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			var colorBlue = GdkColors.InfoBase;
			var colorYellow = GdkColors.WarningBase;

			ylabelDate.Text = ViewModel.Date.ToShortDateString();
			ylabelCar.Text = ViewModel.Car;
			
			yspinbuttonConfirmedMileageAtDay.Binding
				.AddBinding(ViewModel, vm => vm.TotalConfirmedDistanceAtDay, w => w.ValueAsDecimal)
				.InitializeFromSource();
			
			ybuttonAcceptFine.Binding
				.AddBinding(ViewModel, vm => vm.CanAcceptFine, w => w.Sensitive)
				.InitializeFromSource();
			
			ybuttonDistribute.Clicked += (s, a) => ViewModel.DistributeCommand.Execute();
			ybuttonAcceptFine.Clicked += (s, a) => ViewModel.AcceptFineCommand.Execute();
			ybuttonSave.Clicked += (s, a) => ViewModel.SaveDistributionCommand.Execute();

			ytreeviewMiliageDistribution.ColumnsConfig = ColumnsConfigFactory.Create<RouteListMileageDistributionNode>()
				.AddColumn("№ МЛ")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddTextRenderer(node => node.Id)
				.AddColumn("Смена")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddTextRenderer(node => node.DeliveryShift)
				.AddColumn("Водитель")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddTextRenderer(node => node.Driver)
				.AddColumn("Экспедитор")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddTextRenderer(node => node.ForwarderColumn)
				.AddColumn("Пересчитанный\nкилометраж")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddNumericRenderer(node => node.RecalculatedDistanceColumn)
					.XAlign(0.5f)
					.Digits(2)
				.AddColumn("Фактический\nкилометраж по МЛ")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddNumericRenderer(node => node.ConfirmedDistance)
					.Adjustment(new Adjustment(1, 0, (double)RouteList.ConfirmedDistanceLimit, 0.1, 1, 1))
					.Digits(2)
					.AddSetter((c, n) =>
					{
						c.Editable = ViewModel.CanEdit;
						c.Visible = n.IsRouteList;
						c.BackgroundGdk = colorBlue;
					})
					.XAlign(0.5f)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.MinWidth(200)
					.AddTextRenderer(node => node.MileageComment)
					.AddSetter((c, n) =>
					{
						c.Editable = ViewModel.CanEdit;
						c.Visible = n.IsRouteList;
						c.BackgroundGdk = colorYellow;
					})
				.AddColumn("")
				.Finish();

			ytreeviewMiliageDistribution.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Rows, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedNode, w => w.SelectedRow)
				.InitializeFromSource();
		}
	}
}
