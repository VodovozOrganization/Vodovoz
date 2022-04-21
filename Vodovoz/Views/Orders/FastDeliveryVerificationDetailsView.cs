using Gamma.ColumnConfig;
using QS.Views.Dialog;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[WindowSize(800, 600)]
	public partial class FastDeliveryVerificationDetailsView : DialogViewBase<FastDeliveryVerificationDetailsViewModel>
	{
		private readonly Gdk.Color _colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
		private readonly Gdk.Color _colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);
		public FastDeliveryVerificationDetailsView(FastDeliveryVerificationDetailsViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			GtkLblDetails.Text = ViewModel.DetailsTitle;
			lblMessage.Binding
				.AddBinding(ViewModel, vm => vm.Message, w => w.LabelProp)
				.InitializeFromSource();

			ConfigureTreeDetails();
		}

		private void ConfigureTreeDetails()
		{
			treeViewDetails.ColumnsConfig = FluentColumnsConfig<FastDeliveryVerificationDetailsNode>.Create()
				.AddColumn("№")
					.AddNumericRenderer(n => ViewModel.Nodes.IndexOf(n) + 1)
				.AddColumn("Марш. лист")
					.AddNumericRenderer(n => n.RouteList.Id)
				.AddColumn("Водитель")
					.AddTextRenderer(n => n.DriverFIO)
				.AddColumn("Хватает\nостатков\nна борту?")
					.AddToggleRenderer(n => n.IsGoodsEnough.ParameterValue)
					.Editing(false)
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.IsGoodsEnough.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Незакрытые\nэкспресс-доставки\nв МЛ")
					.AddNumericRenderer(n => n.UnClosedFastDeliveries.ParameterValue)
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.UnClosedFastDeliveries.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Остаток времени\nна отгрузку\nнового заказа")
					.AddTextRenderer(n => n.RemainingTimeForShipmentNewOrder.ParameterValue.ToString())
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.RemainingTimeForShipmentNewOrder.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Последние\nкоординаты")
					.AddTextRenderer(n => n.LastCoordinateTime.ParameterValue.ToString())
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.LastCoordinateTime.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Расстояние до\nклиента по\nпрямой, км")
					.AddNumericRenderer(n => n.DistanceByLineToClient.ParameterValue)
					.Digits(2)
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.DistanceByLineToClient.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Расстояние до\nклиента по\nдорогам, км")
					.AddNumericRenderer(n => n.DistanceByRoadToClient.ParameterValue)
					.Digits(2)
					.AddSetter((c, n) =>
						c.CellBackgroundGdk = n.DistanceByRoadToClient.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("")
				.Finish();

			treeViewDetails.ItemsDataSource = ViewModel.Nodes;
		}
	}
}
