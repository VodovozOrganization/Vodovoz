using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using QS.Project.Services;
using QS.Tdi;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;
using Vodovoz.ViewWidgets.Mango;

namespace Vodovoz.Logistic
{
	public partial class RouteListKeepingDlg : TabViewBase<RouteListKeepingViewModel>, ITDICloseControlTab
	{
		//2 уровня доступа к виджетам, для всех и для логистов.
		private readonly bool _allEditing;
		private readonly bool _logisticanEditing;
		private readonly bool _isUserLogist;

		private readonly Dictionary<RouteListItemStatus, Gdk.Pixbuf> _statusIcons = new Dictionary<RouteListItemStatus, Gdk.Pixbuf>();
		private RouteListKeepingItemNode _selectedItem;

		public event RowActivatedHandler OnClosingItemActivated;

		public RouteListKeepingDlg(RouteListKeepingViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		//public RouteListKeepingDlg(int id)
		//{
		//	Build();

		//	ConfigureDlg();
		//}

		//public RouteListKeepingDlg(RouteList sub) : this(sub.Id) { }

		//public RouteListKeepingDlg(int routeId, int[] selectOrderId) : this(routeId)
		//{
		//	var selectedItems = _items.Where(x => selectOrderId.Contains(x.RouteListItem.Order.Id)).ToArray();

		//	if(selectedItems.Any())
		//	{
		//		ytreeviewAddresses.SelectObject(selectedItems);
		//		var iter = ytreeviewAddresses.YTreeModel.IterFromNode(selectedItems[0]);
		//		var path = ytreeviewAddresses.YTreeModel.GetPath(iter);
		//		ytreeviewAddresses.ScrollToCell(path, ytreeviewAddresses.Columns[0], true, 0.5f, 0.5f);
		//	}
		//}
	
		private void Initialize()
		{
			buttonSave.Sensitive = _allEditing;

			//entityentryCar.ViewModel = BuildCarEntryViewModel();
			//entityentryCar.Sensitive = _logisticanEditing;

			var deliveryfreebalanceview = new DeliveryFreeBalanceView(ViewModel.DeliveryFreeBalanceViewModel);
			deliveryfreebalanceview.Binding
				.AddBinding(ViewModel.Entity,
					e => e.ObservableDeliveryFreeBalanceOperations,
					w => w.ObservableDeliveryFreeBalanceOperations)
				.InitializeFromSource();

			deliveryfreebalanceview.ShowAll();
			yhboxDeliveryFreeBalance.PackStart(deliveryfreebalanceview, true, true, 0);

			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var driverFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());

			//evmeDriver.Binding.AddBinding(
			//	ViewModel.Entity,
			//	rl => rl.Driver,
			//	widget => widget.Subject)
			//	.InitializeFromSource();

			//evmeDriver.Sensitive = _logisticanEditing;
			//evmeDriver.Changed += OnEvmeDriverChanged;

			//var forwarderFilter = new EmployeeFilterViewModel();
			//forwarderFilter.SetAndRefilterAtOnce(
			//	x => x.Status = EmployeeStatus.IsWorking,
			//	x => x.RestrictCategory = EmployeeCategory.forwarder);
			//var forwarderFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, forwarderFilter);
			//evmeForwarder.SetEntityAutocompleteSelectorFactory(forwarderFactory.CreateEmployeeAutocompleteSelectorFactory());
			//evmeForwarder.Binding.AddSource(ViewModel.Entity)
			//	.AddBinding(rl => rl.Forwarder, widget => widget.Subject)
			//	.AddFuncBinding(rl => _logisticanEditing && rl.CanAddForwarder, widget => widget.Sensitive)
			//	.InitializeFromSource();

			//evmeForwarder.Changed += ReferenceForwarder_Changed;

			//var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
			//evmeLogistician.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			//evmeLogistician.Binding.AddBinding(ViewModel.Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();
			//evmeLogistician.Sensitive = _logisticanEditing;

			speccomboShift.ItemsList = ViewModel.ActiveShifts;
			speccomboShift.Binding.AddBinding(ViewModel.Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = _logisticanEditing;

			datePickerDate.Binding.AddBinding(ViewModel.Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = _logisticanEditing;

			//ylabelLastTimeCall.Binding.AddFuncBinding(ViewModel.Entity, e => GetLastCallTime(e.LastCallTime), w => w.LabelProp).InitializeFromSource();
			yspinActualDistance.Sensitive = _allEditing;

			buttonMadeCall.Sensitive = _allEditing;

			buttonRetriveEnRoute.Sensitive = ViewModel.Entity.Status == RouteListStatus.OnClosing && _isUserLogist
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_retrieve_routelist_en_route");

			btnReDeliver.Binding.AddBinding(ViewModel.Entity, e => e.CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument, w => w.Sensitive).InitializeFromSource();

			buttonNewFine.Sensitive = _allEditing;
			buttonRefresh.Sensitive = _allEditing;

			//Заполняем иконки
			var ass = Assembly.GetAssembly(typeof(Startup));
			_statusIcons.Add(RouteListItemStatus.EnRoute, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.car.png"));
			_statusIcons.Add(RouteListItemStatus.Completed, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-smile-grin.png"));
			_statusIcons.Add(RouteListItemStatus.Overdue, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-angry.png"));
			_statusIcons.Add(RouteListItemStatus.Canceled, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-crying.png"));
			_statusIcons.Add(RouteListItemStatus.Transfered, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-uncertain.png"));

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
				.AddColumn("Статус")
					.AddPixbufRenderer(x => _statusIcons[x.Status])
					.AddEnumRenderer(node => node.Status, excludeItems: new Enum[] { RouteListItemStatus.Transfered })
					.AddSetter((c, n) => c.Editable = _allEditing && n.Status != RouteListItemStatus.Transfered)
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
					.Editable(_allEditing)
				.AddColumn("Переносы")
					.AddTextRenderer(node => node.Transferred)
				//.RowCells()
				//	.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();

			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			//ytreeviewAddresses.Selection.Changed += OnSelectionChanged;
			ytreeviewAddresses.Sensitive = _allEditing;
			ytreeviewAddresses.RowActivated += YtreeviewAddresses_RowActivated;

			//Point!
			//Заполняем телефоны

			if(ViewModel.Entity.Driver != null && ViewModel.Entity.Driver.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(ViewModel.Entity.Driver.Phones.Count + 1);
				PhonesTable1.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{ViewModel.Entity.Driver.FullName}";
				PhonesTable1.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					Label l = new Label();
					l.LabelProp = "+7 " + ViewModel.Entity.Driver.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable1.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(ViewModel.Entity.Driver.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable1.Attach(h, 1, 2, i, i + 1);
				}
			}

			if(ViewModel.Entity.Forwarder != null && ViewModel.Entity.Forwarder.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(ViewModel.Entity.Forwarder.Phones.Count + 1);
				PhonesTable2.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{ViewModel.Entity.Forwarder.FullName}";
				PhonesTable2.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					Label l = new Label();
					l.LabelProp = "+7 " + ViewModel.Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable2.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(ViewModel.Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable2.Attach(h, 1, 2, i, i + 1);
				}
			}

			//Телефон
			PhonesTable1.ShowAll();
			PhonesTable2.ShowAll();

			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = Startup.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(ViewModel.Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(ViewModel.Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			//Заполняем информацию о бутылях
			//UpdateBottlesSummaryInfo();

			//UpdateNodes();

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}

		private void YtreeviewAddresses_RowActivated(object o, RowActivatedArgs args)
		{
			_selectedItem = ytreeviewAddresses.GetSelectedObjects<RouteListKeepingItemNode>().FirstOrDefault();

			if(_selectedItem != null)
			{
				var dlg = new OrderDlg(_selectedItem.RouteListItem.Order)
				{
					HasChanges = false
				};
				dlg.SetDlgToReadOnly();
				Tab.TabParent.AddSlaveTab(Tab, dlg);
			}
		}

		public bool CanClose()
		{
			return ViewModel.CanClose();
		}

		private void SetSensetivity(bool isSensetive)
		{
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}
	}
}
