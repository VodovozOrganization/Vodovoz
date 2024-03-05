using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using QS.Navigation;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class OnlineOrderView : TabViewBase<OnlineOrderViewModel>
	{
		public OnlineOrderView(OnlineOrderViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnGetToWork.Clicked += (sender, args) => ViewModel.GetToWorkCommand.Execute();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnCancelOnlineOrder.Clicked += (sender, args) => ViewModel.CancelOnlineOrderCommand.Execute();
			
			btnGetToWork.Binding
				.AddBinding(ViewModel, vm => vm.CanGetToWork, w => w.Sensitive)
				.InitializeFromSource();
			
			btnCancelOnlineOrder.Binding
				.AddBinding(ViewModel, vm => vm.CanCancelOnlineOrder, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureInfo();
			ConfigureItemsWidgets();
			ConfigureTreeRentWidgets();
		}

		private void ConfigureInfo()
		{
			lblOnlineOrderWarnings.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarnings, w => w.Visible)
				.InitializeFromSource();
			textViewWarnings.Editable = false;
			textViewWarnings.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowWarnings, w => w.Visible)
				.AddBinding(vm => vm.ValidationErrors, w => w.Buffer.Text)
				.InitializeFromSource();
			GtkScrolledWarnings.Visible = ViewModel.CanShowWarnings;
			
			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			lblId.Selectable = true;
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowId, w => w.Visible)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.InitializeFromSource();
			
			lblEmployeeWorkWithTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowEmployeeWorkWith, w => w.Visible)
				.InitializeFromSource();
			lblEmployeeWorkWith.Selectable = true;
			lblEmployeeWorkWith.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowEmployeeWorkWith, w => w.Visible)
				.AddBinding(vm => vm.EmployeeWorkWith, w => w.LabelProp)
				.InitializeFromSource();
			
			lblOrderTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowOrder, w => w.Visible)
				.InitializeFromSource();
			lblOrder.Selectable = true;
			lblOrder.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowOrder, w => w.Visible)
				.AddBinding(vm => vm.Order, w => w.LabelProp)
				.InitializeFromSource();
			
			lblStatus.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderStatusString, w => w.LabelProp)
				.InitializeFromSource();

			lblCounterparty.Selectable = true;
			lblCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.Counterparty, w => w.LabelProp)
				.InitializeFromSource();

			lblDeliveryPoint.Selectable = true;
			lblDeliveryPoint.Binding
				.AddBinding(ViewModel, vm => vm.DeliveryPoint, w => w.LabelProp)
				.InitializeFromSource();

			chkIsSelfDelivery.Sensitive = false;
			chkIsSelfDelivery.Binding
				.AddBinding(ViewModel.Entity, e => e.IsSelfDelivery, w => w.Active)
				.InitializeFromSource();
			
			chkIsNeedConfirmationByCall.Sensitive = false;
			chkIsNeedConfirmationByCall.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNeedConfirmationByCall, w => w.Active)
				.InitializeFromSource();
			
			lblSelfDeliveryGeoGroupTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowSelfDeliveryGeoGroup, w => w.Visible)
				.InitializeFromSource();
			lblSelfDeliveryGeoGroup.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowSelfDeliveryGeoGroup, w => w.Visible)
				.AddBinding(vm => vm.SelfDeliveryGeoGroup, w => w.LabelProp)
				.InitializeFromSource();
			
			lblPaymentType.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderPaymentType, w => w.LabelProp)
				.InitializeFromSource();
			
			lblOnlinePaymentTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowOnlinePayment, w => w.Visible)
				.InitializeFromSource();
			lblOnlinePayment.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowOnlinePayment, w => w.Visible)
				.AddBinding(vm => vm.OnlinePayment, w => w.LabelProp)
				.InitializeFromSource();
			
			lblOnlinePaymentSourceTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowOnlinePaymentSource, w => w.Visible)
				.InitializeFromSource();
			lblOnlinePaymentSource.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowOnlinePaymentSource, w => w.Visible)
				.AddBinding(vm => vm.OnlinePaymentSource, w => w.LabelProp)
				.InitializeFromSource();
			
			lblDeliveryDate.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderDeliveryDate, w => w.LabelProp)
				.InitializeFromSource();
			
			lblDeliverySchedule.Binding
				.AddBinding(ViewModel, vm => vm.DeliverySchedule, w => w.LabelProp)
				.InitializeFromSource();

			chkIsFastDelivery.Sensitive = false;
			chkIsFastDelivery.Binding
				.AddBinding(ViewModel.Entity, e => e.IsFastDelivery, w => w.Active)
				.InitializeFromSource();

			textViewOnlineOrderComment.Editable = false;
			textViewOnlineOrderComment.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineOrderComment, w => w.Buffer.Text)
				.InitializeFromSource();
			
			lblContactPhoneTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowContactPhone, w => w.Visible)
				.InitializeFromSource();
			lblContactPhone.Binding
				.AddBinding(ViewModel, vm => vm.CanShowContactPhone, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanShowContactPhone, w => w.Visible)
				.InitializeFromSource();
			
			lblCancellationReasonTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCancellationReason, w => w.Visible)
				.InitializeFromSource();

			cancellationReasonEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCancellationReason, w => w.Visible)
				.InitializeFromSource();
			cancellationReasonEntry.ViewModel = ViewModel.CancellationReasonViewModel;
			
			lblBottlesReturn.Binding
				.AddBinding(ViewModel, vm => vm.BottlesReturn, w => w.LabelProp)
				.InitializeFromSource();
			
			lblTrifle.Binding
				.AddBinding(ViewModel, vm => vm.Trifle, w => w.LabelProp)
				.InitializeFromSource();

			lblSum.Text = ViewModel.Entity.OnlineOrderSum.ToString("N2");
		}

		private void ConfigureItemsWidgets()
		{
			lblOnlinePromoSets.Visible = ViewModel.CanShowPromoItems;
			PromoItemsScrolledWindow.Visible = ViewModel.CanShowPromoItems;
			ConfigureTreePromoItems();
			
			lblOnlineOrderItems.Visible = ViewModel.CanShowNotPromoItems;
			NotPromoItemsScrolledWindow.Visible = ViewModel.CanShowNotPromoItems;
			ConfigureTreeNotPromoItems();
		}

		private void ConfigureTreePromoItems()
		{
			treeViewPromoItems.ColumnsConfig = FluentColumnsConfig<OnlineOrderItem>.Create()
				.AddColumn("№")
				.AddNumericRenderer(node => ViewModel.OnlineOrderPromoItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
				.AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "Не указано")
				.AddColumn("Кол-во(онлайн заказ)")
				.AddNumericRenderer(node => node.Count)
				.AddSetter((cell, node) =>
					cell.CellBackgroundGdk = node.Count != node.CountFromPromoSet ? GdkColors.DangerBase : GdkColors.PrimaryBase)
				.AddColumn("Кол-во(в промонаборе)")
				.AddNumericRenderer(node => node.CountFromPromoSet)
				.AddColumn("Цена(онлайн заказ)")
				.AddNumericRenderer(node => node.Price)
				.AddSetter((cell, node) =>
					cell.CellBackgroundGdk = node.Price != node.NomenclaturePrice ? GdkColors.DangerBase : GdkColors.PrimaryBase)
				.AddColumn("Цена(ДВ)")
				.AddNumericRenderer(node => node.NomenclaturePrice)
				.AddColumn("Сумма(онлайн заказ)")
				.AddNumericRenderer(node => node.Sum)
				.AddColumn("Скидка(онлайн заказ)")
				.AddNumericRenderer(node => node.IsDiscountInMoney ? node.MoneyDiscount : node.PercentDiscount)
				.AddSetter((cell, node) =>
					{
						var onlineDiscount = node.IsDiscountInMoney ? node.MoneyDiscount : node.PercentDiscount;
						cell.CellBackgroundGdk = onlineDiscount != node.DiscountFromPromoSet ? GdkColors.DangerBase : GdkColors.PrimaryBase;
					})
				.AddColumn("Скидка(в промонаборе)")
				.AddNumericRenderer(node => node.DiscountFromPromoSet)
				.AddColumn("Скидка в рублях?(онлайн заказ)")
				.AddToggleRenderer(node => node.IsDiscountInMoney)
				.AddSetter((cell, node) =>
					cell.CellBackgroundGdk = node.IsDiscountInMoney != node.IsDiscountInMoneyFromPromoSet ? GdkColors.DangerBase : GdkColors.PrimaryBase)
				.AddColumn("Скидка в рублях?(в промонаборе)")
				.AddToggleRenderer(node => node.IsDiscountInMoneyFromPromoSet)
				.AddColumn("Промонабор")
				.AddTextRenderer(node => node.PromoSet != null ? node.PromoSet.Name : string.Empty)
				.Finish();

			treeViewPromoItems.Visible = ViewModel.CanShowPromoItems;
			treeViewPromoItems.ItemsDataSource = ViewModel.OnlineOrderPromoItems;
		}
		
		private void ConfigureTreeNotPromoItems()
		{
			treeViewNotPromoItems.ColumnsConfig = FluentColumnsConfig<OnlineOrderItem>.Create()
				.AddColumn("№")
				.AddNumericRenderer(node => ViewModel.OnlineOrderNotPromoItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
				.AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "Не указано")
				.AddColumn("Кол-во(онлайн заказ)")
				.AddNumericRenderer(node => node.Count)
				.AddColumn("Цена(онлайн заказ)")
				.AddNumericRenderer(node => node.Price)
				.AddSetter((cell, node) =>
					cell.CellBackgroundGdk = node.Price != node.NomenclaturePrice ? GdkColors.DangerBase : GdkColors.PrimaryBase)
				.AddColumn("Цена(ДВ)")
				.AddNumericRenderer(node => node.NomenclaturePrice)
				.AddColumn("Сумма(онлайн заказ)")
				.AddNumericRenderer(node => node.Sum)
				.AddColumn("")
				.Finish();

			treeViewNotPromoItems.Visible = ViewModel.CanShowNotPromoItems;
			treeViewNotPromoItems.ItemsDataSource = ViewModel.OnlineOrderNotPromoItems;
		}

		private void ConfigureTreeRentWidgets()
		{
			lblOnlineRentPackages.Visible = ViewModel.CanShowRentPackages;
			RentPackagesScrolledWindow.Visible = ViewModel.CanShowRentPackages;
			ConfigureTreeRentPackages();
		}

		private void ConfigureTreeRentPackages()
		{
			treeViewOnlineRentPackages.ColumnsConfig = FluentColumnsConfig<OnlineFreeRentPackage>.Create()
				.AddColumn("№")
				.AddNumericRenderer(node => ViewModel.OnlineRentPackages.IndexOf(node) + 1)
				.AddColumn("Аренда")
				.AddTextRenderer(node =>
					node.FreeRentPackage != null && node.FreeRentPackage.DepositService != null
						? node.FreeRentPackage.DepositService.Name
						: "Не указано")
				.AddColumn("Кол-во(онлайн заказ)")
				.AddNumericRenderer(node => node.Count)
				.AddColumn("Цена(онлайн заказ)")
				.AddNumericRenderer(node => node.Price)
				.AddSetter((cell, node) =>
					cell.CellBackgroundGdk = node.Price != node.FreeRentPackagePriceFromProgram ? GdkColors.DangerBase : GdkColors.PrimaryBase)
				.AddColumn("Цена(ДВ)")
				.AddNumericRenderer(node => node.FreeRentPackagePriceFromProgram)
				.AddColumn("")
				.Finish();

			treeViewOnlineRentPackages.Visible = ViewModel.CanShowRentPackages;
			treeViewOnlineRentPackages.ItemsDataSource = ViewModel.OnlineRentPackages;
		}
	}
}
