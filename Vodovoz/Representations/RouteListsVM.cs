using System;
using System.Collections.Generic;
using System.Linq;
using Dialogs.Logistic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModel
{
	public class RouteListsVM : RepresentationModelEntityBase<RouteList, RouteListsVMNode>
	{
		public RouteListsFilter Filter {
			get {
				return RepresentationFilter as RouteListsFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			RouteListsVMNode resultAlias = null;
			RouteList routeListAlias = null;
			DeliveryShift shiftAlias = null;

			Car carAlias = null;
			Employee driverAlias = null;

			var query = UoW.Session.QueryOver<RouteList>(() => routeListAlias);

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
				.SelectList(list => list
				   .Select(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => routeListAlias.Status).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.ShiftName)
				   .Select(() => carAlias.Model).WithAlias(() => resultAlias.CarModel)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
				   .Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
				   .Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
				   .Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
				   .Select(() => routeListAlias.ClosingComment).WithAlias(() => resultAlias.ClosinComments)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListsVMNode>())
				.List<RouteListsVMNode>();
			#endregion

			SetItemsSource(result);
		}

		#region для ускорения работы редактора
		IColumnsConfig columnsConfig = FluentColumnsConfig<RouteListsVMNode>.Create()
			.AddColumn("Номер").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Дата").SetDataProperty(node => node.Date.ToString("d"))
			.AddColumn("Смена").SetDataProperty(node => node.ShiftName)
			.AddColumn("Статус").SetDataProperty(node => node.StatusEnum.GetEnumTitle())
			.AddColumn("Водитель и машина").SetDataProperty(node => node.DriverAndCar)
			.AddColumn("Комментарий по закрытию").SetDataProperty(node => node.ClosinComments)
			.Finish();

		#endregion
		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject)
		{
			return true;
		}

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
				RouteListStatus.InLoading
			};

		private List<RouteListStatus> FuelIssuingStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.New,
				RouteListStatus.InLoading,
				RouteListStatus.EnRoute
			};

		public override Gtk.Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			lastMenuSelected = selected;
			Gtk.Menu popupMenu = new Gtk.Menu();

			Gtk.MenuItem menuItemRouteListOpenTrack = new Gtk.MenuItem("Открыть трек");
			menuItemRouteListOpenTrack.Activated += MenuItemRouteListOpenTrack_Activated;
			popupMenu.Add(menuItemRouteListOpenTrack);

			popupMenu.Add(new SeparatorMenuItem());

			Gtk.MenuItem menuItemRouteListCreateDlg = new Gtk.MenuItem("Открыть диалог создания");
			menuItemRouteListCreateDlg.Activated += MenuItemRouteListCreateDlg_Activated;
			popupMenu.Add(menuItemRouteListCreateDlg);

			Gtk.MenuItem menuItemRouteListControlDlg = new Gtk.MenuItem("Отгрузка со склада");
			menuItemRouteListControlDlg.Activated += MenuItemRouteListControlDlg_Activated;
			menuItemRouteListControlDlg.Sensitive = selected.Any(x =>
				ControlDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListControlDlg);

			Gtk.MenuItem menuItemRouteListKeepingDlg = new Gtk.MenuItem("Открыть диалог ведения");
			menuItemRouteListKeepingDlg.Activated += MenuItemRouteListKeepingDlg_Activated;
			menuItemRouteListKeepingDlg.Sensitive = selected.Any(x =>
				KeepingDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListKeepingDlg);

			Gtk.MenuItem menuItemRouteListClosingDlg = new Gtk.MenuItem("Открыть диалог закрытия");
			menuItemRouteListClosingDlg.Activated += MenuItemRouteListClosingDlg_Activated;
			menuItemRouteListClosingDlg.Sensitive = selected.Any(x =>
				ClosingDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListClosingDlg);

			Gtk.MenuItem menuItemRouteListMileageCheckDlg = new Gtk.MenuItem("Открыть диалог проверки километража");
			menuItemRouteListMileageCheckDlg.Activated += MenuItemRouteListMileageCheckDlg_Activated;
			menuItemRouteListMileageCheckDlg.Sensitive = selected.Any(x =>
				MileageCheckDlgStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListMileageCheckDlg);

			Gtk.MenuItem menuItemOrderOfAddressesRep = new Gtk.MenuItem("Открыть отчёт по порядку адресов в МЛ");
			menuItemOrderOfAddressesRep.Activated += MenuItemOrderOfAddressesRep_Activated;
			menuItemOrderOfAddressesRep.Sensitive = false; // NYI @Дима
			popupMenu.Add(menuItemOrderOfAddressesRep);

			popupMenu.Add(new SeparatorMenuItem());

			Gtk.MenuItem menuItemDeleteRouteList = new Gtk.MenuItem("Удалить МЛ");
			menuItemDeleteRouteList.Activated += MenuItemRouteListDelete_Activated;
			menuItemDeleteRouteList.Sensitive = selected.Any(x =>
				CanDeletedStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemDeleteRouteList);

			Gtk.MenuItem menuItemRouteListFuelIssuingDlg = new Gtk.MenuItem("Выдать топливо");
			menuItemRouteListFuelIssuingDlg.Activated += MenuItemRouteListFuelIssuing_Activated;
			menuItemRouteListFuelIssuingDlg.Sensitive = selected.Any(x =>
			FuelIssuingStatuses.Contains((x.VMNode as RouteListsVMNode).StatusEnum));
			popupMenu.Add(menuItemRouteListFuelIssuingDlg);

			return popupMenu;
		}

		void MenuItemRouteListOpenTrack_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
			foreach(var id in routeListIds) {
				TrackOnMapWnd track = new TrackOnMapWnd(id);
				track.Show();
			}
		}

		void MenuItemRouteListMileageCheckDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach(var rl in routeLists.Where(x => MileageCheckDlgStatuses.Contains(x.Status))) {
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListMileageCheckDlg(rl.Id)
				);
			}
		}

		void MenuItemRouteListClosingDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach(var rl in routeLists.Where(x => ClosingDlgStatuses.Contains(x.Status))) {
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListClosingDlg(rl.Id)
				);
			}
		}

		void MenuItemRouteListControlDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach(var rl in routeLists.Where(x => ControlDlgStatuses.Contains(x.Status))) {
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListControlDlg(rl.Id)
				);
			}
		}


		void MenuItemRouteListKeepingDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach(var rl in routeLists.Where(x => KeepingDlgStatuses.Contains(x.Status))) {
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListKeepingDlg(rl.Id)
				);
			}
		}

		void MenuItemRouteListCreateDlg_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			foreach(var routeId in routeListIds) {
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(routeId),
					() => new RouteListCreateDlg(routeId)
				);
			}
		}

		void MenuItemOrderOfAddressesRep_Activated(object sender, EventArgs e) // TODO: Сделать вывод порядка адресов в МЛ через контекстное меню в журнале МЛ. @Дима
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			foreach(var routeId in routeListIds) {
				//MainClass.MainWin.TdiMain.OpenTab(
				//	OrmMain.GenerateDialogHashName<RouteList>(routeId)// ,
				//	() => new OrderOrderOfAddressesRep (
				//);

			}
		}

		void MenuItemRouteListDelete_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var objectType = typeof(RouteList);
			if(OrmMain.DeleteObject(objectType, routeListIds[0]))
				this.UpdateNodes();
		}

		void MenuItemRouteListFuelIssuing_Activated(object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
			var RouteList = UoW.GetById<RouteList>(routeListIds[0]);
			MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(routeListIds[0]),
					() => new FuelDocumentDlg(UoW, UoW.GetById<RouteList>(routeListIds[0]))
				);
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

		public string Driver {
			get {
				return StringWorks.PersonFullName(DriverSurname, DriverName, DriverPatronymic);
			}
		}

		public string CarModel { get; set; }

		public string CarNumber { get; set; }

		[UseForSearch]
		public string DriverAndCar {
			get {
				return String.Format("{0} - {1} ({2})", Driver, CarModel, CarNumber);
			}
		}
		public string ClosinComments { get; set; }
	}
}