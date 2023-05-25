using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.GtkWidgets;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrderView : WidgetOnDialogBase
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository = new DeliveryScheduleRepository();
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		private readonly ICommonServices _commonServices = ServicesConfig.CommonServices;
		private readonly IOrderRepository _orderRepository = new OrderRepository();

		private Order _newOrder = null;
		private Order _oldOrder = null;
		private bool _routeListDoesNotExist = false;
		private string _initialProcDepartmentName = String.Empty;
		private IList<GuiltyInUndelivery> _initialGuiltyList = new List<GuiltyInUndelivery>();
		private UndeliveredOrder _undelivery;
		private bool _canChangeProblemSource = false;

		public Func<bool> isSaved;
		public IUnitOfWork UoW { get; set; }
		public UndeliveredOrderView() => this.Build();

		public void OnTabAdded()
		{
			//если новый недовоз без выбранного недовезённого заказа
			if(UoW.IsNew && _undelivery.OldOrder == null)
			{//открыть окно выбора недовезённого заказа
				evmeOldUndeliveredOrder.OpenSelectDialog("Выбор недовезённого заказа");
			}
		}

		public void ConfigureDlg(IUnitOfWork uow, UndeliveredOrder undelivery)
		{
			Sensitive = false;
			evmeOldUndeliveredOrder.Changed += OnUndeliveredOrderChanged;

			_canChangeProblemSource = _commonServices.PermissionService.ValidateUserPresetPermission("can_change_undelivery_problem_source", _commonServices.UserService.CurrentUserId);
			_undelivery = undelivery;
			UoW = uow;
			_oldOrder = undelivery.OldOrder;
			_newOrder = undelivery.NewOrder;
			if(undelivery.Id > 0 && undelivery.InProcessAtDepartment != null)
				_initialProcDepartmentName = undelivery.InProcessAtDepartment.Name;
			if(undelivery.Id > 0){
				foreach(GuiltyInUndelivery g in undelivery.ObservableGuilty) {
					_initialGuiltyList.Add(
						new GuiltyInUndelivery {
							Id = g.Id,
							UndeliveredOrder = g.UndeliveredOrder,
							GuiltySide = g.GuiltySide,
							GuiltyDepartment = g.GuiltyDepartment
						}
					);
				}
			}
			List<OrderStatus> hiddenStatusesList = new List<OrderStatus>();
			var grantedStatusesArray = _orderRepository.GetStatusesForOrderCancelation();
			foreach(OrderStatus status in Enum.GetValues(typeof(OrderStatus))) {
				if(!grantedStatusesArray.Contains(status))
					hiddenStatusesList.Add(status);
			}
			var filterOrders = new OrderJournalFilterViewModel(new CounterpartyJournalFactory(), new DeliveryPointJournalFactory(), new EmployeeJournalFactory());
			filterOrders.SetAndRefilterAtOnce(x => x.HideStatuses = hiddenStatusesList.Cast<Enum>().ToArray());
			evmeOldUndeliveredOrder.Changed += (sender, e) => {
				_oldOrder = undelivery.OldOrder;
				lblInfo.Markup = undelivery.GetOldOrderInfo(_orderRepository);
				if(undelivery.Id <= 0)
					undelivery.OldOrderStatus = _oldOrder.OrderStatus;
				_routeListDoesNotExist = _oldOrder != null && (undelivery.OldOrderStatus == OrderStatus.NewOrder
													   || undelivery.OldOrderStatus == OrderStatus.Accepted
													   || undelivery.OldOrderStatus == OrderStatus.WaitForPayment);

				guiltyInUndeliveryView.ConfigureWidget(UoW, undelivery, !_routeListDoesNotExist);
				SetSensitivities();
				SetVisibilities();
				GetFines();
				RemoveItemsFromEnums();
			};
			var orderFactory = new OrderSelectorFactory(filterOrders);
			evmeOldUndeliveredOrder.SetEntityAutocompleteSelectorFactory(orderFactory.CreateOrderAutocompleteSelectorFactory());
			evmeOldUndeliveredOrder.Binding.AddBinding(undelivery, x => x.OldOrder, x => x.Subject).InitializeFromSource();
			evmeOldUndeliveredOrder.CanEditReference =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");

			yDateDriverCallTime.Binding.AddBinding(undelivery, t => t.DriverCallTime, w => w.DateOrNull).InitializeFromSource();
			if(undelivery.Id <= 0)
				yDateDriverCallTime.DateOrNull = DateTime.Now;

			yEnumCMBDriverCallPlace.ItemsEnum = typeof(DriverCallType);
			yEnumCMBDriverCallPlace.Binding.AddBinding(undelivery, p => p.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			yDateDispatcherCallTime.Binding.AddBinding(undelivery, t => t.DispatcherCallTime, w => w.DateOrNull).InitializeFromSource();
			if(undelivery.Id <= 0)
				yDateDispatcherCallTime.DateOrNull = DateTime.Now;

			var roboatsSettings = new RoboatsSettings(new SettingsController(UnitOfWorkFactory.GetDefaultFactory));
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var deliveryScheduleRepository = new DeliveryScheduleRepository();
			var fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var deliveryScheduleJournalFactory = new DeliveryScheduleJournalFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, deliveryScheduleRepository, _roboatsViewModelFactory);
			entryNewDeliverySchedule.SetEntityAutocompleteSelectorFactory(deliveryScheduleJournalFactory);
			entryNewDeliverySchedule.Binding.AddBinding(undelivery, s => s.NewDeliverySchedule, w => w.Subject).InitializeFromSource();
			entryNewDeliverySchedule.Sensitive = false;

			SetLabelsAcordingToNewOrder();

			yEnumCMBStatus.ItemsEnum = typeof(UndeliveryStatus);
			yEnumCMBStatus.SelectedItem = undelivery.UndeliveryStatus;
			yEnumCMBStatus.EnumItemSelected += (s, e) => {
				SetSensitivities();
				undelivery.SetUndeliveryStatus((UndeliveryStatus)e.SelectedItem);
			};

			yentInProcessAtDepartment.SubjectType = typeof(Subdivision);
			yentInProcessAtDepartment.Binding.AddBinding(undelivery, d => d.InProcessAtDepartment, w => w.Subject).InitializeFromSource();
			yentInProcessAtDepartment.ChangedByUser += (s, e) => {
				undelivery.AddCommentToTheField(
					UoW,
					CommentedFields.Reason,
					String.Format(
						"сменил(а) \"в работе у отдела\" \nс \"{0}\" на \"{1}\"",
						_initialProcDepartmentName,
						undelivery.InProcessAtDepartment.Name
					)
				);
			};

			if(undelivery.Id <= 0 && undelivery.InProcessAtDepartment == null)
			{
				yentInProcessAtDepartment.Subject = _subdivisionRepository.GetQCDepartment(UoW);
			}

			var employeeFactory = new EmployeeJournalFactory();
			evmeRegisteredBy.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeRegisteredBy.Binding.AddBinding(undelivery, s => s.EmployeeRegistrator, w => w.Subject).InitializeFromSource();

			yEnumCMBDriverCallPlace.EnumItemSelected += CMBSelectedItemChanged;

			txtReason.Binding.AddBinding(undelivery, u => u.Reason, w => w.Buffer.Text).InitializeFromSource();

			lblInfo.Markup = undelivery.GetOldOrderInfo(_orderRepository);

			yenumcomboboxTransferType.ItemsEnum = typeof(TransferType);
			yenumcomboboxTransferType.Binding.AddBinding(undelivery, u => u.OrderTransferType, w => w.SelectedItemOrNull).InitializeFromSource();

			comboProblemSource.SetRenderTextFunc<UndeliveryProblemSource>(k => k.GetFullName);
			comboProblemSource.Binding.AddBinding(undelivery, u => u.ProblemSourceItems, w => w.ItemsList).InitializeFromSource();
			comboProblemSource.Binding.AddBinding(undelivery, u => u.ProblemSource, w => w.SelectedItem).InitializeFromSource();
			comboProblemSource.Sensitive = _canChangeProblemSource;

			comboTransferAbsenceReason.SetRenderTextFunc<UndeliveryTransferAbsenceReason>(u => u.Name);
			comboTransferAbsenceReason.Binding.AddBinding(undelivery, u => u.UndeliveryTransferAbsenceReasonItems, w => w.ItemsList).InitializeFromSource();
			comboTransferAbsenceReason.Binding.AddBinding(undelivery, u => u.UndeliveryTransferAbsenceReason, w => w.SelectedItem).InitializeFromSource();
			comboTransferAbsenceReason.Sensitive = _canChangeProblemSource;

			yTreeFines.ColumnsConfig = ColumnsConfigFactory.Create<FineItem>()
				.AddColumn("Номер").AddTextRenderer(node => node.Fine.Id.ToString())
				.AddColumn("Сотудники").AddTextRenderer(node => node.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Money))
				.Finish();

			yenumcomboboxTransferType.Visible = undelivery?.NewOrder != null;

			undelivery.PropertyChanged += (sender, e) => {
				if(e.PropertyName != "NewOrder")
					return;

				if(undelivery.NewOrder == null) {
					yenumcomboboxTransferType.Visible = false;
					undelivery.OrderTransferType = null;
					return;
				}

				yenumcomboboxTransferType.Visible = true;
			};

			GetFines();
			SetVisibilities();
			SetSensitivities();
		}

		private void OnUndeliveredOrderChanged(object sender, EventArgs e)
		{
			this.Sensitive = true;
		}

		void GetFines()
		{
			List<FineItem> fineItems = new List<FineItem>();
			foreach(Fine f in _undelivery.Fines)
				foreach(FineItem i in f.Items)
					fineItems.Add(i);
			yTreeFines.ItemsDataSource = fineItems;
		}

		private void SetLabelsAcordingToNewOrder()
		{
			lblTransferDate.Text = _undelivery.NewOrder == null ?
				"Заказ не\nсоздан" :
				_undelivery.NewOrder.Title + " на сумму " + String.Format(CurrencyWorks.GetShortCurrencyString(_undelivery.NewOrder.OrderSum));
			btnNewOrder.Label = _undelivery.NewOrder == null ? "Создать новый заказ" : "Открыть заказ";

			SetVisibilities();
		}

		void RemoveItemsFromEnums()
		{
			//удаляем статус "закрыт" из списка, если недовоз не закрыт и нет прав на их закрытие
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_undeliveries") && _undelivery.UndeliveryStatus != UndeliveryStatus.Closed) {
				yEnumCMBStatus.AddEnumToHideList(new Enum[] { UndeliveryStatus.Closed });
				yEnumCMBStatus.SelectedItem = (UndeliveryStatus)_undelivery.UndeliveryStatus;
			}
		}

		void SetVisibilities()
		{
			lblDriverCallPlace.Visible = yEnumCMBDriverCallPlace.Visible = !_routeListDoesNotExist;
			lblDriverCallTime.Visible = yDateDriverCallTime.Visible = _undelivery.DriverCallType != DriverCallType.NoCall;
			btnChooseOrder.Visible = _undelivery.NewOrder == null;
			lblTransferDate.Visible = _undelivery.NewOrder != null;
		}

		void SetSensitivities()
		{
			bool hasPermissionOrNew = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_undeliveries") || _undelivery.Id == 0;

			//основные поля доступны если есть разрешение или это новый недовоз,
			//выбран старый заказ и статус недовоза не "Закрыт"
			yEnumCMBDriverCallPlace.Sensitive =
				yDateDriverCallTime.Sensitive =
					yDateDispatcherCallTime.Sensitive =
						evmeRegisteredBy.Sensitive =
							vbxReasonAndFines.Sensitive = (
								_undelivery.OldOrder != null
								&& hasPermissionOrNew
								&& _undelivery.UndeliveryStatus != UndeliveryStatus.Closed
							);

			//выбор старого заказа доступен, если есть разрешение или это новый недовоз и не выбран старый заказ
			hbxUndelivery.Sensitive = _undelivery.OldOrder == null && hasPermissionOrNew;

			//можем менять статус, если есть права или нет прав и статус не "закрыт"
			hbxStatus.Sensitive = (
				(
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_undeliveries")
					|| _undelivery.UndeliveryStatus != UndeliveryStatus.Closed
				)
				&& _undelivery.OldOrder != null
			);

			//кнопки для выбора/создания нового заказа и группа "В работе у отдела"
			//доступны всегда, если статус недовоза не "Закрыт"
			hbxInProcessAtDepartment.Sensitive =
				hbxForNewOrder.Sensitive = _undelivery.UndeliveryStatus != UndeliveryStatus.Closed;
		}

		void AddAutocomment()
		{
			#region удаление дублей из спсика ответственных
			IList<GuiltyInUndelivery> guiltyTempList = new List<GuiltyInUndelivery>();
			foreach(GuiltyInUndelivery g in _undelivery.ObservableGuilty)
				guiltyTempList.Add(g);
			_undelivery.ObservableGuilty.Clear();
			foreach(GuiltyInUndelivery g in guiltyTempList.Distinct())
				_undelivery.ObservableGuilty.Add(g);
			#endregion

			#region формирование и добавление автокомментарния об изменении списка ответственных
			if(_undelivery.Id > 0) {
				IList<GuiltyInUndelivery> removedGuiltyList = new List<GuiltyInUndelivery>();
				IList<GuiltyInUndelivery> addedGuiltyList = new List<GuiltyInUndelivery>();
				IList<GuiltyInUndelivery> toRemoveFromBoth = new List<GuiltyInUndelivery>();
				foreach(GuiltyInUndelivery r in _initialGuiltyList)
					removedGuiltyList.Add(r);
				foreach(GuiltyInUndelivery a in _undelivery.ObservableGuilty)
					addedGuiltyList.Add(a);
				foreach(GuiltyInUndelivery gu in addedGuiltyList) {
					foreach(var g in removedGuiltyList)
						if(gu == g)
							toRemoveFromBoth.Add(g);
				}
				foreach(var r in toRemoveFromBoth) {
					addedGuiltyList.Remove(r);
					removedGuiltyList.Remove(r);
				}
				StringBuilder sb = new StringBuilder();
				if(addedGuiltyList.Any()) {
					sb.AppendLine("добавил(а) ответственных:");
					foreach(var a in addedGuiltyList)
						sb.AppendLine(String.Format("\t- {0}", a));
				}
				if(removedGuiltyList.Any()) {
					sb.AppendLine("удалил(а) ответственных:");
					foreach(var r in removedGuiltyList)
						sb.AppendLine(String.Format("\t- {0}", r));
				}
				string text = sb.ToString().Trim();
				if(sb.Length > 0)
					_undelivery.AddCommentToTheField(UoW, CommentedFields.Reason, text);
			}
			#endregion
		}

		public void BeforeSaving()
		{
			AddAutocomment();
			_undelivery.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			_undelivery.LastEditedTime = DateTime.Now;
			if(_undelivery.DriverCallType == DriverCallType.NoCall) {
				_undelivery.DriverCallTime = null;
				_undelivery.DriverCallNr = null;
			}
		}

		protected void CMBSelectedItemChanged(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			SetVisibilities();
		}

		protected void OnBtnNewOrderClicked(object sender, EventArgs e)
		{
			if(_undelivery.NewOrder == null) {
				CreateNewOrder(_oldOrder);
			} else {
				OpenOrder(_newOrder);
			}
		}

		protected void OnBtnChooseOrderClicked(object sender, EventArgs e)
		{
			var filter = new OrderJournalFilterViewModel(new CounterpartyJournalFactory(), new DeliveryPointJournalFactory(), new EmployeeJournalFactory());
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCounterparty = _oldOrder.Client,
				x => x.HideStatuses = new Enum[] { OrderStatus.WaitForPayment }
			);
			var orderFactory = new OrderSelectorFactory(filter);
			var orderJournal = orderFactory.CreateOrderJournalViewModel();
			orderJournal.SelectionMode = JournalSelectionMode.Single;

			MyTab.TabParent.AddTab(orderJournal, MyTab, false);

			orderJournal.OnEntitySelectedResult += (s, ea) =>
			{
				var selectedId = ea.SelectedNodes.FirstOrDefault()?.Id ?? 0;
				if(selectedId == 0)
				{
					return;
				}
				if(_oldOrder.Id == selectedId) {
					MessageDialogHelper.RunErrorDialog("Перенесённый заказ не может совпадать с недовезённым!");
					OnBtnChooseOrderClicked(sender, ea);
					return;
				}
				_newOrder = _undelivery.NewOrder = UoW.GetById<Order>(selectedId);
				_newOrder.Author = this._oldOrder.Author;
				SetLabelsAcordingToNewOrder();
				_undelivery.NewDeliverySchedule = _newOrder.DeliverySchedule;
				if ((_oldOrder.PaymentType == Domain.Client.PaymentType.ByCard) &&
					(_oldOrder.OrderSum == _newOrder.OrderSum) &&
					MessageDialogHelper.RunQuestionDialog("Перенести на выбранный заказ Оплату по Карте?")){
					_newOrder.PaymentType = _oldOrder.PaymentType;
					_newOrder.OnlineOrder = _oldOrder.OnlineOrder;
					_newOrder.PaymentByCardFrom = _oldOrder.PaymentByCardFrom;
				}
			};
		}

		/// <summary>
		/// Создаёт новый заказ, копируя поля существующего.
		/// </summary>
		/// <param name="order">Заказ, из которого копируются свойства.</param>
		void CreateNewOrder(Order order)
		{
			var dlg = new OrderDlg();
			dlg.CopyOrderFrom(order.Id);
			MyTab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Order>(dlg.Entity.Id),
				() => dlg
			);

			dlg.TabClosed += (sender, e) => {
				if(sender is OrderDlg) {
					Order o = (sender as OrderDlg).Entity;
					if(o.Id > 0) {
						_newOrder = _undelivery.NewOrder = o;
						SetLabelsAcordingToNewOrder();
						_undelivery.NewDeliverySchedule = _newOrder.DeliverySchedule;
					}
				}
			};
		}

		/// <summary>
		/// Открытие существующего заказа
		/// </summary>
		/// <param name="order">Заказ, который требуется открыть</param>
		void OpenOrder(Order order)
		{
			if(MessageDialogHelper.RunQuestionDialog("Требуется сохранить недовоз. Сохранить?")) {
				UoW.Save();
				UoW.Commit();
				var dlg = new OrderDlg(order);
				MyTab.TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Order>(order.Id),
					() => dlg
				);
			}
		}

		protected void OnYEnumCMBDriverCallPlaceEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<UndeliveredOrder>()
							.Where(x => x.Id == _undelivery.Id)
							.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != (DriverCallType)yEnumCMBDriverCallPlace.SelectedItem) {
				var max = UoW.Session.QueryOver<UndeliveredOrder>().Select(NHibernate.Criterion.Projections.Max<UndeliveredOrder>(x => x.DriverCallNr)).SingleOrDefault<int>();
				if(max != 0)
					_undelivery.DriverCallNr = max + 1;
				else
					_undelivery.DriverCallNr = 1;
			}
		}

		protected void OnButtonAddFineClicked(object sender, EventArgs e)
		{
			if(_undelivery.Id == 0) {
				if(QSOrmProject.CommonDialogs.SaveBeforeCreateSlaveEntity(_undelivery.GetType(), typeof(Fine))) {
					var saved = isSaved?.Invoke();
					if(!saved.HasValue || !saved.Value)
						return;
				} else
					return;
			}

			FineDlg fineDlg;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				fineDlg = new FineDlg(uow.GetById<UndeliveredOrder>(_undelivery.Id));
			}

			MyTab.TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Fine>(_undelivery.Id),
				() => fineDlg
			);

			var address = new RouteListItemRepository().GetRouteListItemForOrder(UoW, _undelivery.OldOrder);

			if (address != null)
				fineDlg.Entity.AddAddress(address);

			fineDlg.EntitySaved += (object sender2, QS.Tdi.EntitySavedEventArgs args) => {
				_undelivery.Fines.Add(args.Entity as Fine);

				GetFines();
			};
		}
	}
}
