using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using QS.Utilities;
using Gamma.GtkWidgets.Cells;
using Vodovoz.Domain.Goods;
using System.Linq;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderWithoutShipmentForAdvancePaymentView : TabViewBase<OrderWithoutShipmentForAdvancePaymentViewModel>
	{
		public OrderWithoutShipmentForAdvancePaymentView(OrderWithoutShipmentForAdvancePaymentViewModel viewModel) : base(viewModel)
		{
			this.Build();

			Configure();
		}

		private void Configure()
		{
			buttonAddForSale.Clicked += (sender, e) => ViewModel.AddForSaleCommand.Execute();
			btnDeleteOrderItem.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute();
			ybtnSendEmail.Clicked += (sender, e) => ViewModel.SendEmailCommand.Execute();

			//ylabelOrderNum.Binding.AddBinding(ViewModel, vm => vm.Entity.Id, w => w.Text).InitializeFromSource();
			ylabelOrderDate.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text).InitializeFromSource();
			ylabelOrderAuthor.Binding.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text).InitializeFromSource();
			btnDeleteOrderItem.Binding.AddFuncBinding(ViewModel, vm => vm.SelectedItem != null, w => w.Sensitive).InitializeFromSource();

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));
			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = ViewModel.UoW.Session.QueryOver<DiscountReason>().List();

			//yentryEmail.Binding.AddBinding();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, vm => vm.Client, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.CanEditReference = true;

			ViewModel.OpenCounterpatyJournal += entityviewmodelentry1.OpenSelectDialog;

			ConfigureTreeItems();
		}

		private void ConfigureTreeItems()
		{
			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightYellow = new Gdk.Color(0xe1, 0xd6, 0x70);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderWithoutShipmentForAdvancePaymentItem>()
				.AddColumn("Номенклатура")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn("Кол-во")
				.SetTag("Count")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing().WidthChars(10)
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					//.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
				.AddTextRenderer(node => node.ActualCount.HasValue ? string.Format("[{0}]", node.ActualCount) : string.Empty)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : string.Empty)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					/*.AddSetter((NodeCellRendererSpin<OrderWithoutShipmentForAdvancePaymentItem> c, OrderWithoutShipmentForAdvancePaymentItem node) => {

						c.ForegroundGdk = colorBlack;
						if(node.AdditionalAgreement == null) {
							return;
						}
						AdditionalAgreement aa = node.AdditionalAgreement.Self;
						if(aa is WaterSalesAgreement wsa && wsa.HasFixedPrice && wsa.FixedPrices.Any(x => x.Nomenclature.Id == node.Nomenclature.Id)) {
							c.ForegroundGdk = colorGreen;
						} else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
							c.ForegroundGdk = colorBlue;
						}
						
					})*/
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS))
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount).Editing()
				.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)n.Price * n.CurrentCount, 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney).Editing()
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(OrderSingletonRepository.GetInstance().GetDiscountReasons(ViewModel.UoW))
					.AddSetter((c, n) => c.Editable = n.Discount > 0)
					.AddSetter(
						(c, n) => c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null
						? colorLightRed
						: colorWhite
					)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = ViewModel.Entity.ObservableOrderWithoutDeliveryForAdvancePaymentItems;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			//treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault().Visible = Entity.PromotionalSets.Count > 0;
		}

		private void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			ViewModel.SelectedItem = treeItems.GetSelectedObject();
		}
	}
}
