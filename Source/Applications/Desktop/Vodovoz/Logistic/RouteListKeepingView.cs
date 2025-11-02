using Gamma.GtkWidgets;
using Gdk;
using Gtk;
using QS.Commands;
using QS.Journal.GtkUI;
using QS.Tdi;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.ViewWidgets.Logistics;
using Vodovoz.ViewWidgets.Mango;
using Label = Gtk.Label;

namespace Vodovoz.Logistic
{
	public partial class RouteListKeepingView : TabViewBase<RouteListKeepingViewModel>, ITDICloseControlTab
	{
		private readonly Color _dangerBaseColor = GdkColors.DangerBase;
		private readonly Color _successBaseColor = GdkColors.SuccessBase;
		private readonly Color _insensitiveBaseColor = GdkColors.InsensitiveBase;
		private readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		private readonly Dictionary<RouteListItemStatus, Pixbuf> _statusIcons
			= new Dictionary<RouteListItemStatus, Pixbuf>();
		private RouteListKeepingItemNode _selectedItem;

		public event RowActivatedHandler OnClosingItemActivated;

		private Menu _addressesPopup = new Menu();
		private MenuItem _addressesOpenOrderCodes = new MenuItem("Просмотр кодов по заказу");

		public RouteListKeepingView(RouteListKeepingViewModel viewModel)
			: base(viewModel)
		{
			CopyIdCommand = new DelegateCommand(CopyEntityId);

			Build();
			Initialize();
		}
	
		private void Initialize()
		{
			ybuttonSave.BindCommand(ViewModel.SaveCommand);

			ybuttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCancel.BindCommand(ViewModel.CancelCommand);

			ybuttonCancel.Binding
				.AddBinding(ViewModel, vm => vm.IsCanClose, w => w.Sensitive)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarViewModel;
			entityentryCar.Binding
				.AddBinding(ViewModel, vm => vm.LogisticanEditing, w => w.Sensitive)
				.InitializeFromSource();

			var deliveryfreebalanceview = new DeliveryFreeBalanceView(ViewModel.DeliveryFreeBalanceViewModel);

			deliveryfreebalanceview.Binding
				.AddBinding(
					ViewModel.Entity,
					e => e.ObservableDeliveryFreeBalanceOperations,
					w => w.ObservableDeliveryFreeBalanceOperations)
				.InitializeFromSource();

			deliveryfreebalanceview.ShowAll();
			yhboxDeliveryFreeBalance
				.PackStart(deliveryfreebalanceview, true, true, 0);

			entityentryDriver.ViewModel = ViewModel.DriverViewModel;
			entityentryDriver.Binding
				.AddBinding(ViewModel, vm => vm.LogisticanEditing, w => w.Sensitive)
				.InitializeFromSource();

			entityentryForwarder.ViewModel = ViewModel.ForwarderViewModel;
			entityentryForwarder.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeForwarder, w => w.Sensitive)
				.InitializeFromSource();

			entityentryLogistician.ViewModel = ViewModel.LogisticianViewModel;
			entityentryLogistician.Binding
				.AddBinding(ViewModel, vm => vm.LogisticanEditing, w => w.Sensitive)
				.InitializeFromSource();

			speciallistcomboboxShift.ItemsList = ViewModel.ActiveShifts;
			speciallistcomboboxShift.Binding
				.AddBinding(ViewModel.Entity, rl => rl.Shift, widget => widget.SelectedItem)
				.AddBinding(ViewModel, wm => wm.LogisticanEditing, w => w.Sensitive)
				.InitializeFromSource();

			datePickerDate.Binding
				.AddBinding(ViewModel.Entity, rl => rl.Date, widget => widget.Date)
				.AddBinding(ViewModel, vm => vm.LogisticanEditing, w => w.Sensitive)
				.InitializeFromSource();

			ylabelLastTimeCall.Binding
				.AddFuncBinding(
					ViewModel.Entity,
					e => ViewModel.GetLastCallTime(e.LastCallTime),
					w => w.LabelProp)
				.InitializeFromSource();

			yspinActualDistance.Binding
				.AddBinding(ViewModel, vm => vm.AllEditing, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCallMaden.BindCommand(ViewModel.CallMadenCommand);

			ybuttonCallMaden.Binding
				.AddBinding(ViewModel, vm => vm.AllEditing, w => w.Sensitive)
				.InitializeFromSource();

			ylabelBottleInfo.UseMarkup = true;

			ylabelBottleInfo.Binding
				.AddBinding(ViewModel, vm => vm.BottlesInfo, w => w.LabelProp)
				.InitializeFromSource();

			ybuttonSetStatusComplete.BindCommand(ViewModel.SetStatusCompleteCommand);

			ybuttonSetStatusComplete.Binding
				.AddBinding(ViewModel, vm => vm.CanComplete, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonChangeDeliveryTime.BindCommand(ViewModel.ChangeDeliveryTimeCommand);

			ybuttonChangeDeliveryTime.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeDeliveryTime, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSetStatusEnRoute.BindCommand(ViewModel.ReturnToEnRouteStatus);

			ybuttonSetStatusEnRoute.Binding
				.AddBinding(ViewModel, vm => vm.CanReturnRouteListToEnRouteStatus, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonSetStatusDelivered.BindCommand(ViewModel.ReDeliverCommand);

			ybuttonSetStatusDelivered.Binding
				.AddBinding(ViewModel.Entity, e => e.CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateFine.BindCommand(ViewModel.CreateFineCommand);

			ybuttonCreateFine.Binding
				.AddBinding(ViewModel, vm => vm.AllEditing, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonRefresh.BindCommand(ViewModel.RefreshCommand);

			ybuttonRefresh.Binding
				.AddBinding(ViewModel, vm => vm.AllEditing, w => w.Sensitive)
				.InitializeFromSource();

			InitilizeItemsRowsIcons();

			InitializeRouteListAddressesTreeView();

			InitializePhones();

			ybuttonCopyId.BindCommand(CopyIdCommand);

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SelectedRouteListAddressesObjects))
			{
				var selectedItems = ViewModel.Items
					.Where(x => ViewModel.SelectedRouteListAddresses
						.Select(srla => srla.RouteListItem.Order.Id)
						.Contains(x.RouteListItem.Order.Id))
					.ToArray();

				if(selectedItems.Any())
				{
					ytreeviewAddresses.SelectObject(selectedItems);
					var iter = ytreeviewAddresses.YTreeModel.IterFromNode(selectedItems[0]);
					var path = ytreeviewAddresses.YTreeModel.GetPath(iter);
					ytreeviewAddresses.ScrollToCell(path, ytreeviewAddresses.Columns[0], true, 0.5f, 0.5f);
				}
			}
		}

		private void InitializePhones()
		{
			if(ViewModel.Entity.Driver != null && ViewModel.Entity.Driver.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(ViewModel.Entity.Driver.Phones.Count + 1);
				PhonesTable1.Resize(rows, 2);
				var label = new Label();
				label.LabelProp = $"{ViewModel.Entity.Driver.FullName}";
				PhonesTable1.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					var l = new Label();
					l.LabelProp = "+7 " + ViewModel.Entity.Driver.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable1.Attach(l, 0, 1, i, i + 1);

					var h = new HandsetView(
						ViewModel.Entity.Driver.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable1.Attach(h, 1, 2, i, i + 1);
				}
			}

			if(ViewModel.Entity.Forwarder != null && ViewModel.Entity.Forwarder.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(ViewModel.Entity.Forwarder.Phones.Count + 1);
				PhonesTable2.Resize(rows, 2);
				var label = new Label();
				label.LabelProp = $"{ViewModel.Entity.Forwarder.FullName}";
				PhonesTable2.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					var l = new Label();
					l.LabelProp = "+7 " + ViewModel.Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable2.Attach(l, 0, 1, i, i + 1);

					var h = new HandsetView(
						ViewModel.Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable2.Attach(h, 1, 2, i, i + 1);
				}
			}

			//Телефон
			PhonesTable1.ShowAll();
			PhonesTable2.ShowAll();

			var mangoManager = Startup.MainWin.MangoManager;

			phoneLogistican.MangoManager = mangoManager;
			phoneDriver.MangoManager = mangoManager;
			phoneForwarder.MangoManager = mangoManager;

			phoneLogistican.Binding
				.AddBinding(ViewModel.Entity, e => e.Logistician, w => w.Employee)
				.InitializeFromSource();

			phoneDriver.Binding
				.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Employee)
				.InitializeFromSource();

			phoneForwarder.Binding
				.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Employee)
				.InitializeFromSource();
		}

		private void InitializeRouteListAddressesTreeView()
		{
			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.RouteListItem.IndexInRoute + 1)
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliveryPoint == null ? "Требуется точка доставки" : node.RouteListItem.Order.DeliveryPoint.ShortAddress)
				.AddColumn("Ожидает до")
					.AddTimeRenderer(node => node.WaitUntil)
					.AddSetter((c, n) => c.Editable = ViewModel.IsOrderWaitUntilActive && n.RouteListItem.Order.OrderStatus == Domain.Orders.OrderStatus.OnTheWay)
					.WidthChars(5)
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)
				.AddColumn("Форма оплаты")
					.AddEnumRenderer(node => 
						node.PaymentType,
						excludeItems: ViewModel.ExcludedPaymentTypes
					)
					.AddSetter((c, n) =>
					{
						c.Editable = ViewModel.AllEditing 
						&& n.RouteListItem.Status == RouteListItemStatus.EnRoute 
						&& Order.EditablePaymentTypes.Contains(n.RouteListItem.Order.PaymentType);
					})
				.AddColumn("Статус")
					.AddPixbufRenderer(x => _statusIcons[x.Status])
					.AddEnumRenderer(node => node.Status, excludeItems: new Enum[] { RouteListItemStatus.Transfered })
					.AddSetter((c, n) => c.Editable = ViewModel.AllEditing && n.Status != RouteListItemStatus.Transfered)
				.AddColumn("Отгрузка")
					.AddNumericRenderer(node => node.RouteListItem.Order.OrderItems
					.Where(b => b.Nomenclature.Category == NomenclatureCategory.water && b.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Sum(b => b.Count))
				.AddColumn("Возврат тары")
					.AddNumericRenderer(node => node.RouteListItem.Order.BottlesReturn)
				.AddColumn("Сдали по факту")
					.AddNumericRenderer(node => node.RouteListItem.DriverBottlesReturned)
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.RouteListItem.Order.IsFastDelivery).Editing(false)
				.AddColumn("Статус изменен")
					.AddTextRenderer(node => node.LastUpdate)
				.AddColumn("Комментарий")
					.AddTextRenderer(node => node.Comment)
				.AddColumn("Время передачи")
					.AddTextRenderer(node => node.RecievedTransferAt == null ? "": node.RecievedTransferAt.Value.ToString("dd.MM.yyyy hh:mm:ss"))
					.Editable(ViewModel.AllEditing)
				.AddColumn("Переносы")
					.AddTextRenderer(node => node.Transferred)
				.AddColumn("Клиент")
					.AddTextRenderer(node => node.RouteListItem.Order.Client != null
						? node.RouteListItem.Order.Client.Name
						: "")
				.AddColumn("Телефон")
					.AddTextRenderer(node => node.RouteListItem.Order.ContactPhone != null
						? node.RouteListItem.Order.ContactPhone.Additional + node.RouteListItem.Order.ContactPhone
						: "")
				.AddColumn("Отзвон за")
					.AddTextRenderer(node => node.RouteListItem.Order.CallBeforeArrivalMinutes.HasValue
						? $"{node.RouteListItem.Order.CallBeforeArrivalMinutes.Value} мин."
						: "")
				.RowCells()
					.AddSetter<CellRenderer>((cell, node) =>
					{
						switch(node.Status)
						{
							case RouteListItemStatus.Overdue:
								cell.CellBackgroundGdk = _dangerBaseColor;
								break;
							case RouteListItemStatus.Completed:
								cell.CellBackgroundGdk = _successBaseColor;
								break;
							case RouteListItemStatus.Canceled:
								cell.CellBackgroundGdk = _insensitiveBaseColor;
								break;
							default:
								cell.CellBackgroundGdk = _primaryBaseColor;
								break;
						}
					})
				.Finish();

			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			ytreeviewAddresses.RowActivated += OnYtreeviewAddressesRowActivated;

			ytreeviewAddresses.Binding
				.AddBinding(ViewModel, vm => vm.SelectedRouteListAddressesObjects, w => w.SelectedRows)
				.AddBinding(ViewModel, vm => vm.AllEditing, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
			ytreeviewAddresses.Add(_addressesPopup);
			_addressesPopup.Add(_addressesOpenOrderCodes);
			_addressesOpenOrderCodes.Show();
			_addressesPopup.Show();
			_addressesOpenOrderCodes.Activated += (sender, e) => ViewModel.OpenOrderCodesCommand.Execute(null);
			ytreeviewAddresses.ButtonReleaseEvent += OnAddressRightClick;
		}

		private void OnAddressRightClick(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}

			_addressesOpenOrderCodes.Sensitive = ViewModel.OpenOrderCodesCommand.CanExecute(null);
			_addressesPopup.Popup();
		}

		/// <summary>
		/// Заполнение иконок для дальнейшего отображения
		/// </summary>
		public void InitilizeItemsRowsIcons()
		{
			var waterDeliveryApplicationAssembly = Assembly.GetAssembly(typeof(Startup));
			_statusIcons.Add(
				RouteListItemStatus.EnRoute,
				new Gdk.Pixbuf(
					waterDeliveryApplicationAssembly,
					"Vodovoz.icons.status.car.png"));

			_statusIcons.Add(
				RouteListItemStatus.Completed,
				new Gdk.Pixbuf(
					waterDeliveryApplicationAssembly,
					"Vodovoz.icons.status.face-smile-grin.png"));

			_statusIcons.Add(
				RouteListItemStatus.Overdue,
				new Gdk.Pixbuf(
					waterDeliveryApplicationAssembly,
					"Vodovoz.icons.status.face-angry.png"));

			_statusIcons.Add(
				RouteListItemStatus.Canceled,
				new Gdk.Pixbuf(
					waterDeliveryApplicationAssembly,
					"Vodovoz.icons.status.face-crying.png"));

			_statusIcons.Add(
				RouteListItemStatus.Transfered,
				new Gdk.Pixbuf(
					waterDeliveryApplicationAssembly,
					"Vodovoz.icons.status.face-uncertain.png"));
		}

		private DelegateCommand CopyIdCommand { get; }

		protected void CopyEntityId()
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}

		private void OnYtreeviewAddressesRowActivated(object o, RowActivatedArgs args)
		{
			_selectedItem = ytreeviewAddresses.GetSelectedObjects<RouteListKeepingItemNode>().FirstOrDefault();

			if(_selectedItem != null)
			{
				var dlg = new OrderDlg(_selectedItem.RouteListItem.Order)
				{
					HasChanges = false
				};
				dlg.SetDlgToReadOnly();
				dlg.EntitySaved += OrderSaved;
				Tab.TabParent.AddSlaveTab(Tab, dlg);
			}
		}

		private void OrderSaved(object sender, EntitySavedEventArgs e)
		{
			if(!(sender is OrderDlg dlg))
			{
				return;
			}
			
			dlg.EntitySaved -= OrderSaved;
			var address =
				ViewModel.Entity.Addresses.FirstOrDefault(x => x.Order.Id == e.GetEntity<Order>().Id);

			if(address != null)
			{
				ViewModel.UoW.Session.Refresh(address.Order);
			}
		}

		public bool CanClose()
		{
			return ViewModel.CanClose();
		}
	}
}
