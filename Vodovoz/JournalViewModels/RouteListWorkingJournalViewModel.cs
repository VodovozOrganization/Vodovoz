﻿using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using NHibernate.Linq;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class RouteListWorkingJournalViewModel : FilterableSingleEntityJournalViewModelBase<RouteList, TdiTabBase, RouteListJournalNode, RouteListJournalFilterViewModel>
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly BaseParametersProvider _baseParametersProvider;
		private readonly IExpenseParametersProvider _expenseParametersProvider;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IAccountableDebtsRepository _accountableDebtsRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public RouteListWorkingJournalViewModel(
			RouteListJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IRouteListRepository routeListRepository,
			IFuelRepository fuelRepository,
			ICallTaskRepository callTaskRepository,
			BaseParametersProvider baseParametersProvider,
			IExpenseParametersProvider expenseParametersProvider,
			ISubdivisionRepository subdivisionRepository,
			IAccountableDebtsRepository accountableDebtsRepository,
			IGtkTabsOpener gtkTabsOpener)
		: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Работа кассы с МЛ";

			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_callTaskRepository = callTaskRepository ?? throw new ArgumentNullException(nameof(callTaskRepository));
			_baseParametersProvider = baseParametersProvider ?? throw new ArgumentNullException(nameof(baseParametersProvider));
			_expenseParametersProvider = expenseParametersProvider ?? throw new ArgumentNullException(nameof(expenseParametersProvider));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_accountableDebtsRepository = accountableDebtsRepository ?? throw new ArgumentNullException(nameof(accountableDebtsRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			UseSlider = false;

			NotifyConfiguration.Enable();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(OnRouteListChanged);

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
			DeliveryShift shiftAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Subdivision subdivisionAlias = null;
			GeographicGroup geographicalGroupAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Left.JoinAlias(o => o.Shift, () => shiftAlias)
				.Left.JoinAlias(o => o.Car, () => carVersionAlias)
				.Left.JoinAlias(o => o.ClosingSubdivision, () => subdivisionAlias)
				.Left.JoinAlias(o => o.Driver, () => driverAlias)
				.Left.JoinAlias(() => carVersionAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias);

			if(FilterViewModel.SelectedStatuses != null)
			{
				query.WhereRestrictionOn(o => o.Status).IsIn(FilterViewModel.SelectedStatuses);
			}

			if(FilterViewModel.DeliveryShift != null)
			{
				query.Where(o => o.Shift == FilterViewModel.DeliveryShift);
			}

			if(FilterViewModel.StartDate != null)
			{
				query.Where(o => o.Date >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null)
			{
				query.Where(o => o.Date <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.GeographicGroup != null)
			{
				query.Left.JoinAlias(o => o.GeographicGroups, () => geographicalGroupAlias)
					 .Where(() => geographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
			}

			#region RouteListAddressTypeFilter

			if(FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.VisitingMaster);
			}
			else if(FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.VisitingMaster);
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
			{
				query.Where(Restrictions.Or(
					Restrictions.Where(() => driverAlias.VisitingMaster),
					Restrictions.Where(() => driverAlias.IsChainStoreDriver)
				));
			}
			else if(!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => driverAlias.IsChainStoreDriver);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => driverAlias.VisitingMaster);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => routeListAlias.Id == null);
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

			switch(FilterViewModel.TransportType)
			{
				case RLFilterTransport.Mercenaries:
					query.Where(() => carVersionAlias.CarOwnershipType == CarOwnershipType.HiredCar); break;
				case RLFilterTransport.Raskat:
					query.Where(() => carVersionAlias.IsRaskat); break;
				case RLFilterTransport.Largus:
					query.Where(() => carModelAlias.TypeOfUse == CarTypeOfUse.Largus); break;
				case RLFilterTransport.GAZelle:
					query.Where(() => carModelAlias.TypeOfUse == CarTypeOfUse.GAZelle); break;
				case RLFilterTransport.Waggon:
					query.Where(() => carModelAlias.TypeOfUse == CarTypeOfUse.Truck); break;
				case RLFilterTransport.Others:
					query.Where(() => carVersionAlias.CarOwnershipType == CarOwnershipType.RaskatCar || carVersionAlias.CarOwnershipType == CarOwnershipType.HiredCar); break;
				default: break;
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
				() => carAlias.CarModel,
				() => carAlias.RegistrationNumber
			));

			var result = query
				.SelectList(list => list
				   .SelectGroup(() => routeListAlias.Id).WithAlias(() => routeListJournalNodeAlias.Id)
					   .Select(() => routeListAlias.Date).WithAlias(() => routeListJournalNodeAlias.Date)
					   .Select(() => routeListAlias.Status).WithAlias(() => routeListJournalNodeAlias.StatusEnum)
					   .Select(() => shiftAlias.Name).WithAlias(() => routeListJournalNodeAlias.ShiftName)
					   .Select(() => carAlias.CarModel).WithAlias(() => routeListJournalNodeAlias.CarModel)
					   .Select(() => carAlias.RegistrationNumber).WithAlias(() => routeListJournalNodeAlias.CarNumber)
					   .Select(() => driverAlias.LastName).WithAlias(() => routeListJournalNodeAlias.DriverSurname)
					   .Select(() => driverAlias.Name).WithAlias(() => routeListJournalNodeAlias.DriverName)
					   .Select(() => driverAlias.Patronymic).WithAlias(() => routeListJournalNodeAlias.DriverPatronymic)
					   .Select(() => routeListAlias.LogisticiansComment).WithAlias(() => routeListJournalNodeAlias.LogisticiansComment)
					   .Select(() => routeListAlias.ClosingComment).WithAlias(() => routeListJournalNodeAlias.ClosinComments)
					   .Select(() => subdivisionAlias.Name).WithAlias(() => routeListJournalNodeAlias.ClosingSubdivision)
					   .Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => routeListJournalNodeAlias.NotFullyLoaded)
					   .Select(() => carModelAlias.TypeOfUse).WithAlias(() => routeListJournalNodeAlias.CarTypeOfUse)
					   .Select(() => carVersionAlias.CarOwnershipType).WithAlias(() => routeListJournalNodeAlias.CarOwnershipType)
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
			RouteListStatus.Closed
		};

		private readonly RouteListStatus[] _canGiveChangeForRL = new[]
{
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute
		};

		#endregion

		protected override Func<RouteListJournalNode, TdiTabBase> OpenDialogFunction => (node) =>
		{
			switch(node.StatusEnum)
			{
				case RouteListStatus.New:
				case RouteListStatus.Confirmed:
					return new RouteListCreateDlg(node.Id);
				case RouteListStatus.InLoading:
					if(_routeListRepository.IsTerminalRequired(UoW, node.Id))
					{
						return new CarLoadDocumentDlg(node.Id, null);
					}
					else
					{
						return new RouteListCreateDlg(node.Id);
					}
				case RouteListStatus.EnRoute:
					return new RouteListKeepingDlg(node.Id);
				case RouteListStatus.Delivered:
				case RouteListStatus.OnClosing:
				case RouteListStatus.MileageCheck:
				case RouteListStatus.Closed:
					return new RouteListClosingDlg(node.Id);
				default:
					throw new InvalidOperationException("Неизвестный статус МЛ");
			}
		};

		protected void InitPopupActions()
		{
			var callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					_callTaskRepository,
					new OrderRepository(),
					new EmployeeRepository(),
					_baseParametersProvider,
					commonServices.UserService,
					SingletonErrorReporter.Instance);

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
								commonServices,
								_subdivisionRepository,
								new EmployeeRepository(),
								_fuelRepository,
								NavigationManagerProvider.NavigationManager,
								new TrackRepository(),
								new CategoryRepository(new ParametersProvider())
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
								routeList.ChangeStatusAndCreateTask(RouteListStatus.OnClosing, callTaskWorker);
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
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						var routeList = UoW.GetById<RouteList>(selectedNode.Id);
						var driverId = routeList.Driver.Id;

						if(_accountableDebtsRepository.UnclosedAdvance(UoW,
							routeList.Driver,
							UoW.GetById<ExpenseCategory>(_expenseParametersProvider.ChangeCategoryId),
							null).Count > 0)
						{
							commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error,
								"Закройте сначала прошлые авансовые со статусом \"Сдача клиенту\"", "Нельзя выдать сдачу");
							return;
						}

						var changesToOrders = routeList.GetCashChangesForOrders();

						if(!changesToOrders.Any())
						{
							commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Info, "Для данного МЛ нет наличных заказов требующих сдачи");
							return;
						}

						_gtkTabsOpener.OpenRouteListChangeGiveoutExpenceDlg(this,
							driverId,
							changesToOrders.Sum(item => item.Value),
							$"Сдача по МЛ №{routeList.Id}" +
							$"\n-----" +
							"\n" + string.Join("\n", changesToOrders.Select(pair => $"Заказ №{pair.Key} - {pair.Value}руб.")),
							commonServices.PermissionService);
					}
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
			CreateDefaultEditAction();
		}
	}
}
