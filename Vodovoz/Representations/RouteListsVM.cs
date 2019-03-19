using System;
using System.Collections.Generic;
using System.Linq;
using Dialogs.Logistic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Tdi.Gtk;
using QS.Utilities.Text;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModel
{
	public class RouteListsVM : RepresentationModelEntityBase<RouteList, RouteListsVMNode>
	{
		public RouteListsFilter Filter {
			get => RepresentationFilter as RouteListsFilter;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			RouteListsVMNode resultAlias = null;
			RouteList routeListAlias = null;
			DeliveryShift shiftAlias = null;

			Car carAlias = null;
			Employee driverAlias = null;
			Subdivision subdivisionAlias = null;
			GeographicGroup geographicGroupsAlias = null;

			var query = UoW.Session.QueryOver(() => routeListAlias);

			if(Filter.RestrictStatus != null) {
				query.Where(o => o.Status == Filter.RestrictStatus);
			}

			if(Filter.OnlyStatuses != null) {
				query.WhereRestrictionOn(o => o.Status).IsIn(Filter.OnlyStatuses);
			}

			if(Filter.RestrictShift != null) {
				query.Where(o => o.Shift == Filter.RestrictShift);
			}

			if(Filter.RestrictStartDate != null) {
				query.Where(o => o.Date >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null) {
				query.Where(o => o.Date <= Filter.RestrictEndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(Filter.RestrictGeographicGroup != null) {
				query.Left.JoinAlias(o => o.GeographicGroups, () => geographicGroupsAlias)
					 .Where(() => geographicGroupsAlias.Id == Filter.RestrictGeographicGroup.Id);
			}

			//логика фильтра ТС
			switch(Filter.RestrictTransport) {
				case RLFilterTransport.Mercenaries:
					query.Where(() => !carAlias.IsCompanyHavings && !carAlias.IsRaskat); break;
				case RLFilterTransport.Raskat:
					query.Where(() => carAlias.IsRaskat); break;
				case RLFilterTransport.Largus:
					query.Where(() => carAlias.IsCompanyHavings && carAlias.TypeOfUse == CarTypeOfUse.Largus); break;
				case RLFilterTransport.GAZelle:
					query.Where(() => carAlias.IsCompanyHavings && carAlias.TypeOfUse == CarTypeOfUse.GAZelle); break;
				case RLFilterTransport.Waggon:
					query.Where(() => carAlias.IsCompanyHavings && carAlias.TypeOfUse == CarTypeOfUse.Truck); break;
				case RLFilterTransport.Others:
					query.Where(() => carAlias.IsCompanyHavings && carAlias.TypeOfUse == CarTypeOfUse.Other); break;
				default: break;
			}

			#region для ускорения редактора
			var result = query
				.JoinAlias(o => o.Shift, () => shiftAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(o => o.Car, () => carAlias)
				.JoinAlias(o => o.Driver, () => driverAlias)
				.JoinAlias(o => o.ClosingSubdivision, () => subdivisionAlias)
				.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => routeListAlias.Status).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.ShiftName)
				   .Select(() => carAlias.Model).WithAlias(() => resultAlias.CarModel)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
				   .Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
				   .Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
				   .Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
				   .Select(() => routeListAlias.ClosingComment).WithAlias(() => resultAlias.ClosinComments)
				   .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.ClosingSubdivision)
				   .Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => resultAlias.NotFullyLoaded)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListsVMNode>())
				.List<RouteListsVMNode>();
			#endregion

			SetItemsSource(result);
		}

		#region для ускорения работы редактора
		IColumnsConfig columnsConfig = FluentColumnsConfig<RouteListsVMNode>.Create()
			.AddColumn("Номер")
				.AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Дата")
				.AddTextRenderer(node => node.Date.ToString("d"))
			.AddColumn("Смена")
				.AddTextRenderer(node => node.ShiftName)
			.AddColumn("Статус")
				.AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
			.AddColumn("Водитель и машина")
				.AddTextRenderer(node => node.DriverAndCar)
			.AddColumn("Сдается в кассу")
				.AddTextRenderer(node => node.ClosingSubdivision)
			.AddColumn("Комментарий по закрытию")
				.AddTextRenderer(node => node.ClosinComments)
			.RowCells()
				.AddSetter<CellRendererText>((c, n) => c.Foreground = n.NotFullyLoaded ? "Orange" : "Black")
			.Finish();

		#endregion
		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject) => true;

		#endregion

		public RouteListsVM(RouteListsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public RouteListsVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new RouteListsFilter(UoW);
		}

		public RouteListsVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
			if(Filter == null)
				Filter = new RouteListsFilter(UoW);
		}

		public override bool PopupMenuExist => true;

		private RepresentationSelectResult[] lastMenuSelected;
		RouteList selectedRouteList;

		private List<RouteListStatus> KeepingDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.EnRoute,
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		private List<RouteListStatus> ControlDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.InLoading
			};

		private List<RouteListStatus> TakingMoneyStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck
			};

		private List<RouteListStatus> ClosingDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		private List<RouteListStatus> MileageCheckDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		private List<RouteListStatus> CanDeletedStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.New,
				RouteListStatus.Confirmed,
				RouteListStatus.InLoading
			};

		private List<RouteListStatus> FuelIssuingStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.New,
				RouteListStatus.Confirmed,
				RouteListStatus.InLoading,
				RouteListStatus.EnRoute,
				RouteListStatus.OnClosing,
				RouteListStatus.MileageCheck
			};

		public override Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			lastMenuSelected = selected;

			#region получение и обновление выделенного МЛ
			var routeListId = lastMenuSelected.Select(x => x.EntityId)
											  .FirstOrDefault();

			selectedRouteList = UoW.Session.QueryOver<RouteList>()
										   .Where(x => x.Id == routeListId)
										   .List()
										   .FirstOrDefault();
			UoW.Session.Refresh(selectedRouteList);
			#endregion

			Menu popupMenu = new Menu();

			MenuItem menuItemRouteListOpenTrack = new MenuItem("Открыть трек");
			menuItemRouteListOpenTrack.Activated += MenuItemRouteListOpenTrack_Activated;
			popupMenu.Add(menuItemRouteListOpenTrack);

			popupMenu.Add(new SeparatorMenuItem());

			MenuItem menuItemRouteListCreateDlg = new MenuItem("Открыть диалог создания");
			menuItemRouteListCreateDlg.Activated += MenuItemRouteListCreateDlg_Activated;
			popupMenu.Add(menuItemRouteListCreateDlg);

			MenuItem menuItemRouteListControlDlg = new MenuItem("Отгрузка со склада");
			menuItemRouteListControlDlg.Activated += MenuItemRouteListControlDlg_Activated;
			menuItemRouteListControlDlg.Sensitive = selected.Any(x =>
				ControlDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListControlDlg);

			MenuItem menuItemRouteListSendToLoading = new MenuItem("Отправить МЛ на погрузку") {
				Sensitive = selected.Any((x) => (x.VMNode as RouteListsVMNode).StatusEnum == RouteListStatus.Confirmed)
			};
			menuItemRouteListSendToLoading.Activated += (sender, e) => {
				bool isSlaveTabActive = false;
				var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
				var routeLists = UoW.Session.QueryOver<RouteList>()
					.Where(x => x.Id.IsIn(routeListIds))
					.List();
				routeLists.Where((arg) => arg.Status == RouteListStatus.Confirmed).ToList().ForEach((routeList) => {
					if(TDIMain.MainNotebook.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null) {
						MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
						isSlaveTabActive = true;
						return;
					}
					foreach(var address in routeList.Addresses) {
						if(address.Order.OrderStatus < Domain.Orders.OrderStatus.OnLoading)
							address.Order.ChangeStatus(Domain.Orders.OrderStatus.OnLoading);
					}

					routeList.ChangeStatus(RouteListStatus.InLoading);
					UoW.Save(routeList);
				});

				if(isSlaveTabActive)
					return;

				foreach(var rlNode in lastMenuSelected) {
					var node = (rlNode.VMNode as RouteListsVMNode);
					if(node != null)
						node.StatusEnum = RouteListStatus.InLoading;
				}
				UoW.Commit();
			};
			popupMenu.Add(menuItemRouteListSendToLoading);

			MenuItem menuItemRouteListKeepingDlg = new MenuItem("Открыть диалог ведения");
			menuItemRouteListKeepingDlg.Activated += MenuItemRouteListKeepingDlg_Activated;
			menuItemRouteListKeepingDlg.Sensitive = selected.Any(x =>
				KeepingDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListKeepingDlg);

			MenuItem menuItemRouteListClosingDlg = new MenuItem("Открыть диалог закрытия");
			menuItemRouteListClosingDlg.Activated += MenuItemRouteListClosingDlg_Activated;
			menuItemRouteListClosingDlg.Sensitive = selected.Any(x =>
				ClosingDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListClosingDlg);

			MenuItem menuItemRouteListMileageCheckDlg = new MenuItem("Открыть диалог проверки километража");
			menuItemRouteListMileageCheckDlg.Activated += MenuItemRouteListMileageCheckDlg_Activated;
			menuItemRouteListMileageCheckDlg.Sensitive = selected.Any(x =>
				MileageCheckDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListMileageCheckDlg);

			popupMenu.Add(new SeparatorMenuItem());

			MenuItem menuItemDeleteRouteList = new MenuItem("Удалить МЛ");
			menuItemDeleteRouteList.Activated += MenuItemRouteListDelete_Activated;
			menuItemDeleteRouteList.Sensitive = selected.Any(x =>
				CanDeletedStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemDeleteRouteList);

			MenuItem menuItemRouteListFuelIssuingDlg = new MenuItem("Выдать топливо");
			menuItemRouteListFuelIssuingDlg.Activated += MenuItemRouteListFuelIssuing_Activated;
			menuItemRouteListFuelIssuingDlg.Sensitive = selected.Any(x =>
				FuelIssuingStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListFuelIssuingDlg);

			return popupMenu;
		}

		void MenuItemRouteListOpenTrack_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null) {
				TrackOnMapWnd track = new TrackOnMapWnd(selectedRouteList.Id);
				track.Show();
			}
		}

		void MenuItemRouteListMileageCheckDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null && MileageCheckDlgStatuses.Contains(selectedRouteList.Status))
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListMileageCheckDlg(selectedRouteList.Id)
				);
		}

		void MenuItemRouteListClosingDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null && ClosingDlgStatuses.Contains(selectedRouteList.Status))
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListClosingDlg(selectedRouteList.Id)
				);
		}

		void MenuItemRouteListControlDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null && ControlDlgStatuses.Contains(selectedRouteList.Status))
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListControlDlg(selectedRouteList.Id)
				);
		}


		void MenuItemRouteListKeepingDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null && KeepingDlgStatuses.Contains(selectedRouteList.Status))
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListKeepingDlg(selectedRouteList.Id)
				);
		}

		void MenuItemRouteListCreateDlg_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null)
				MainClass.MainWin.TdiMain.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(selectedRouteList.Id),
					() => new RouteListCreateDlg(selectedRouteList.Id)
				);
		}

		void MenuItemRouteListDelete_Activated(object sender, EventArgs e)
		{
			var objectType = typeof(RouteList);
			if(selectedRouteList != null && OrmMain.DeleteObject(objectType, selectedRouteList.Id))
				this.UpdateNodes();
		}

		void MenuItemRouteListFuelIssuing_Activated(object sender, EventArgs e)
		{
			if(selectedRouteList != null) {
				var routeListId = selectedRouteList.Id;
				var RouteList = UoW.GetById<RouteList>(routeListId);
				MainClass.MainWin.TdiMain.OpenTab(
						DialogHelper.GenerateDialogHashName<RouteList>(routeListId),
						() => new FuelDocumentDlg(RouteList)
					);
			}
		}
	}

	public class RouteListsVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		public RouteListStatus StatusEnum { get; set; }

		public string ShiftName { get; set; }

		public DateTime Date { get; set; }

		public string DriverSurname { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }

		public string Driver => PersonHelper.PersonFullName(DriverSurname, DriverName, DriverPatronymic);

		public string CarModel { get; set; }

		public string CarNumber { get; set; }

		[UseForSearch]
		public string DriverAndCar => string.Format("{0} - {1} ({2})", Driver, CarModel, CarNumber);
		public string ClosinComments { get; set; }
		public string ClosingSubdivision { get; set; }
		public bool NotFullyLoaded { get; set; }
	}
}