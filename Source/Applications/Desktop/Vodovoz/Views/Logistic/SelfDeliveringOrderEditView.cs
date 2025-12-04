using Autofac;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Project.Services;
using QS.Utilities;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using System.Globalization;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
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
			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CloseCommand);

			var counterpartyViewModel = new LegacyEEVMBuilderFactory<Order>(
				ViewModel,
				ViewModel,
				ViewModel.Entity,
				ViewModel.UoW,
				ViewModel.NavigationManager,
				ViewModel.LifetimeScope)
				.ForProperty(x => x.Client)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			entityentryCounterparty.ViewModel = counterpartyViewModel;
			entityentryCounterparty.Sensitive = false;

			ycheckbuttonPayAfterShipment.Binding
				.AddBinding(ViewModel.Entity, e => e.PayAfterShipment, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonPayAfterShipment.Sensitive = false;

			specialListCmbSelfDeliveryGeoGroup.ItemsList = ViewModel.GetSelfDeliveryGeoGroups();
			specialListCmbSelfDeliveryGeoGroup.Binding
				.AddBinding(ViewModel.Entity, e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
				.AddBinding(e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();
			specialListCmbSelfDeliveryGeoGroup.Sensitive = false;

			yentryPaymentType.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PaymentType.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();
			yentryPaymentType.Sensitive = false;

			buttonSelectPaymentType.BindCommand(ViewModel.PaymentTypeCommand);
			buttonSelectPaymentType.Sensitive = false;

			// FIXME Возникают нюансы с перехода с PaymentType.Terminal на любой другой вид оплаты, нужно смотреть
			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItem)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();
			yenumcomboboxTerminalSubtype.Sensitive = false;

			yentryPaymentNumber.Binding
				.AddBinding(ViewModel.Entity,
				e => e.OnlinePaymentNumber,
				w => w.Text,
				new NullableIntToStringConverter())
				.InitializeFromSource();
			yentryPaymentNumber.Sensitive = false;

			ConfigureTrees();
			treeItems.ItemsDataSource = ViewModel.Entity.ObservableOrderItems;
		}
		private void ConfigureTrees()
		{
			var colorPrimaryText = GdkColors.PrimaryText;
			var colorBlue = GdkColors.InfoText;
			var colorGreen = GdkColors.SuccessText;
			var colorPrimaryBase = GdkColors.PrimaryBase;
			var colorLightYellow = GdkColors.WarningBase;
			var colorLightRed = GdkColors.DangerBase;

			treeItems.CreateFluentColumnsConfig<OrderItem>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.OrderItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
					.SetTag(nameof(Nomenclature))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
					.AddSetter((c, n) => { c.Sensitive = false; })
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, n) => c.Editable = ViewModel.CanChangeDiscountValue)
					.EditedEvent((o, args) => OnSpinPriceEdited(o, args, treeItems))
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) =>
					{
						if(ViewModel.Entity.OrderStatus == OrderStatus.NewOrder || (ViewModel.Entity.OrderStatus == OrderStatus.WaitForPayment && !ViewModel.Entity.SelfDelivery))//костыль. на Win10 не видна цветная цена, если виджет засерен
						{
							c.ForegroundGdk = colorPrimaryText;
							var fixedPrice = new Order().GetFixedPriceOrNull(node.Nomenclature, node.TotalCountInOrder);
							if(fixedPrice != null && node.PromoSet == null && node.CopiedFromUndelivery == null)
							{
								c.ForegroundGdk = colorGreen;
							}
							else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category))
							{
								c.ForegroundGdk = colorBlue;
							}
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
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
					.EditedEvent(OnDiscountReasonComboEdited)
					.AddSetter((c, n) => c.Editable = ViewModel.CanChangeDiscountValue)
					.AddSetter((c, n) => { c.Sensitive = false; })
					.AddSetter(
						(c, n) =>
							c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.PromoSet == null ? colorLightRed : colorPrimaryBase
					)
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
		}
		private void OnSpinPriceEdited(object o, EditedArgs args, yTreeView treeItems)
		{ 
			decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newPrice);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			orderItem.SetPrice(newPrice);
		}
		private void OnDiscountReasonComboEdited(object o, EditedArgs args)
		{
			var index = int.Parse(args.Path);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			var previousDiscountReason = orderItem.DiscountReason;

			Gtk.Application.Invoke((sender, eventArgs) =>
			{
				ViewModel.ApplyDiscountReasonToOrderItem(orderItem, index);
			});
		}
	}
}
