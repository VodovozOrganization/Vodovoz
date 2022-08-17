using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class RouteListMileageDistributionView : TabViewBase<RouteListMileageDistributionViewModel>
	{
		private static Gdk.Color clr = new Gdk.Color(0xee, 0x66, 0x66);
		public RouteListMileageDistributionView(RouteListMileageDistributionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			var colorBlue = new Gdk.Color(0xa8, 0xe8, 0xff);// new Gdk.Color(0x1a, 0xb8, 0xf1);
			var colorYellow = new Gdk.Color(0xf9, 0xfa, 0xb1);// new Gdk.Color(0xf5, 0xf7, 0x77);

			ylabelDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.Date.ToShortDateString(), w => w.Text).InitializeFromSource();
			ylabelCar.Binding.AddFuncBinding(ViewModel.Entity.Car, c => $"{c.CarModel.Title} ({c.RegistrationNumber})", w => w.Text).InitializeFromSource();
			yspinbuttonConfirmedMileageAtDay.Binding.AddBinding(ViewModel, vm => vm.TotalConfirmedDistanceAtDay, w => w.ValueAsDecimal).InitializeFromSource();
			yspinbuttonConfirmedMileageAtDay.ValueChanged += (s,e) => 
				ytreeviewMiliageDistribution.YTreeModel.EmitModelChanged();
			ybuttonDistribute.Clicked += (sender, args) => ViewModel.DistributeCommand.Execute();
			ybuttonSave.Clicked += (sender, args) => ViewModel.SaveDistributionCommand.Execute();

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
					.AddTextRenderer(node => node.Forwarder)
				.AddColumn("Пересчитанный\nкилометраж")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddNumericRenderer(node => node.RecalculatedDistance)
					.XAlign(0.5f)
				.AddColumn("Фактический километраж")
					.HeaderAlignment(0.5f)
					.MinWidth(100)
					.AddNumericRenderer(node => node.ConfirmedDistance)
					.Adjustment(new Adjustment(1, 0, 1000000, 0.1, 1, 1))
					.Digits(2)
					.AddSetter((c, n) => 
						{ 
							c.Editable = ViewModel.CanEdit;
							c.Visible = n.RouteList != null;
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
							c.Visible = n.RouteList != null;
							c.BackgroundGdk = colorYellow;
						})
				.AddColumn("")
				//.RowCells()
				.Finish();

			ytreeviewMiliageDistribution.ItemsDataSource = ViewModel.RouteListMileageDistributions;
		}
	}

	internal class RouteLists
	{
	}
}
