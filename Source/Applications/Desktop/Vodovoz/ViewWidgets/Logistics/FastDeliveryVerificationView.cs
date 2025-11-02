using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FastDeliveryVerificationView : WidgetViewBase<FastDeliveryVerificationViewModel>
	{
		private readonly Gdk.Color _colorWhite = GdkColors.PrimaryBase;
		private readonly Gdk.Color _colorLightRed = GdkColors.DangerBase;
		public FastDeliveryVerificationView(FastDeliveryVerificationViewModel viewModel) : base(viewModel)
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

			ytextviewLogisticiaComment.Binding.AddBinding(ViewModel.FastDeliveryAvailabilityHistory, h => h.LogisticianComment, w => w.Buffer.Text).InitializeFromSource();

			yvboxLogisticianComment.Binding.AddBinding(ViewModel, vm => vm.ShowLogisticianComment, w => w.Visible).InitializeFromSource();

			ybuttonSaveLogisticianComment.Clicked += (sender, args) => ViewModel.SaveLogisticianCommentCommand.Execute();

			ConfigureTreeDetails();
			ConfigureOrderItems();
		}

		private void ConfigureTreeDetails()
		{
			treeViewDetails.ColumnsConfig = FluentColumnsConfig<FastDeliveryVerificationDetailsNode>.Create()
				.AddColumn("№")
				.AddNumericRenderer(n => ViewModel.Nodes.IndexOf(n) + 1)
				.AddColumn("Марш. лист")
				.AddNumericRenderer(n => n.RouteList.Id)
				.AddColumn("Радиус")
				.AddNumericRenderer(n => $"{n.RouteListFastDeliveryRadius:N1}")
				.AddColumn("Водитель")
				.AddTextRenderer(n => n.DriverFIO)
				.AddColumn("Хватает\nостатков\nна борту?")
				.AddToggleRenderer(n => n.IsGoodsEnough.ParameterValue)
				.Editing(false)
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.IsGoodsEnough.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Незакрытые\nэкспресс-доставки\nв МЛ")
				.AddTextRenderer(n => $"{n.UnClosedFastDeliveries.ParameterValue}/{n.RouteListMaxFastDeliveryOrders}").XAlign(0.5f)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.UnClosedFastDeliveries.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Остаток времени\nна отгрузку\nнового заказа")
				.AddTextRenderer(n => n.RemainingTimeForShipmentNewOrder.ParameterValue.ToString())
				.AddSetter((c, n) =>
					c.CellBackgroundGdk = n.RemainingTimeForShipmentNewOrder.IsValidParameter ? _colorWhite : _colorLightRed)
				.AddColumn("Последние\nкоординаты")
				.AddTextRenderer(n => $"{n.LastCoordinateTime.ParameterValue.TotalHours:00}:{n.LastCoordinateTime.ParameterValue.Minutes:00}")
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

		private void ConfigureOrderItems()
		{
			treeViewNomenclatures.ColumnsConfig = FluentColumnsConfig<FastDeliveryOrderItemHistory>.Create()
				.AddColumn("Товар").HeaderAlignment(0.5f).AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во").MinWidth(75).HeaderAlignment(0.5f).AddNumericRenderer(node => node.Count)
				.AddColumn("")
				.Finish();

			treeViewNomenclatures.ItemsDataSource = ViewModel.OrderItemsHistory;
		}
	}
}
