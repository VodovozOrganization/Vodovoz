using System;
using System.Collections.Generic;
using System.Linq;
using Dialogs.Logistic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
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
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			RouteListsVMNode resultAlias = null;
			RouteList routeListAlias = null;
			DeliveryShift shiftAlias = null;

			Car carAlias = null;
			Employee driverAlias = null;

			var query = UoW.Session.QueryOver<RouteList> (() => routeListAlias);

			if(Filter.RestrictStatus != null)
			{
				query.Where (o => o.Status == Filter.RestrictStatus);
			}

			if(Filter.RestrictShift != null)
			{
				query.Where (o => o.Shift == Filter.RestrictShift);
			}

			if(Filter.RestrictStartDate != null)
			{
				query.Where (o => o.Date >= Filter.RestrictStartDate);
			}

			if(Filter.RestrictEndDate != null)
			{
				query.Where (o => o.Date <= Filter.RestrictEndDate.Value.AddDays (1).AddTicks (-1));
			}

			var result = query
				.JoinAlias (o => o.Shift, () => shiftAlias)
				.JoinAlias (o => o.Car, () => carAlias)
				.JoinAlias (o => o.Driver, () => driverAlias)
				.SelectList (list => list
					.Select (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => routeListAlias.Date).WithAlias (() => resultAlias.Date)
					.Select (() => routeListAlias.Status).WithAlias (() => resultAlias.StatusEnum)
					.Select (() => shiftAlias.Name).WithAlias (() => resultAlias.ShiftName)
					.Select (() => carAlias.Model).WithAlias (() => resultAlias.CarModel)
					.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.CarNumber)
					.Select (() => driverAlias.LastName).WithAlias (() => resultAlias.DriverSurname)
					.Select (() => driverAlias.Name).WithAlias (() => resultAlias.DriverName)
					.Select (() => driverAlias.Patronymic).WithAlias (() => resultAlias.DriverPatronymic)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing (Transformers.AliasToBean<RouteListsVMNode> ())
				.List<RouteListsVMNode> ();

			SetItemsSource (result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <RouteListsVMNode>.Create ()
			.AddColumn ("Номер").SetDataProperty (node => node.Id.ToString())
			.AddColumn ("Дата").SetDataProperty (node => node.Date.ToString("d"))
			.AddColumn ("Смена").SetDataProperty (node => node.ShiftName)
			.AddColumn ("Статус").SetDataProperty (node => node.StatusEnum.GetEnumTitle ())
			.AddColumn ("Водитель и машина").SetDataProperty (node => node.DriverAndCar)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (RouteList updatedSubject)
		{
			return true;
		}

		#endregion

		public RouteListsVM (RouteListsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public RouteListsVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new RouteListsFilter(UoW);
		}

		public RouteListsVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
			if(Filter == null)
				Filter = new RouteListsFilter(UoW);
		}

		public override bool PopupMenuExist {
			get	{return true;}
		}

		private RepresentationSelectResult[] lastMenuSelected;

		private List<RouteListStatus> KeepingDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.EnRoute,
				RouteListStatus.OnClosing,
				RouteListStatus.NotDelivered,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		private List<RouteListStatus> ClosingDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.OnClosing,
				RouteListStatus.NotDelivered,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		private List<RouteListStatus> MileageCheckDlgStatuses = new List<RouteListStatus>()
			{
				RouteListStatus.OnClosing,
				RouteListStatus.NotDelivered,
				RouteListStatus.MileageCheck,
				RouteListStatus.Closed
			};

		public override Gtk.Menu GetPopupMenu (RepresentationSelectResult[] selected)
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

			return popupMenu;
		}

		void MenuItemRouteListOpenTrack_Activated (object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();
			foreach (var id in routeListIds)
			{
				TrackOnMapWnd track = new TrackOnMapWnd(id);
				track.Show();
			}
		}

		void MenuItemRouteListMileageCheckDlg_Activated (object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach (var rl in routeLists.Where(x => MileageCheckDlgStatuses.Contains(x.Status)))
			{
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListMileageCheckDlg (rl.Id)
				);
			}
		}

		void MenuItemRouteListClosingDlg_Activated (object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();

			foreach (var rl in routeLists.Where(x => ClosingDlgStatuses.Contains(x.Status)))
			{
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListClosingDlg (rl.Id)
				);
			}
		}

		void MenuItemRouteListKeepingDlg_Activated (object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			var routeLists = UoW.Session.QueryOver<RouteList>()
				.Where(x => x.Id.IsIn(routeListIds))
				.List();
			
			foreach (var rl in routeLists.Where(x => KeepingDlgStatuses.Contains(x.Status)))
			{
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(rl.Id),
					() => new RouteListKeepingDlg (rl.Id)
				);
			}
		}

		void MenuItemRouteListCreateDlg_Activated (object sender, EventArgs e)
		{
			var routeListIds = lastMenuSelected.Select(x => x.EntityId).ToArray();

			foreach (var routeId in routeListIds)
			{
				MainClass.MainWin.TdiMain.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(routeId),
					() => new RouteListCreateDlg (routeId)
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

		public string Driver { get{ return StringWorks.PersonFullName(DriverSurname, DriverName, DriverPatronymic);
		} }

		public string CarModel { get; set; }

		public string CarNumber { get; set; }

		[UseForSearch]
		public string DriverAndCar { get{ return String.Format("{0} - {1} ({2})", Driver, CarModel, CarNumber);
			} }
	}
}