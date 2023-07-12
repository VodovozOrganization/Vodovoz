using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Dialogs.Logistic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
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
using QS.DomainModel.NotifyChange;
using QS.Navigation;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.JournalViewers;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Settings.Cash;

namespace Vodovoz.ViewModel
{
	public class RouteListsVM : QSOrmProject.RepresentationModel.RepresentationModelEntityBase<RouteList, RouteListsVMNode>
	{
		private readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private bool _userHasOnlyAccessToWarehouseAndComplaints;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings = MainClass.AppDIContainer.Resolve<IFinancialCategoriesGroupsSettings>();

		public RouteListsFilter Filter {
			get => RepresentationFilter as RouteListsFilter;
			set => RepresentationFilter = value;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			RouteListsVMNode resultAlias = null;
			RouteList routeListAlias = null;
			RouteList routeList2Alias = null;
			DeliveryShift shiftAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Subdivision subdivisionAlias = null;
			GeoGroup geoGroupAlias = null;
			GeoGroupVersion geoGroupVersionAlias = null;

			var query = UoW.Session.QueryOver(() => routeListAlias);

			var firstRouteListGeoGroup = QueryOver.Of(() => routeList2Alias)
            	.JoinAlias(() => routeList2Alias.GeographicGroups, () => geoGroupAlias)
            	.Where(() => routeList2Alias.Id == routeListAlias.Id)
            	.Select(Projections.Property(() => geoGroupAlias.Id))
            	.Take(1);
            
            var closingSubdivision = QueryOver.Of(() => subdivisionAlias)
            	.JoinEntityAlias(() => geoGroupVersionAlias, () => geoGroupVersionAlias.CashSubdivision.Id == subdivisionAlias.Id)
            	.Where(Restrictions.EqProperty(
					Projections.Property(() => geoGroupVersionAlias.GeoGroup.Id),
					Projections.SubQuery(firstRouteListGeoGroup)))
            	.And(() => geoGroupVersionAlias.ActivationDate <= routeListAlias.Date)
            	.And(() => geoGroupVersionAlias.ClosingDate == null || geoGroupVersionAlias.ClosingDate >= routeListAlias.Date)
            	.Select(s => s.Name);


			query.Left.JoinAlias(rl => rl.Driver, () => driverAlias)
				.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Left.JoinAlias(o => o.GeographicGroups, () => geoGroupAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date));

			if(Filter.SelectedStatuses != null) {
				query.WhereRestrictionOn(o => o.Status).IsIn(Filter.SelectedStatuses);
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
				query.Where(() => geoGroupAlias.Id == Filter.RestrictGeographicGroup.Id);
			}

			if(Filter.ShowDriversWithTerminal)
			{
				DriverAttachedTerminalDocumentBase baseAlias = null;
				DriverAttachedTerminalGiveoutDocument giveoutAlias = null;
				var baseQuery = QueryOver.Of(() => baseAlias)
					.Where(doc => doc.Driver.Id == routeListAlias.Driver.Id)
					.And(doc => doc.CreationDate.Date <= routeListAlias.Date)
					.Select(doc => doc.Id).OrderBy(doc => doc.CreationDate).Desc.Take(1);
				var giveoutQuery = QueryOver.Of(() => giveoutAlias).WithSubquery.WhereProperty(giveout => giveout.Id).Eq(baseQuery)
					.Select(doc => doc.Driver.Id);
				query.WithSubquery.WhereProperty(rl => rl.Driver.Id).In(giveoutQuery);
			}

			#region RouteListAddressTypeFilter
			
			//WithDeliveryAddresses(Доставка) означает МЛ без WithChainStoreAddresses(Сетевой магазин) и WithServiceAddresses(Сервисное обслуживание)
			if(Filter.WithDeliveryAddresses && Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) 
			{
				query.Where(() => !driverAlias.VisitingMaster);
			}
			else if(Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && Filter.WithServiceAddresses) 
			{
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) 
			{
				query.Where(() => !driverAlias.VisitingMaster);
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(!Filter.WithDeliveryAddresses && Filter.WithChainStoreAddresses && Filter.WithServiceAddresses) 
			{
				query.Where(Restrictions.Or(
					Restrictions.Where(() => driverAlias.VisitingMaster),
					Restrictions.Where(() => driverAlias.IsChainStoreDriver)
				));
			}
			else if(!Filter.WithDeliveryAddresses && Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) 
			{
				query.Where(() => driverAlias.IsChainStoreDriver);
			}
			else if(!Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && Filter.WithServiceAddresses) 
			{
				query.Where(() => driverAlias.VisitingMaster);
			}
			else if(!Filter.WithDeliveryAddresses && !Filter.WithChainStoreAddresses && !Filter.WithServiceAddresses) 
			{
				SetItemsSource(new List<RouteListsVMNode>());
				return;
			}

			#endregion

			if(Filter.RestrictedCarOwnTypes != null)
			{
				query.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(Filter.RestrictedCarOwnTypes.ToArray());
			}

			if(Filter.RestrictedCarTypesOfUse != null)
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(Filter.RestrictedCarTypesOfUse.ToArray());
			}

			var result = query
				.Left.JoinAlias(o => o.Shift, () => shiftAlias)
				.Left.JoinAlias(o => o.Car, () => carAlias)
				.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => routeListAlias.Status).WithAlias(() => resultAlias.StatusEnum)
				   .Select(() => shiftAlias.Name).WithAlias(() => resultAlias.ShiftName)
				   .Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
				   .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarNumber)
				   .Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverSurname)
				   .Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
				   .Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
				   .Select(() => driverAlias.Comment).WithAlias(() => resultAlias.DriverComment)
				   .Select(() => routeListAlias.LogisticiansComment).WithAlias(() => resultAlias.LogisticiansComment)
				   .Select(() => routeListAlias.ClosingComment).WithAlias(() => resultAlias.ClosinComments)
				   .SelectSubQuery(closingSubdivision).WithAlias(() => resultAlias.ClosingSubdivision)
				   .Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => resultAlias.NotFullyLoaded)
				   .Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
				   .Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
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
			.AddColumn("Комментарий по водителю")
				.AddTextRenderer(node => node.DriverComment)
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
			ConfigureDlg();
		}

		public RouteListsVM()
		{
			CreateRepresentationFilter = () => new RouteListsFilter(UoW);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			NotifyConfiguration.Enable();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(OnRouteListChanged);
			
			_userHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			Filter.SelectedStatuses = new[]{
					RouteListStatus.New,
					RouteListStatus.Confirmed,
					RouteListStatus.InLoading,
					RouteListStatus.EnRoute,
					RouteListStatus.Delivered,
					RouteListStatus.OnClosing,
					RouteListStatus.MileageCheck,
					RouteListStatus.Closed
				};
		}

		private void OnRouteListChanged(EntityChangeEvent[] changeEvents)
		{
			UpdateNodes();
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
		};
		
		private List<RouteListStatus> CanReturnToEnRoute = new List<RouteListStatus>
		{
			RouteListStatus.Delivered
		};

		private List<RouteListStatus> ControlDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.InLoading
		};

		private List<RouteListStatus> ClosingDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> AnalysisViewModelStatuses = new List<RouteListStatus> {
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private List<RouteListStatus> MileageCheckDlgStatuses = new List<RouteListStatus> {
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private bool CanDeleteRouteList(RouteListsVMNode selectedNode)
		{
			bool result = false;
			
			if(selectedNode.StatusEnum == RouteListStatus.New)
			{
				RouteListRepository rep = new RouteListRepository(
					new StockRepository(), 
					new BaseParametersProvider(new ParametersProvider())); 
				if(rep.RouteListContainsGivedFuelLiters(UoW, selectedNode.Id))
				{
					result = false;
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		private List<RouteListStatus> FuelIssuingStatuses = new List<RouteListStatus> {
			RouteListStatus.New,
			RouteListStatus.Confirmed,
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};


		public override IEnumerable<IJournalPopupItem> PopupItems {
			get {
				var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					new OrderRepository(),
					new EmployeeRepository(),
					new BaseParametersProvider(_parametersProvider),
					ServicesConfig.CommonServices.UserService,
					ErrorReporter.Instance);

				var result = new List<IJournalPopupItem>();

				if(_userHasOnlyAccessToWarehouseAndComplaints)
				{
					return result;
				}
				
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

						using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot()) {
							var routeLists = uowLocal.Session.QueryOver<RouteList>()
								.Where(x => x.Id.IsIn(routeListIds))
								.List();
							
							bool needShowMessage = false;
							var warehouseRepository = new WarehouseRepository();
							List<LackStockNode> messageStockList = new List<LackStockNode>();

							foreach (var routeList in routeLists)
							{
								var routeListParametersProvider = new RouteListParametersProvider(_parametersProvider);
								int warehouseId = 0;

								var geoGroup = routeList.GeographicGroups.FirstOrDefault();
								var geoGroupVersion = geoGroup.GetVersionOrNull(routeList.Date);
								if(geoGroupVersion != null)
								{
									warehouseId = geoGroupVersion.Warehouse.Id;
								}

								if(warehouseId > 0)
								{
									var onlineOrders = routeList.Addresses
										.SelectMany(adressItem => adressItem.Order.OrderItems)
										.Where(orderItem => orderItem.Nomenclature.OnlineStore != null);

									var warehouseStocks = warehouseRepository
										.GetWarehouseNomenclatureStock(
											UoW,
											OperationType.WarehouseBulkGoodsAccountingOperation,
											warehouseId,
											onlineOrders.Select(o => o.Nomenclature.Id).Distinct());
									
									var lackWarehouseStocks = onlineOrders
										.Join(warehouseStocks,
										o => o.Nomenclature.Id, 
										w => w.NomenclatureId,
										(o, w) => new LackStockNode() {NomenclatureId = o.Nomenclature.Id, OrderId = o.Order.Id,
											NomenclatureName = o.Nomenclature.Name, Count = o.Count, Stock= w.Stock, Measure = o.Nomenclature.Unit.Name })
										.Where(w=> w.Stock < w.Count);

									messageStockList.AddRange(lackWarehouseStocks);

									var notExistInWarehouseNomenclatures = onlineOrders
										.Where(o => !warehouseStocks.Any(w => w.NomenclatureId == o.Nomenclature.Id))
										.Select(o => new LackStockNode()
										{
											OrderId = o.Order.Id,
											NomenclatureName = o.Nomenclature.Name,
											Count = o.Count,
											Measure = o.Nomenclature.Unit.Name
										});

									messageStockList.AddRange(notExistInWarehouseNomenclatures);
								}
							}

							StringBuilder stockMessage = new StringBuilder();
							
							if (messageStockList.Count > 0)
							{
								needShowMessage = true;
								stockMessage.Append($"В наличии нет следующих товаров:");

								messageStockList.ForEach(messageItem =>
								{
									stockMessage.Append(Environment.NewLine);
									stockMessage.Append($"Заказ {messageItem.OrderId}: {messageItem.NomenclatureName} - {messageItem.Count} {messageItem.Measure}");
								});
								
								stockMessage.Append($"{Environment.NewLine}Всё равно отправить МЛ на погрузку?");
							}

							if (!needShowMessage || (needShowMessage && ServicesConfig.CommonServices.InteractiveService.Question(stockMessage.ToString())))
							{
								routeLists.Where((arg) => arg.Status == RouteListStatus.Confirmed).ToList().ForEach(
									(routeList) =>
									{
										if (TDIMain.MainNotebook.FindTab(
											DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
										{
											MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
											isSlaveTabActive = true;
											return;
										}

										foreach (var address in routeList.Addresses)
										{
											if (address.Order.OrderStatus < Domain.Orders.OrderStatus.OnLoading)
												address.Order.ChangeStatusAndCreateTasks(
													Domain.Orders.OrderStatus.OnLoading, callTaskWorker);
										}

										routeList.ChangeStatusAndCreateTask(RouteListStatus.InLoading, callTaskWorker);
										uowLocal.Save(routeList);
									});

								if (isSlaveTabActive)
									return;

								uowLocal.Commit();
							}
						}
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
				
				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Вернуть в путь",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						bool isSlaveTabActive = false;
						var routeListIds = selectedNodes.Select(x => x.Id).ToArray();

						using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot()) {

							var routeLists = uowLocal.Session.QueryOver<RouteList>()
								.Where(x => x.Id.IsIn(routeListIds))
								.List();

							routeLists.Where((arg) => arg.Status == RouteListStatus.Delivered).ToList().ForEach((routeList) => {
								if(TDIMain.MainNotebook.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null) {
									MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
									isSlaveTabActive = true;
									return;
								}
								routeList.ChangeStatusAndCreateTask(RouteListStatus.EnRoute, callTaskWorker);
								uowLocal.Save(routeList);
							});

							if(isSlaveTabActive)
								return;

							uowLocal.Commit();
						}
					},
					(selectedItems) => selectedItems.Any(x => CanReturnToEnRoute.Contains((x as RouteListsVMNode).StatusEnum))
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
						{
							MainClass.MainWin.NavigationManager.OpenViewModel<RouteListAnalysisViewModel, IEntityUoWBuilder>(
								null, EntityUoWBuilder.ForOpen(selectedNode.Id), OpenPageOptions.IgnoreHash);
						}
					},
					(selectedItems) => selectedItems.Any(x => AnalysisViewModelStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Открыть диалог проверки километража",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						if(selectedNode != null
							&& MileageCheckDlgStatuses.Contains(selectedNode.StatusEnum)
							&& selectedNode.CarTypeOfUse != CarTypeOfUse.Truck)
						{
							MainClass.MainWin.NavigationManager.OpenViewModel<RouteListMileageCheckViewModel, IEntityUoWBuilder>(
								null, EntityUoWBuilder.ForOpen(selectedNode.Id), OpenPageOptions.AsSlave);
						}
					},
					(selectedItems) => selectedItems.Any(
						x => MileageCheckDlgStatuses.Contains((x as RouteListsVMNode).StatusEnum)
						&& (x as RouteListsVMNode).UsesCompanyCar && ((RouteListsVMNode) x).CarTypeOfUse != CarTypeOfUse.Truck
					)
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible("Удалить МЛ",
					(selectedItems) => {
						var selectedNodes = selectedItems.Cast<RouteListsVMNode>();
						var selectedNode = selectedNodes.FirstOrDefault();
						var objectType = typeof(RouteList);

						var routeList = UoW.Session.QueryOver<RouteList>()
								.Where(x => x.Id == selectedNode.Id)
								.SingleOrDefault<RouteList>();

						var orders = new List<Vodovoz.Domain.Orders.Order>();

						foreach (var address in routeList.Addresses)
						{
							UoW.Session.Refresh(address.Order);
							if (address.Order.OrderStatus == Domain.Orders.OrderStatus.OnLoading 
							 || address.Order.OrderStatus == Domain.Orders.OrderStatus.InTravelList)
                            {
								orders.Add(address.Order);
							}
						}

						if (selectedNode != null && OrmMain.DeleteObject(objectType, selectedNode.Id))
                        {
							foreach (var order in orders)
                            {
								order.ChangeStatusAndCreateTasks(Domain.Orders.OrderStatus.Accepted, callTaskWorker);
								UoW.Save(order);
							}

							UoW.Commit();
							UpdateNodes();
						}
					},
					(selectedItems) => selectedItems.Any(x => CanDeleteRouteList(x as RouteListsVMNode))
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
														new SubdivisionRepository(_parametersProvider), 
														new EmployeeRepository(), 
														new FuelRepository(),
														NavigationManagerProvider.NavigationManager,
														new TrackRepository(),
														new EmployeeJournalFactory(),
														_financialCategoriesGroupsSettings,
														new CarJournalFactory(MainClass.MainWin.NavigationManager)
									)
								);
						}
					},
					(selectedItems) => selectedItems.Any(x => FuelIssuingStatuses.Contains((x as RouteListsVMNode).StatusEnum))
				));

				result.Add(JournalPopupItemFactory.CreateNewAlwaysVisible(
					"Перенести терминал на вторую ходку",
					(selectedItems) =>
					{
						if(selectedItems.FirstOrDefault() is RouteListsVMNode selectedNode)
						{
							var routeList = UoW.GetById<RouteList>(selectedNode.Id);
							routeList?.CreateSelfDriverTerminalTransferDocument();
						}
					},
					(selectedItems) =>
					{
						var userPermission = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(
							typeof(SelfDriverTerminalTransferDocument), ServicesConfig.UserService.CurrentUserId);

						return userPermission.CanCreate;
					}
				));

				return result;
			}
		}
		
		private class LackStockNode
		{
			public int NomenclatureId;
			public int OrderId;
			public string NomenclatureName;
			public decimal Count;
			public decimal Stock;
			public string Measure;
		}
		
		public new void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
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
		public string DriverComment { get; set; }

		public string Driver => PersonHelper.PersonFullName(DriverSurname, DriverName, DriverPatronymic);

		public string CarModelName { get; set; }

		public string CarNumber { get; set; }

		[UseForSearch]
		public string DriverAndCar => string.Format("{0} - {1} ({2})", Driver, CarModelName, CarNumber);
		public string LogisticiansComment { get; set; }
		public string ClosinComments { get; set; }
		public string ClosingSubdivision { get; set; }
		public bool NotFullyLoaded { get; set; }
		public CarTypeOfUse CarTypeOfUse { get; set; }
		
		public CarOwnType CarOwnType { get; set; }

		public bool UsesCompanyCar => CarOwnType == CarOwnType.Company;
	}
}
