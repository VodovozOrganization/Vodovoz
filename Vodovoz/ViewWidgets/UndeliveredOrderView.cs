using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NHibernate.Util;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.ViewModel;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrderView : WidgetOnDialogBase
	{
		Order newOrder = null;
		Order oldOrder = null;
		IUnitOfWork uow;
		bool routeListDoesNotExist = false;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		UndeliveryStatus initialStatus;
		UndeliveredOrder undelivery;

		public UndeliveredOrderView()
		{
			this.Build();
		}

		public void OnTabAdded()
		{
			//если новый недовоз без выбранного недовезённого заказа
			if(UoW.IsNew && undelivery.OldOrder == null)
				//открыть окно выбора недовезённого заказа
				yEForUndeliveredOrder.OpenSelectDialog("Выбор недовезённого заказа");
		}

		public void ConfigureDlg(IUnitOfWork uow, UndeliveredOrder undelivery, UndeliveryStatus initialStatus = UndeliveryStatus.InProcess)
		{
			this.initialStatus = initialStatus;
			this.undelivery = undelivery;
			UoW = uow;
			oldOrder = undelivery.OldOrder;
			newOrder = undelivery.NewOrder;
			var filterOrders = new OrdersFilter(UoW);
			List<OrderStatus> hiddenStatusesList = new List<OrderStatus>();
			var grantedStatusesArray = OrderRepository.GetStatusesForOrderCancelation();
			foreach(OrderStatus status in Enum.GetValues(typeof(OrderStatus))) {
				if(!grantedStatusesArray.Contains(status))
					hiddenStatusesList.Add(status);
			}
			filterOrders.HideStatuses = hiddenStatusesList.Cast<Enum>().ToArray();
			yEForUndeliveredOrder.Changed += (sender, e) => {
				oldOrder = undelivery.OldOrder;
				lblInfo.Markup = undelivery.GetUndeliveryInfo();
				if(undelivery.Id <= 0)
					undelivery.OldOrderStatus = oldOrder.OrderStatus;
				routeListDoesNotExist = oldOrder != null && (undelivery.OldOrderStatus == OrderStatus.NewOrder
													   || undelivery.OldOrderStatus == OrderStatus.Accepted
													   || undelivery.OldOrderStatus == OrderStatus.WaitForPayment);

				SetSensitivities();
				SetVisibilities();
				GetFines();
				RemoveItemsFromEnums();
			};
			yEForUndeliveredOrder.RepresentationModel = new OrdersVM(filterOrders);
			yEForUndeliveredOrder.Binding.AddBinding(undelivery, x => x.OldOrder, x => x.Subject).InitializeFromSource();

			yEnumCMBGuilty.ItemsEnum = typeof(GuiltyTypes);
			yEnumCMBGuilty.Binding.AddBinding(undelivery, g => g.GuiltySide, w => w.SelectedItem).InitializeFromSource();

			yDateDriverCallTime.Binding.AddBinding(undelivery, t => t.DriverCallTime, w => w.DateOrNull).InitializeFromSource();
			if(undelivery.Id <= 0)
				yDateDriverCallTime.DateOrNull = DateTime.Now;

			yEnumCMBDriverCallPlace.ItemsEnum = typeof(DriverCallType);
			yEnumCMBDriverCallPlace.Binding.AddBinding(undelivery, p => p.DriverCallType, w => w.SelectedItem).InitializeFromSource();

			yDateDispatcherCallTime.Binding.AddBinding(undelivery, t => t.DispatcherCallTime, w => w.DateOrNull).InitializeFromSource();
			if(undelivery.Id <= 0)
				yDateDispatcherCallTime.DateOrNull = DateTime.Now;

			referenceNewDeliverySchedule.ItemsQuery = DeliveryScheduleRepository.AllQuery();
			referenceNewDeliverySchedule.SetObjectDisplayFunc<DeliverySchedule>(e => e.Name);
			referenceNewDeliverySchedule.Binding.AddBinding(undelivery, s => s.NewDeliverySchedule, w => w.Subject).InitializeFromSource();
			referenceNewDeliverySchedule.Sensitive = false;

			SetLabelsAcordingToNewOrder();

			yEnumCMBStatus.ItemsEnum = typeof(UndeliveryStatus);
			yEnumCMBStatus.Binding.AddBinding(undelivery, s => s.UndeliveryStatus, w => w.SelectedItem).InitializeFromSource();
			yEnumCMBStatus.ChangedByUser += (e, s) => SetSensitivities();

			yentrySubdivision.SubjectType = typeof(Subdivision);
			yentrySubdivision.Binding.AddBinding(undelivery, g => g.GuiltyDepartment, w => w.Subject).InitializeFromSource();

			var filterRegisteredBy = new EmployeeFilter(UoW);
			filterRegisteredBy.RestrictFired = false;
			refRegisteredBy.RepresentationModel = new EmployeesVM(filterRegisteredBy);
			refRegisteredBy.Binding.AddBinding(undelivery, s => s.EmployeeRegistrator, w => w.Subject).InitializeFromSource();

			yEnumCMBDriverCallPlace.EnumItemSelected += CMBSelectedItemChanged;
			yEnumCMBGuilty.EnumItemSelected += CMBSelectedItemChanged;

			txtReason.Binding.AddBinding(undelivery, u => u.Reason, w => w.Buffer.Text).InitializeFromSource();

			lblInfo.Markup = undelivery.GetUndeliveryInfo();

			yTreeFines.ColumnsConfig = ColumnsConfigFactory.Create<FineItem>()
				.AddColumn("Номер").AddTextRenderer(node => node.Fine.Id.ToString())
				.AddColumn("Сотудники").AddTextRenderer(node => node.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Money))
				.Finish();

			GetFines();
			SetVisibilities();
			SetSensitivities();
		}

		void GetFines()
		{
			List<FineItem> fineItems = new List<FineItem>();
			undelivery.Fines.ForEach(f => f.Items.ForEach(i => fineItems.Add(i)));
			yTreeFines.ItemsDataSource = fineItems;
		}

		private void SetLabelsAcordingToNewOrder()
		{
			lblTransferDate.Text = undelivery.NewOrder == null ? "Заказ не\nсоздан" : undelivery.NewOrder.Title;
			btnNewOrder.Label = undelivery.NewOrder == null ? "Создать новый заказ" : "Открыть заказ";

			SetVisibilities();
		}

		void RemoveItemsFromEnums()
		{
			//удаляем статус "закрыт" из списка, если недовоз не закрыт и нет прав на их закрытие
			if(!QSMain.User.Permissions["can_close_undeliveries"] && undelivery.UndeliveryStatus != UndeliveryStatus.Closed) {
				yEnumCMBStatus.AddEnumToHideList(new Enum[] { UndeliveryStatus.Closed });
				yEnumCMBStatus.SelectedItem = (UndeliveryStatus)undelivery.UndeliveryStatus;
			}

			//если недовезённый заказ не был в мл, то водитель не может быть виновным
			if(routeListDoesNotExist)
				yEnumCMBGuilty.AddEnumToHideList(new Enum[] { GuiltyTypes.Driver });
		}

		void SetVisibilities()
		{
			lblGuiltyDepartment.Visible = yentrySubdivision.Visible = undelivery.GuiltySide == GuiltyTypes.Department;
			lblDriverCallPlace.Visible = yEnumCMBDriverCallPlace.Visible = !routeListDoesNotExist;
			lblDriverCallTime.Visible = yDateDriverCallTime.Visible = undelivery.DriverCallType != DriverCallType.NoCall;
			btnChooseOrder.Visible = undelivery.NewOrder == null;
			lblTransferDate.Visible = undelivery.NewOrder != null;
		}

		void SetSensitivities()
		{
			bool hasPermissionOrNew = QSMain.User.Permissions["can_edit_undeliveries"] || undelivery.Id == 0;

			//основные поля доступны если есть разрешение или это новый недовоз,
			//выбран старый заказ и статус недовоза не "Закрыт"
			yEnumCMBDriverCallPlace.Sensitive =
				yDateDriverCallTime.Sensitive =
					yDateDispatcherCallTime.Sensitive =
						refRegisteredBy.Sensitive =
							hbxReasonAndFines.Sensitive = (
								undelivery.OldOrder != null
								&& hasPermissionOrNew
								&& undelivery.UndeliveryStatus != UndeliveryStatus.Closed
							);

			//выбор старого заказа доступен, если есть разрешение или это новый недовоз и не выбран старый заказ
			hbxUndelivery.Sensitive = undelivery.OldOrder == null && hasPermissionOrNew;

			//можем менять статус, если есть права или нет прав и статус не "закрыт"
			hbxStatus.Sensitive = (
				(
					QSMain.User.Permissions["can_close_undeliveries"]
					|| undelivery.UndeliveryStatus != UndeliveryStatus.Closed
				)
				&& undelivery.OldOrder != null
			);

			//кнопки для выбора/создания нового заказа доступны всегда, если статус недовоза не "Закрыт"
			yentrySubdivision.Sensitive =
				yEnumCMBGuilty.Sensitive =
					hbxForNewOrder.Sensitive = undelivery.UndeliveryStatus != UndeliveryStatus.Closed;
		}

		/// <summary>
		/// Добавление комментариев к полям.
		/// Если не указан текст, то к каждому полю будет добавлятся комментарий,
		/// в соответствии с правилами внутри метода.
		/// </summary>
		/// <param name="field">Комментируемое поле</param>
		/// <param name="text">Текст комментария (опционально)</param>
		void AddComment(CommentedFields field, string text = null)
		{
			switch(field) {
				case CommentedFields.Status:
					if(text == null && initialStatus != undelivery.UndeliveryStatus)
						text = String.Format(
							"сменил(а) статус недовоза\nс \"{0}\" на \"{1}\"",
							initialStatus.GetEnumTitle(),
							undelivery.UndeliveryStatus.GetEnumTitle()
						);
					break;
				default:
					break;
			}
			if(text == null)
				return;

			UndeliveredOrderComment comment = new UndeliveredOrderComment {
				Comment = text,
				CommentDate = DateTime.Now,
				CommentedField = field,
				Employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW),
				UndeliveredOrder = undelivery
			};

			UoW.Save(comment);
		}

		public void SaveChanges()
		{
			undelivery.LastEditor = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			undelivery.LastEditedTime = DateTime.Now;
			AddComment(CommentedFields.Status);
			BeforeSave();
		}

		void BeforeSave()
		{
			if(undelivery.DriverCallType == DriverCallType.NoCall) {
				undelivery.DriverCallTime = null;
				undelivery.DriverCallNr = null;
			}
		}

		protected void CMBSelectedItemChanged(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(undelivery.GuiltySide != GuiltyTypes.Department)
				undelivery.GuiltyDepartment = null;
			SetVisibilities();
		}

		protected void OnBtnNewOrderClicked(object sender, EventArgs e)
		{
			if(undelivery.NewOrder == null) {
				CreateNewOrder(oldOrder);
			} else {
				OpenOrder(newOrder);
			}
		}

		protected void OnBtnChooseOrderClicked(object sender, EventArgs e)
		{
			var filter = new OrdersFilter(UnitOfWorkFactory.CreateWithoutRoot());
			filter.HideStatuses = new Enum[] { OrderStatus.WaitForPayment };
			filter.RestrictCounterparty = oldOrder.Client;
			ReferenceRepresentation dlg = new ReferenceRepresentation(new OrdersVM(filter));
			dlg.Mode = OrmReferenceMode.Select;
			dlg.ButtonMode = ReferenceButtonMode.None;

			MyTab.TabParent.AddTab(dlg, MyTab, false);

			dlg.ObjectSelected += (s, ea) => {
				if(oldOrder.Id == ea.ObjectId){
					MessageDialogWorks.RunErrorDialog("Перенесённый заказ не может совпадать с недовезённым!");
					OnBtnChooseOrderClicked(sender, ea);
					return;
				}
				newOrder = undelivery.NewOrder = UoW.GetById<Order>(ea.ObjectId);
				SetLabelsAcordingToNewOrder();
				undelivery.NewDeliverySchedule = newOrder.DeliverySchedule;
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
				OrmMain.GenerateDialogHashName<Domain.Orders.Order>(dlg.Entity.Id),
				() => dlg
			);

			dlg.CloseTab += (sender, e) => {
				if(sender is OrderDlg) {
					Order o = (sender as OrderDlg).Entity;
					if(o.Id > 0) {
						newOrder = undelivery.NewOrder = o;
						SetLabelsAcordingToNewOrder();
						undelivery.NewDeliverySchedule = newOrder.DeliverySchedule;
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
			var dlg = new OrderDlg(order);
			MyTab.TabParent.OpenTab(
				OrmMain.GenerateDialogHashName<Domain.Orders.Order>(order.Id),
				() => dlg
			);
		}

		protected void OnYEnumCMBDriverCallPlaceEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<UndeliveredOrder>()
							.Where(x => x.Id == undelivery.Id)
							.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != (DriverCallType)yEnumCMBDriverCallPlace.SelectedItem) {
				var max = UoW.Session.QueryOver<UndeliveredOrder>().Select(NHibernate.Criterion.Projections.Max<UndeliveredOrder>(x => x.DriverCallNr)).SingleOrDefault<int>();
				if(max != 0)
					undelivery.DriverCallNr = max + 1;
				else
					undelivery.DriverCallNr = 1;
			}
		}
	}
}