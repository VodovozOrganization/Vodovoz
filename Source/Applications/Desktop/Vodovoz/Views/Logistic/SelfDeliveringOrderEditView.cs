using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelfDeliveringOrderEditView : TabViewBase<SelfDeliveringOrderEditViewModel>
	{
		public SelfDeliveringOrderEditView(SelfDeliveringOrderEditViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ViewModel.TreeItems = treeItems;

			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CloseCommand);
			
			entityVMEntryClient1.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			entityVMEntryClient1.Binding
				.AddBinding(ViewModel.Entity, s => s.Client, w => w.Subject)
				.InitializeFromSource();
			//Проверку сделать?
			//entityVMEntryClient1.CanEditReference = !ViewModel.UserHasOnlyAccessToWarehouseAndComplaints;

			ycheckbuttonPayAfterShipment.Binding
				.AddBinding(ViewModel.Entity, e => e.PayAfterShipment, w => w.Active)
				.InitializeFromSource();

			specialListCmbSelfDeliveryGeoGroup.ItemsList = ViewModel.GetSelfDeliveryGeoGroups();
			specialListCmbSelfDeliveryGeoGroup.Binding
				.AddBinding(ViewModel.Entity, e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
				.AddBinding(e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			yentryPaymentType.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PaymentType.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			buttonSelectPaymentType.BindCommand(ViewModel.PaymentTypeCommand);

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItem)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();

			yentryPaymentNumber.Binding
				.AddBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber,
				w => w.Text,
				new NullableIntToStringConverter())
				.InitializeFromSource();

			treeItems.CreateFluentColumnsConfig<OrderItem>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.OrderItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
					.SetTag(nameof(Nomenclature))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					.EditedEvent(ViewModel.OnSpinPriceEdited)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Альтерн.\nцена")
					.AddToggleRenderer(x => x.IsAlternativePrice).Editing(false)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
					.AddSetter((c, n) =>
					{
						if(ViewModel.Entity.OrderStatus == OrderStatus.DeliveryCanceled || ViewModel.Entity.OrderStatus == OrderStatus.NotDelivered)
						{
							c.Text = CurrencyWorks.GetShortCurrencyString(n.OriginalSum);
						}
					}
					)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount)
					.AddSetter((c, n) => c.Editable = ViewModel.CanChangeDiscountValue)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)(n.Price * n.CurrentCount), 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.AddSetter((c, n) =>
					{
						if(ViewModel.Entity.OrderStatus == OrderStatus.DeliveryCanceled || ViewModel.Entity.OrderStatus == OrderStatus.NotDelivered)
						{
							c.Text = n.ManualChangingOriginalDiscount.ToString();
						}
					})
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
					.AddSetter((c, n) => c.Activatable = ViewModel.CanChangeDiscountValue)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(x => x.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(item =>
					{
						var list = ViewModel.DiscountReasons.Where(
								dr => ViewModel.DiscountsController.IsApplicableDiscount(dr, item.Nomenclature)).ToList();
						return list;
					})
					.EditedEvent(ViewModel.OnDiscountReasonComboEdited)
					.AddSetter((cell, node) => cell.Editable = node.DiscountByStock == 0)
					//.AddSetter(
					//	(c, n) =>
					//		c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.PromoSet == null ? colorLightRed : colorPrimaryBase
					//)
					.AddSetter((c, n) =>
					{
						if(n.PromoSet != null && n.DiscountReason == null && n.Discount > 0)
						{
							c.Text = n.PromoSet.DiscountReasonInfo;
						}
						else if(ViewModel.Entity.OrderStatus == OrderStatus.DeliveryCanceled || ViewModel.Entity.OrderStatus == OrderStatus.NotDelivered)
						{
							c.Text = n.OriginalDiscountReason?.Name ?? n.DiscountReason?.Name;
						}
					})
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = ViewModel.Entity.ObservableOrderItems;
		}
	}
}
