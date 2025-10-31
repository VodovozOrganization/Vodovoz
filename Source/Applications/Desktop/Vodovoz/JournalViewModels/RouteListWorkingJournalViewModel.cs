using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Profitability;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Fuel;
using Vodovoz.Settings.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.JournalViewModels
{
	public class RouteListWorkingJournalViewModel : FilterableSingleEntityJournalViewModelBase<RouteList, TdiTabBase, RouteListJournalNode, RouteListJournalFilterViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IAccountableDebtsRepository _accountableDebtsRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IRouteListService _routeListService;
		private readonly decimal _routeListProfitabilityIndicator;

		public RouteListWorkingJournalViewModel(
			RouteListJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			IRouteListRepository routeListRepository,
			IFuelRepository fuelRepository,
			ICallTaskRepository callTaskRepository,
			ICallTaskWorker callTaskWorker,
			IExpenseSettings expenseSettings,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			ISubdivisionRepository subdivisionRepository,
			IAccountableDebtsRepository accountableDebtsRepository,
			IGtkTabsOpener gtkTabsOpener,
			IRouteListProfitabilitySettings routeListProfitabilitySettings,
			IOrganizationRepository organizationRepository,
			INavigationManager navigationManager,
			IRouteListService routeListService,
			Action<RouteListJournalFilterViewModel> filterParams = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			TabName = "Работа кассы с МЛ";
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_accountableDebtsRepository = accountableDebtsRepository ?? throw new ArgumentNullException(nameof(accountableDebtsRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_routeListProfitabilityIndicator = FilterViewModel.RouteListProfitabilityIndicator =
				(routeListProfitabilitySettings ?? throw new ArgumentNullException(nameof(routeListProfitabilitySettings)))
				.GetRouteListProfitabilityIndicatorInPercents;
			UseSlider = false;

			if(filterParams != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterParams);
			} 
			
			UpdateOnChanges(typeof(RouteList), typeof(RouteListProfitability), typeof(RouteListDebt));
			InitPopupActions();
		}

		private void OnRouteListChanged(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		protected override Func<IUnitOfWork, IQueryOver<RouteList>> ItemsSourceQueryFunction => (uow) =>
		{
			RouteListJournalNode routeListJournalNodeAlias = null;
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
			RouteListProfitability routeListProfitabilityAlias = null;
			RouteListDebt routeListDebtAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Left.JoinAlias(o => o.Shift, () => shiftAlias)
				.Left.JoinAlias(o => o.Car, () => carAlias)
				.Left.JoinAlias(o => o.GeographicGroups, () => geoGroupAlias)
				.Left.JoinAlias(o => o.Driver, () => driverAlias)
				.Left.JoinAlias(rl => rl.RouteListProfitability, () => routeListProfitabilityAlias)
				.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date));

			if(FilterViewModel.SelectedStatuses != null)
			{
				query.WhereRestrictionOn(o => o.Status).IsIn(FilterViewModel.SelectedStatuses);
			}

			if(FilterViewModel.DeliveryShift != null)
			{
				query.Where(o => o.Shift == FilterViewModel.DeliveryShift);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null)
			{
				query.Where(o => o.Date >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null)
			{
				query.Where(o => o.Date <= endDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.GeographicGroup != null)
			{
				query.Where(() => geoGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
			}

			#region RouteListAddressTypeFilter

			{
				var delivery = FilterViewModel.WithDeliveryAddresses;
				var chainStore = FilterViewModel.WithChainStoreAddresses;
				var service = FilterViewModel.WithServiceAddresses;

				if(delivery && chainStore && !service)
				{
					query.Where(() => !driverAlias.VisitingMaster);
				}
				else if(delivery && !chainStore && service)
				{
					query.Where(() => !driverAlias.IsChainStoreDriver);
				}
				else if(delivery && !chainStore && !service)
				{
					query.Where(() => !driverAlias.VisitingMaster);
					query.Where(() => !driverAlias.IsChainStoreDriver);
				}
				else if(!delivery && chainStore && service)
				{
					query.Where(Restrictions.Or(
						Restrictions.Where(() => driverAlias.VisitingMaster),
						Restrictions.Where(() => driverAlias.IsChainStoreDriver)
					));
				}
				else if(!delivery && chainStore && !service)
				{
					query.Where(() => driverAlias.IsChainStoreDriver);
				}
				else if(!delivery && !chainStore && service)
				{
					query.Where(() => driverAlias.VisitingMaster);
				}
				else if(!delivery && !chainStore && !service)
				{
					query.Where(() => routeListAlias.Id == null);
				}
			}

			#endregion

			if(FilterViewModel.ShowDriversWithTerminal)
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

			if(FilterViewModel.RestrictedCarOwnTypes != null)
			{
				query.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(FilterViewModel.RestrictedCarOwnTypes.ToArray());
			}

			if(FilterViewModel.RestrictedCarTypesOfUse != null)
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(FilterViewModel.RestrictedCarTypesOfUse.ToArray());
			}

			var driverProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic)
			);

			query.Where(GetSearchCriterion(
				() => routeListAlias.Id,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic,
				() => driverProjection,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber
			));
			
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

			var routeListDebtSubquery = QueryOver.Of(() => routeListDebtAlias)
				.Where(() => routeListAlias.Id == routeListDebtAlias.RouteList.Id)
				.Select(r => r.Debt)
				.Take(1);

			var result = query
				.SelectList(list => list
					.SelectGroup(() => routeListAlias.Id).WithAlias(() => routeListJournalNodeAlias.Id)
					.Select(() => routeListAlias.Date).WithAlias(() => routeListJournalNodeAlias.Date)
					.Select(() => routeListAlias.Status).WithAlias(() => routeListJournalNodeAlias.StatusEnum)
					.Select(() => shiftAlias.Name).WithAlias(() => routeListJournalNodeAlias.ShiftName)
					.Select(() => carModelAlias.Name).WithAlias(() => routeListJournalNodeAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => routeListJournalNodeAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => routeListJournalNodeAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => routeListJournalNodeAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => routeListJournalNodeAlias.DriverPatronymic)
					.Select(() => driverAlias.Comment).WithAlias(() => routeListJournalNodeAlias.DriverComment)
					.SelectSubQuery(routeListDebtSubquery).WithAlias(() => routeListJournalNodeAlias.RouteListDebt)
					.Select(() => routeListAlias.LogisticiansComment).WithAlias(() => routeListJournalNodeAlias.LogisticiansComment)
					.Select(() => routeListAlias.ClosingComment).WithAlias(() => routeListJournalNodeAlias.ClosinComments)
					.SelectSubQuery(closingSubdivision).WithAlias(() => routeListJournalNodeAlias.ClosingSubdivision)
					.Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => routeListJournalNodeAlias.NotFullyLoaded)
					.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => routeListJournalNodeAlias.CarTypeOfUse)
					.Select(() => carVersionAlias.CarOwnType).WithAlias(() => routeListJournalNodeAlias.CarOwnType)
					.Select(() => routeListProfitabilityAlias.GrossMarginPercents)
						.WithAlias(() => routeListJournalNodeAlias.GrossMarginPercents)
					.Select(Projections.Constant(_routeListProfitabilityIndicator))
						.WithAlias(() => routeListJournalNodeAlias.RouteListProfitabilityIndicator)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListJournalNode>());

			return result;
		};

		protected override Func<TdiTabBase> CreateDialogFunction => () => throw new NotSupportedException();

		#region restrictions

		private readonly RouteListStatus[] _closingDlgStatuses = new[]
		{
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
		};

		private readonly RouteListStatus[] _createCarLoadDocument = new[]
		{
			RouteListStatus.InLoading
		};

		private readonly RouteListStatus[] _createCarUnloadDocument = new[]
		{
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
		};

		private readonly RouteListStatus[] _fuelIssuingStatuses = new[]
		{
			RouteListStatus.New,
			RouteListStatus.Confirmed,
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};

		private readonly RouteListStatus[] _canReturnToOnClosing = new[]
		{
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private readonly RouteListStatus[] _canGiveChangeForRL = new[]
{
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute
		};

		private readonly RouteListStatus[] _canReturnChangeFromRL = new[]
{
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};

		#endregion

		protected override Func<RouteListJournalNode, TdiTabBase> OpenDialogFunction => (node) =>
		{
			if(!(NavigationManager is ITdiCompatibilityNavigation navigationManager))
			{
				return null;
			}
			
			switch(node.StatusEnum)
			{
				case RouteListStatus.New:
				case RouteListStatus.Confirmed:
					navigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return null;
				case RouteListStatus.InLoading:
					if(_routeListRepository.IsTerminalRequired(UoW, node.Id))
					{
						navigationManager.OpenTdiTab<CarLoadDocumentDlg, int, int?>(this, node.Id, null);
						return null;
					}
					
					navigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return null;
				case RouteListStatus.EnRoute:
					navigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id));
					return null;
				case RouteListStatus.Delivered:
				case RouteListStatus.OnClosing:
				case RouteListStatus.MileageCheck:
				case RouteListStatus.Closed:
					navigationManager.OpenTdiTab<RouteListClosingDlg, int>(this, node.Id);
					return null;
				default:
					throw new InvalidOperationException("Неизвестный статус МЛ");
			}
		};

		protected void InitPopupActions()
		{
			PopupActionsList.Add(new JournalAction(
				"Закрытие МЛ",
				(selectedItems) => selectedItems.Any(x => _closingDlgStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _closingDlgStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode
					&& _closingDlgStatuses.Contains(selectedNode.StatusEnum))
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListClosingDlg(selectedNode.Id, true)
						);
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Создание талона погрузки",
				(selectedItems) => selectedItems.Any(x => _createCarLoadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _createCarLoadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						TabParent.OpenTab(() => new CarLoadDocumentDlg(selectedNode.Id, null));
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Создание талона разгрузки",
				(selectedItems) => selectedItems.Any(x => _createCarUnloadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _createCarUnloadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						TabParent.OpenTab(() => new CarUnloadDocumentDlg(selectedNode.Id, null));
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Выдать топливо",
				(selectedItems) => selectedItems.Any(x => _fuelIssuingStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _fuelIssuingStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						var RouteList = UoW.GetById<RouteList>(selectedNode.Id);
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new FuelDocumentViewModel(
								RouteList,
								UnitOfWorkFactory,
								commonServices,
								_subdivisionRepository,
								_lifetimeScope.Resolve<IEmployeeRepository>(),
								_fuelRepository,
								NavigationManager,
								_lifetimeScope.Resolve<ITrackRepository>(),
								_lifetimeScope.Resolve<IEmployeeJournalFactory>(),
								_financialCategoriesGroupsSettings,
								_organizationRepository,
								_lifetimeScope.Resolve<IFuelApiService>(),
								_lifetimeScope.Resolve<IFuelControlSettings>(),
								_lifetimeScope.Resolve<IGuiDispatcher>(),
								_lifetimeScope.Resolve<IUserSettingsService>(),
								_lifetimeScope.Resolve<IYesNoCancelQuestionInteractive>(),
								_lifetimeScope
							)
						);
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Вернуть в статус Сдается",
				(selectedItems) => selectedItems.Any(x => _canReturnToOnClosing.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _canReturnToOnClosing.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					bool isSlaveTabActive = false;
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
						{
							var routeList = uowLocal.Session.QueryOver<RouteList>()
								.Where(x => x.Id == selectedNode.Id)
								.List().FirstOrDefault();

							if(_canReturnToOnClosing.Contains(routeList.Status))
							{
								if(TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
								{
									MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
									isSlaveTabActive = true;
									return;
								}
								_routeListService.ChangeStatusAndCreateTask(uowLocal, routeList, RouteListStatus.OnClosing);
								uowLocal.Save(routeList);
								if(isSlaveTabActive)
								{
									return;
								}
							}
							uowLocal.Commit();
						}
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Выдать сдачу",
				(selectedItems) => selectedItems.Any(x => _canGiveChangeForRL.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _canGiveChangeForRL.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(!(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode))
					{
						return;
					}

					using(var localUow = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var routeList = localUow.GetById<RouteList>(selectedNode.Id);
						var driverId = routeList.Driver.Id;

						if(_accountableDebtsRepository.GetUnclosedAdvances(localUow,
							   routeList.Driver,
							   _financialCategoriesGroupsSettings.ChangeFinancialExpenseCategoryId,
							   null).Count() > 0)
						{
							commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error,
								"Закройте сначала прошлые авансовые со статусом \"Сдача клиенту\"", "Нельзя выдать сдачу");
							return;
						}

						var changesToOrders = routeList.GetCashChangesForOrders();

						if(!changesToOrders.Any())
						{
							commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Info,
								"Для данного МЛ нет наличных заказов требующих сдачи");
							return;
						}

						var page = NavigationManager.OpenViewModel<ExpenseViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());

						page.ViewModel.ConfigureForRouteListChangeGiveout(
							driverId,
							routeList.Id);
					}
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Возврат сдачи",
				(selectedItems) => selectedItems.Any(x => _canReturnChangeFromRL.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) => selectedItems.Any(x => _canReturnChangeFromRL.Contains((x as RouteListJournalNode).StatusEnum)),
				(selectedItems) =>
				{
					if(!(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode))
					{
						return;
					}

					var page = NavigationManager.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());

					page.ViewModel.ConFigureForReturnChange(selectedNode.Id);
				}
			));

			PopupActionsList.Add(new JournalAction(
				"Перенести терминал на вторую ходку",
				(selectedItems) =>
				{
					var userPermission = commonServices.PermissionService.ValidateUserPermission(
						typeof(SelfDriverTerminalTransferDocument), commonServices.UserService.CurrentUserId);

					return userPermission.CanCreate;
				},
				(selectedItems) => true,
				(selectedItems) =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						var routeList = UoW.GetById<RouteList>(selectedNode.Id);
						routeList?.CreateSelfDriverTerminalTransferDocument();
					}
				}
			));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
		}
		
		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<RouteListJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<RouteListJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));
					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}
	}
}
