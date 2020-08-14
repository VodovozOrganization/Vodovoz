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
using QS.Project.Services;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
using QS.Utilities.Text;
using QSOrmProject;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Logistic;
using QS.Project.Domain;

namespace Vodovoz.ViewModel
{
	public class RouteListsVM : QSOrmProject.RepresentationModel.RepresentationModelEntityBase<RouteList, RouteListsVMNode>
	{
		public RouteListsFilter Filter {
			get => RepresentationFilter as RouteListsFilter;
			set => RepresentationFilter = value as QSOrmProject.RepresentationModel.IRepresentationFilter;
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

			query.Left.JoinAlias(o => o.Driver, () => driverAlias);


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

			#region RouteListAddressTypeFilter
			
			//WithDeliveryAddresses(Доставка) означает МЛ без WithChainStoreAddresses(Сетевой магазин) и WithServiceAddresses(Сервисное обслуживание)
			if(      Filter.WithDeliveryAddresses &&  Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) {
				query.Where(() => !driverAlias.VisitingMaster);
			}
			else if( Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses &&  Filter.WithServiceAddresses) {
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if( Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) {
				query.Where(() => !driverAlias.VisitingMaster);
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(!Filter.WithDeliveryAddresses &&  Filter.WithChainStoreAddresses &&  Filter.WithServiceAddresses) {
				query.Where(Restrictions.Or(
					Restrictions.Where(() => driverAlias.VisitingMaster),
					Restrictions.Where(() => driverAlias.IsChainStoreDriver)
				));
			}
			else if(!Filter.WithDeliveryAddresses &&  Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) {
				query.Where(() => driverAlias.IsChainStoreDriver);
			}
			else if(!Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses &&  Filter.WithServiceAddresses) {
				query.Where(() => driverAlias.VisitingMaster);
			}
			else if(!Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) {
				SetItemsSource(new List<RouteListsVMNode>());
				return;
			}

			#endregion
			
			switch(Filter.RestrictTransport) {
				case RLFilterTransport.Mercenaries:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar && !carAlias.IsRaskat); break;
				case RLFilterTransport.Raskat:
					query.Where(() => carAlias.IsRaskat); break;
				case RLFilterTransport.Largus:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyLargus); break;
				case RLFilterTransport.GAZelle:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyGAZelle); break;
				case RLFilterTransport.Waggon:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyTruck); break;
				case RLFilterTransport.Others:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar); break;
				default: break;
			}
			
			var result = query
				.Left.JoinAlias(o => o.Shift, () => shiftAlias)
				.Left.JoinAlias(o => o.Car, () => carAlias)
				.Left.JoinAlias(o => o.ClosingSubdivision, () => subdivisionAlias)
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
				   .Select(() => routeListAlias.LogisticiansComment).WithAlias(() => resultAlias.LogisticiansComment)
				   .Select(() => routeListAlias.ClosingComment).WithAlias(() => resultAlias.ClosinComments)
				   .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.ClosingSubdivision)
				   .Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => resultAlias.NotFullyLoaded)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListsVMNode>())
				.List<RouteListsVMNode>();

			SetItemsSource(result);
		}
		
		public override IColumnsConfig ColumnsConfig { get; } = FluentColumnsConfig<RouteListsVMNode>.Create()
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
			.AddColumn("Комментарий ЛО")
				.AddTextRenderer(node => node.LogisticiansComment)
				.WrapWidth(300)
				.WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Комментарий по закрытию")
				.AddTextRenderer(node => node.ClosinComments)
				.WrapWidth(300)
				.WrapMode(Pango.WrapMode.WordChar)
			.RowCells()
				.AddSetter<CellRendererText>((c, n) => c.Foreground = n.NotFullyLoaded ? "Orange" : "Black")
			.Finish();

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(RouteList updatedSubject) => true;

		#endregion

		public RouteListsVM(RouteListsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public RouteListsVM()
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

		private List<RouteListStatus> KeepingDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.EnRoute,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> ControlDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.InLoading
		};

		private List<RouteListStatus> TakingMoneyStatuses = new List<RouteListStatus> {
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};

		private List<RouteListStatus> ClosingDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> AnalysisViewModelStatuses = new List<RouteListStatus> {
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> MileageCheckDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> CanDeletedStatuses = new List<RouteListStatus> {
			RouteListStatus.New,
			RouteListStatus.Confirmed,
			RouteListStatus.InLoading
		};

		private List<RouteListStatus> FuelIssuingStatuses = new List<RouteListStatus> {
			RouteListStatus.New,
			RouteListStatus.Confirmed,
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};


		public override IEnumerable<IJournalPopupItem> PopupItems {
			get {
				var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					OrderSingletonRepository.GetInstance(),
					EmployeeSingletonRepository.GetInstance(),
					new BaseParametersProvider(),
					ServicesConfig.CommonServices.UserService,
					SingletonErrorReporter.Instance);

				var result = new List<IJournalPopupItem>();
				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Открыть трек",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							TrackOnMapWnd track = new TrackOnMapWnd(selectedNode.Id);
							track.Show();
						}
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Открыть диалог создания",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null)
							MainClass.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListCreateDlg(selectedNode.Id)
							);
					}
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Отгрузка со склада",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && ControlDlgStatuses.Contains(selectedNode.StatusEnum))
							MainClass.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListControlDlg(selectedNode.Id)
							);
					},
					(selectedItems) => selectedItems.Any(x =>
						ControlDlgStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Отправить МЛ на погрузку",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						bool isSlaveTabActive = false;
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
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
									address.Order.ChangeStatus(Domain.Orders.OrderStatus.OnLoading, callTaskWorker);
							}

							routeList.ChangeStatus(RouteListStatus.InLoading, callTaskWorker);
							UoW.Save(routeList);
						});

						if(isSlaveTabActive)
							return;

						foreach(var rlNode in selectedNodes) {
							var node = (rlNode as RouteListsVMNode);
							if(node != null)
								node.StatusEnum = RouteListStatus.InLoading;
						}
						UoW.Commit();
					},
					(selectedItems) => selectedItems.Any((x) => (x as RouteListsVMNode).StatusEnum == RouteListStatus.Confirmed)
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Открыть диалог ведения",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && KeepingDlgStatuses.Contains(selectedNode.StatusEnum))
							MainClass.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListKeepingDlg(selectedNode.Id)
							);
					},
					(selectedItems) => selectedItems.Any(x => KeepingDlgStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Открыть диалог закрытия",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && ClosingDlgStatuses.Contains(selectedNode.StatusEnum))
							MainClass.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListClosingDlg(selectedNode.Id)
							);
					},
					(selectedItems) => selectedItems.Any(x => ClosingDlgStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Открыть диалог разбора",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && AnalysisViewModelStatuses.Contains(selectedNode.StatusEnum))
							MainClass.MainWin.TdiMain.AddTab(
								new RouteListAnalysisViewModel(
									EntityUoWBuilder.ForOpen(selectedNode.Id),
									UnitOfWorkFactory.GetDefaultFactory,
									ServicesConfig.CommonServices
								)
							);
					},
					(selectedItems) => selectedItems.Any(x => AnalysisViewModelStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Открыть диалог проверки километража",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null && MileageCheckDlgStatuses.Contains(selectedNode.StatusEnum))
							MainClass.MainWin.TdiMain.OpenTab(
								DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
								() => new RouteListMileageCheckDlg(selectedNode.Id)
							);
					},
					(selectedItems) => selectedItems.Any(x => MileageCheckDlgStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Удалить МЛ",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						var objectType = typeof(RouteList);
						if(selectedNode != null && OrmMain.DeleteObject(objectType, selectedNode.Id))
							this.UpdateNodes();
					},
					(selectedItems) => selectedItems.Any(x => CanDeletedStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Выдать топливо",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null) {
							var routeListId = selectedNode.Id;
							var RouteList = UoW.GetById<RouteList>(routeListId);
							MainClass.MainWin.TdiMain.OpenTab(
									DialogHelper.GenerateDialogHashName<RouteList>(routeListId),
									() => new FuelDocumentViewModel(
														RouteList, 
														ServicesConfig.CommonServices, 
														new SubdivisionRepository(), 
														EmployeeSingletonRepository.GetInstance(), 
														new FuelRepository(),
														NavigationManagerProvider.NavigationManager
									)
								);
						}
					},
					(selectedItems) => selectedItems.Any(x => FuelIssuingStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				return result;
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
		public string LogisticiansComment { get; set; }
		public string ClosinComments { get; set; }
		public string ClosingSubdivision { get; set; }
		public bool NotFullyLoaded { get; set; }
	}
}