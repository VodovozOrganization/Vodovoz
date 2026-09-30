using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[ToolboxItem(true)]
	public partial class OrderWithoutShipmentForAdvancePaymentView : TabViewBase<OrderWithoutShipmentForAdvancePaymentViewModel>
	{
		public OrderWithoutShipmentForAdvancePaymentView(OrderWithoutShipmentForAdvancePaymentViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			buttonAddForSale.Clicked += (sender, e) => ViewModel.AddForSaleCommand.Execute();
			btnDeleteOrderItem.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute();
			ybtnOpenBill.Clicked += (sender, e) => ViewModel.OpenBillCommand.Execute();

			ylabelOrderNum.Binding
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ylabelOrderDate.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text)
				.InitializeFromSource();

			ylabelOrderAuthor.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text)
				.InitializeFromSource();

			btnDeleteOrderItem.Binding
				.AddFuncBinding(ViewModel, vm => vm.SelectedItem != null, w => w.Sensitive)
				.InitializeFromSource();

			yCheckBtnHideSignature.Binding
				.AddBinding(ViewModel.Entity, e => e.HideSignature, w => w.Active)
				.InitializeFromSource();
			
			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			entityViewModelEntryCounterparty.Changed += ViewModel.OnCounterpartyEntityViewModelEntryChanged;

			entityViewModelEntryCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.Client, w => w.Subject)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.CanEditReference = true;
			
			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hboxSendDocuments.Add(sendEmailView);
			sendEmailView.Show();
			
			ViewModel.OpenCounterpartyJournal += entityViewModelEntryCounterparty.OpenSelectDialog;

			organizationEntry.ViewModel = ViewModel.OrganizationViewModel;
			organizationEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanSetOrganization, w => w.Sensitive)
				.InitializeFromSource();
			
			ConfigureTreeItems();

			treeViewEdoContainers.ColumnsConfig = FluentColumnsConfig<EdoContainer>.Create()
				.AddColumn("Код документооборота")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn("Отправленные\nдокументы")
					.AddTextRenderer(x => x.SentDocuments)
				.AddColumn("Статус\nдокументооборота")
					.AddEnumRenderer(x => x.EdoDocFlowStatus)
				.AddColumn("Доставлено\nклиенту?")
					.AddToggleRenderer(x => x.Received)
					.Editing(false)
				.AddColumn("Описание ошибки")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();

			if(ViewModel.Entity.Id != 0)
			{
				CustomizeSendDocumentAgainButton();
			}

			treeViewEdoContainers.ItemsDataSource = ViewModel.EdoContainers;

			btnUpdateEdoDocFlowStatus.Clicked += (sender, args) =>
			{
				ViewModel.UpdateEdoContainers();
				CustomizeSendDocumentAgainButton();
			};

			ybuttonSendDocumentAgain.Clicked += ViewModel.OnButtonSendDocumentAgainClicked;

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			UpdateContainersVisibility();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanSendBillByEdo)
				|| e.PropertyName == nameof(ViewModel.CanResendEdoBill))
			{
				UpdateContainersVisibility();
				CustomizeSendDocumentAgainButton();
			}
		}

		private void UpdateContainersVisibility()
		{
			vboxEdo.Visible = ViewModel.CanSendBillByEdo || ViewModel.EdoContainers.Any();
		}

		private void ConfigureTreeItems()
		{
			var colorWhite = GdkColors.PrimaryBase;
			var colorLightRed = GdkColors.DangerBase;

			treeItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderWithoutShipmentForAdvancePaymentItem>()
				.AddColumn("Номенклатура")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn("Кол-во")
				.SetTag("Count")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count, OnCountEdited)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing()
					.WidthChars(10)
					.AddSetter((cell, node) => cell.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.IsRentCategory ? node.RentString : string.Empty)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price)
					.Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(true)
					.AddSetter((cell, node) => cell.Editable = node.CanEditPrice)
					.EditedEvent(OnPriceEdited)
				.AddColumn("Альтерн.\nцена")
					.AddToggleRenderer(x => x.IsAlternativePrice).Editing(false)
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS ?? 0))
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Sum))
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount, OnDiscountEdited)
					.AddSetter((c, n) => c.Editable = ViewModel.CanChangeDiscountValue)
					.Editing()
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
							? new Adjustment(0, 0, (double)(n.Price * n.Count), 1, 100, 1)
							: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
					.AddSetter((c, n) => c.Activatable = ViewModel.CanChangeDiscountValue)
					.Editing()
					.ToggledEvent(OnIsDiscountMoneyToggled)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DiscountReasonsNames)                    
				.AddSetter((c, n) =>
					c.BackgroundGdk = n.Discount > 0 && !n.DiscountReasons.Any()
						? colorLightRed
						: colorWhite)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = ViewModel.Entity.ObservableOrderWithoutDeliveryForAdvancePaymentItems;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;

			orderitemdiscountreasonsview.ViewModel = ViewModel.OrderItemDiscountReasonsViewModel;
		}

		private void OnCountEdited(object o, EditedArgs args)
		{
			var stringCount = args.NewText.Replace(',', '.');
			decimal.TryParse(stringCount, NumberStyles.Any, CultureInfo.InvariantCulture, out var newCount);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is OrderWithoutShipmentForAdvancePaymentItem saleItem))
			{
				return;
			}

			ViewModel.SaleHandler.SetCount(saleItem, newCount);
			var path = new TreePath(args.Path);
			treeItems.YTreeModel.GetIter(out var iter, path);
			treeItems.YTreeModel.Adapter.EmitRowChanged(path, iter);
		}
		
		private void OnPriceEdited(object o, EditedArgs args)
		{
			var stringPrice = args.NewText.Replace(',', '.');
			decimal.TryParse(stringPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var newPrice);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(newPrice < 0)
			{
				newPrice = 0;
			}

			if(!(node is OrderWithoutShipmentForAdvancePaymentItem saleItem))
			{
				return;
			}

			ViewModel.SaleHandler.SetPrice(saleItem, (SaleItemPriceType.User, newPrice));
		}
		
		private void OnIsDiscountMoneyToggled(object o, ToggledArgs args)
		{
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			
			if(!(node is OrderWithoutShipmentForAdvancePaymentItem saleItem))
			{
				return;
			}
			
			//Т.к. событие приходит до изменения значения, то передаем в расчет инвертированное текущее значение
			//и меняем местами скидки
			ViewModel.SaleHandler.RecalculateDiscounts(
				CommonRecalculateDiscount.CreateDataContext(
					saleItem,
					DiscountValue.Create(!saleItem.IsDiscountInMoney, saleItem.DiscountMoney, saleItem.Discount))
				);
		}
		
		private void OnDiscountEdited(object o, EditedArgs args)
		{
			var stringDiscount = args.NewText.Replace(',', '.');
			decimal.TryParse(stringDiscount, NumberStyles.Any, CultureInfo.InvariantCulture, out var newDiscount);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is OrderWithoutShipmentForAdvancePaymentItem saleItem))
			{
				return;
			}
			
			ViewModel.SaleHandler.RecalculateDiscounts(
				CommonRecalculateDiscount.CreateDataContext(
					saleItem,
					DiscountValue.Create(saleItem.IsDiscountInMoney, newDiscount, newDiscount))
				);
		}

		private void CustomizeSendDocumentAgainButton()
		{
			if(!ViewModel.EdoContainers.Any())
			{
				ybuttonSendDocumentAgain.Sensitive = ViewModel.CanSendBillByEdo;
				ybuttonSendDocumentAgain.Label = "Отправить";
				return;
			}

			ybuttonSendDocumentAgain.Sensitive = ViewModel.CanResendEdoBill;
			ybuttonSendDocumentAgain.Label = "Отправить повторно";
		}

		private void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			ViewModel.SelectedItem = treeItems.GetSelectedObject();
		}
		
		public override void Destroy()
		{
			entityViewModelEntryCounterparty.Changed -= ViewModel.OnCounterpartyEntityViewModelEntryChanged;
			
			base.Destroy();
		}
	}
}
