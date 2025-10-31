using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Errors.Common;
using Vodovoz.Filters.ViewModels;
using Vodovoz.NotificationSenders;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.NotificationSenders;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class RouteListTransferringViewModel : DialogTabViewModelBase
	{
		public delegate IPage OpenLegacyOrderForRouteListJournalViewModel(Action<OrderJournalFilterViewModel> filterConfig);

		public OpenLegacyOrderForRouteListJournalViewModel OpenLegacyOrderForRouteListJournalViewModelHandler { get; set; }

		private readonly ILogger<RouteListTransferringViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeNomenclatureMovementRepository _employeeNomenclatureMovementRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListTransferService _routeListTransferService;
		private readonly IUserService _userService;
		private readonly IEmployeeService _employeeService;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private int? _sourceRouteListId;
		private RouteList _sourceRouteList;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListChangesNotificationSender _routeListChangesNotificationSender;

		private readonly RouteListStatus[] _defaultSourceRouteListStatuses =
		{
			RouteListStatus.EnRoute
		};

		private const int _defaultSourceRouteListStartDateOffsetDays = -3;

		private readonly DateTime _defaultSourceRouteListStartDate =
			DateTime.Today.AddDays(_defaultSourceRouteListStartDateOffsetDays);

		private const int _defaultSourceRouteListEndDateOffsetDays = 1;

		private readonly DateTime _defaultSourceRouteListEndDate =
			DateTime.Today.AddDays(_defaultSourceRouteListEndDateOffsetDays);

		private int? _targetRouteListId;
		private RouteList _targetRouteList;

		private readonly RouteListStatus[] _defaultTargetRouteListStatuses =
		{
			RouteListStatus.New,
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute
		};

		private const int _defaultTargetRouteListStartDateOffsetDays = -3;

		private readonly DateTime _defaultTargetRouteListStartDate =
			DateTime.Today.AddDays(_defaultTargetRouteListStartDateOffsetDays);

		private const int _defaultTargetRouteListEndDateOffsetDays = 1;

		private readonly DateTime _defaultTargetRouteListEndDate =
			DateTime.Today.AddDays(_defaultTargetRouteListEndDateOffsetDays);
		private object[] _selectedSourceRouteListAddresses = new object[] { };
		private object[] _selectedTargetRouteListAddresses = new object[] { };

		public RouteListTransferringViewModel(
			ILogger<RouteListTransferringViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEmployeeNomenclatureMovementRepository employeeNomenclatureMovementRepository,
			INomenclatureSettings nomenclatureSettings,
			IRouteListRepository routeListRepository,
			IRouteListTransferService routeListTransferService,
			IUserService userService,
			IEmployeeService employeeService,
			IGtkTabsOpener gtkTabsOpener,
			IRouteListItemRepository routeListItemRepository,
			RouteListJournalFilterViewModel sourceRouteListJournalFilterViewModel,
			RouteListJournalFilterViewModel targetRouteListJournalFilterViewModel,
			DeliveryFreeBalanceViewModel sourceDeliveryFreeBalanceViewModel,
			DeliveryFreeBalanceViewModel targetDeliveryFreeBalanceViewModel,
			ViewModelEEVMBuilder<RouteList> sourceRouteListEEVMBuilder,
			ViewModelEEVMBuilder<RouteList> targetRouteListEEVMBuilder,
			IRouteListChangesNotificationSender routeListChangesNotificationSender)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeNomenclatureMovementRepository = employeeNomenclatureMovementRepository
				?? throw new ArgumentNullException(nameof(employeeNomenclatureMovementRepository));
			_nomenclatureSettings = nomenclatureSettings
				?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeListRepository = routeListRepository
				?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListTransferService = routeListTransferService
				?? throw new ArgumentNullException(nameof(routeListTransferService));
			_userService = userService
				?? throw new ArgumentNullException(nameof(userService));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_gtkTabsOpener = gtkTabsOpener
				?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_routeListItemRepository = routeListItemRepository
				?? throw new ArgumentNullException(nameof(routeListItemRepository));
			SourceRouteListJournalFilterViewModel = sourceRouteListJournalFilterViewModel
				?? throw new ArgumentNullException(nameof(sourceRouteListJournalFilterViewModel));
			TargetRouteListJournalFilterViewModel = targetRouteListJournalFilterViewModel
				?? throw new ArgumentNullException(nameof(targetRouteListJournalFilterViewModel));
			SourceRouteListDeliveryFreeBalanceViewModel = sourceDeliveryFreeBalanceViewModel
				?? throw new ArgumentNullException(nameof(sourceDeliveryFreeBalanceViewModel));
			TargetRouteListDeliveryFreeBalanceViewModel = targetDeliveryFreeBalanceViewModel
				?? throw new ArgumentNullException(nameof(targetDeliveryFreeBalanceViewModel));
			_routeListChangesNotificationSender = routeListChangesNotificationSender
				?? throw new ArgumentNullException(nameof(routeListChangesNotificationSender));

			SourceRouteListJournalFilterViewModel.SetAndRefilterAtOnce(filter =>
			{
				filter.DisplayableStatuses = _defaultSourceRouteListStatuses;
				filter.StartDate = _defaultSourceRouteListStartDate;
				filter.EndDate = _defaultSourceRouteListEndDate;
				filter.AddressTypeNodes.ForEach(x => x.Selected = true);
				filter.ExcludeIds = ExcludeIds;
			});

			SourceRouteListJournalFilterViewModel.DisposeOnDestroy = false;

			TargetRouteListJournalFilterViewModel.SetAndRefilterAtOnce(filter =>
			{
				filter.DisplayableStatuses = _defaultTargetRouteListStatuses;
				filter.StartDate = _defaultTargetRouteListStartDate;
				filter.EndDate = _defaultTargetRouteListEndDate;
				filter.AddressTypeNodes.ForEach(x => x.Selected = true);
				filter.ExcludeIds = ExcludeIds;
			});

			TargetRouteListJournalFilterViewModel.DisposeOnDestroy = false;

			TabName = "Перенос адресов маршрутных листов";

			AddOrderToRouteListEnRouteCommand =
				new DelegateCommand(() => SelectNewOrdersForRouteListEnRoute());

			TransferAddressesCommand = new DelegateCommand(TransferAddresses);
			RevertTransferAddressesCommand = new DelegateCommand(RevertTransferAddresses, () => CanRevertTransferAddresses);

			TransferTerminalCommand = new DelegateCommand(TransferTerminal, () => IsTargetAndSourceRouteListsSelected);

			RevertTransferTerminalCommand = new DelegateCommand(RevertTransferTerminal, () => IsTargetAndSourceRouteListsSelected);

			SourceRouteListViewModel = sourceRouteListEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, vm => vm.SourceRouteList)
				.UseViewModelDialog<RouteListCreateViewModel>()
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(SourceRouteListJournalFilterViewModel)
				.Finish();

			TargetRouteListViewModel = targetRouteListEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, vm => vm.TargetRouteList)
				.UseViewModelDialog<RouteListCreateViewModel>()
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(TargetRouteListJournalFilterViewModel)
				.Finish();

			PropertyChanged += OnRouteListTransferringViewModelPropertyChanged;
		}

		private void OnRouteListTransferringViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(SourceRouteListId)
				|| e.PropertyName == nameof(TargetRouteListId))
			{
				TargetRouteListJournalFilterViewModel.ExcludeIds = ExcludeIds;
				SourceRouteListJournalFilterViewModel.ExcludeIds = ExcludeIds;
			}
		}

		#region Source RouteList

		public int? SourceRouteListId
		{
			get => _sourceRouteListId;
			set
			{
				if(_sourceRouteListId != null)
				{
					UoW.Session.Evict(SourceRouteList);
				}

				if(SetField(ref _sourceRouteListId, value))
				{
					if(value is null)
					{
						SourceRouteList = null;
						return;
					}

					SourceRouteList = UoW.GetById<RouteList>(value.Value);
				}
			}
		}

		[PropertyChangedAlso(
			nameof(SourceRouteListId),
			nameof(IsSourceRouteListSelected),
			nameof(IsTargetAndSourceRouteListsSelected),
			nameof(CanAddOrder),
			nameof(CanTransferTerminal))]
		public RouteList SourceRouteList
		{
			get => _sourceRouteList;
			set
			{
				if(value != null
					&& _gtkTabsOpener.FindAndSwitchOnTab<RouteList>(value.Id))
				{
					value = null;
					OnPropertyChanged(nameof(SourceRouteList));
				}

				if(IsTargetRouteListSelected && value?.Id == TargetRouteList?.Id)
				{
					value = null;
					OnPropertyChanged(nameof(SourceRouteList));
				}

				if(SetField(ref _sourceRouteList, value))
				{
					_sourceRouteListId = value?.Id;

					RefreshSourceRouteListAddresses();

					if(_sourceRouteListId != null)
					{
						SourceRouteList.UoW = UoW;
					}

					FillObservableDriverBalance(SourceRouteListDriverNomenclatureBalance, SourceRouteList);

					RefreshSourceFreeBalanceOperations();

					OnPropertyChanged(nameof(CanTransferTerminal));
				}
			}
		}

		public IEntityEntryViewModel SourceRouteListViewModel { get; }

		public GenericObservableList<RouteListItemNode> SourceRouteListAddresses { get; }
			= new GenericObservableList<RouteListItemNode>();

		[PropertyChangedAlso(nameof(CanTransferAddress))]
		public object[] SelectedSourceRouteListAddresses
		{
			get => _selectedSourceRouteListAddresses;
			set => SetField(ref _selectedSourceRouteListAddresses, value);
		}

		#endregion Source RouteList

		#region Target RouteList

		public int? TargetRouteListId
		{
			get => _targetRouteListId;
			set
			{
				if(_targetRouteListId != null)
				{
					UoW.Session.Evict(TargetRouteList);
				}

				if(SetField(ref _targetRouteListId, value))
				{
					if(value is null)
					{
						TargetRouteList = null;
						return;
					}

					TargetRouteList = UoW.GetById<RouteList>(value.Value);
				}
			}
		}

		[PropertyChangedAlso(
			nameof(IsTargetRouteListSelected),
			nameof(TargetRouteListId),
			nameof(CanRevertTransferTerminal),
			nameof(IsTargetAndSourceRouteListsSelected),
			nameof(CanTransferAddress))]
		public RouteList TargetRouteList
		{
			get => _targetRouteList;
			private set
			{
				if(value != null
					&& _gtkTabsOpener.FindAndSwitchOnTab<RouteList>(value.Id))
				{
					value = null;
					OnPropertyChanged(nameof(TargetRouteList));
				}

				if(IsSourceRouteListSelected && value?.Id == SourceRouteList?.Id)
				{
					value = null;
					OnPropertyChanged(nameof(TargetRouteList));
				}

				if(SetField(ref _targetRouteList, value))
				{
					_targetRouteListId = value?.Id;

					RefreshTargetRouteListAddresses();

					if(_targetRouteList != null)
					{
						TargetRouteList.UoW = UoW;
					}

					FillObservableDriverBalance(TargetRouteListDriverNomenclatureBalance, TargetRouteList);

					RefreshTargetFreeBalanceOperations();

					OnPropertyChanged(nameof(CanRevertTransferTerminal));
				}
			}
		}

		public IEntityEntryViewModel TargetRouteListViewModel { get; }

		public GenericObservableList<RouteListItemNode> TargetRouteListAddresses { get; }
			= new GenericObservableList<RouteListItemNode>();

		[PropertyChangedAlso(nameof(CanRevertTransferAddresses))]
		public object[] SelectedTargetRouteListAddresses
		{
			get => _selectedTargetRouteListAddresses;
			set => SetField(ref _selectedTargetRouteListAddresses, value);
		}

		#endregion Target RouteList

		public bool CanAddOrder => !IsSourceRouteListSelected;

		public bool CanTransferAddress =>
			IsTargetRouteListSelected
			&& SelectedSourceRouteListAddresses.Any()
			&& SelectedSourceRouteListAddresses
				.Cast<RouteListItemNode>()
				.All(x => x.AddressStatus == RouteListItemStatus.EnRoute
					|| x.Order != null);

		public bool CanRevertTransferAddresses => SelectedTargetRouteListAddresses.Any()
			&& SelectedTargetRouteListAddresses
				.Cast<RouteListItemNode>()
				.All(x => x.AddressStatus == RouteListItemStatus.EnRoute
					&& (x.WasTransfered
						|| (x.IsFromFreeBalance && x.AddressStatus != RouteListItemStatus.Transfered)));

		[PropertyChangedAlso(
			nameof(IsTargetAndSourceRouteListsSelected),
			nameof(CanAddOrder))]
		public bool IsSourceRouteListSelected => !(SourceRouteList is null);

		[PropertyChangedAlso(
			nameof(IsTargetAndSourceRouteListsSelected),
			nameof(CanTransferAddress))]
		public bool IsTargetRouteListSelected => !(TargetRouteList is null);
		public bool IsTargetAndSourceRouteListsSelected => IsTargetRouteListSelected && IsSourceRouteListSelected;

		public int[] ExcludeIds => SourceRouteListId.HasValue
			? TargetRouteListId.HasValue
				? new[] { SourceRouteListId.Value, TargetRouteListId.Value }
				: new[] { SourceRouteListId.Value }
			: TargetRouteListId.HasValue
				? new[] { TargetRouteListId.Value }
				: new int[] { };

		public DeliveryFreeBalanceViewModel SourceRouteListDeliveryFreeBalanceViewModel { get; }
		public DeliveryFreeBalanceViewModel TargetRouteListDeliveryFreeBalanceViewModel { get; }
		public DelegateCommand AddOrderToRouteListEnRouteCommand { get; }
		public DelegateCommand TransferAddressesCommand { get; }
		public DelegateCommand RevertTransferAddressesCommand { get; }
		public DelegateCommand TransferTerminalCommand { get; }
		public DelegateCommand RevertTransferTerminalCommand { get; }

		public GenericObservableList<EmployeeBalanceNode> SourceRouteListDriverNomenclatureBalance { get; }
			= new GenericObservableList<EmployeeBalanceNode>();

		public bool CanTransferTerminal => SourceRouteListDriverNomenclatureBalance.Any();

		public GenericObservableList<EmployeeBalanceNode> TargetRouteListDriverNomenclatureBalance { get; }
			= new GenericObservableList<EmployeeBalanceNode>();

		public bool CanRevertTransferTerminal => TargetRouteListDriverNomenclatureBalance.Any();

		public RouteListJournalFilterViewModel SourceRouteListJournalFilterViewModel { get; }

		public RouteListJournalFilterViewModel TargetRouteListJournalFilterViewModel { get; }

		private void RefreshSourceRouteListAddresses()
		{
			SourceRouteListAddresses.Clear();

			if(SourceRouteList != null)
			{
				foreach(var routeListItem in SourceRouteList.Addresses.Reverse())
				{
					SourceRouteListAddresses.Insert(0, new RouteListItemNode { RouteListItem = routeListItem });
				}
			}
		}

		private void RefreshTargetRouteListAddresses()
		{
			TargetRouteListAddresses.Clear();

			if(TargetRouteList != null)
			{
				foreach(var routeListItem in TargetRouteList.Addresses)
				{
					TargetRouteListAddresses.Add(new RouteListItemNode { RouteListItem = routeListItem });
				}
			}
		}

		private void RefreshSourceFreeBalanceOperations()
		{
			if(!IsSourceRouteListSelected)
			{
				SourceRouteListDeliveryFreeBalanceViewModel
					.ObservableDeliveryFreeBalanceOperations =
						new GenericObservableList<DeliveryFreeBalanceOperation>();

				return;
			}

			SourceRouteListDeliveryFreeBalanceViewModel
				.ObservableDeliveryFreeBalanceOperations =
					SourceRouteList.ObservableDeliveryFreeBalanceOperations;
		}

		private void RefreshTargetFreeBalanceOperations()
		{
			if(!IsTargetRouteListSelected)
			{
				TargetRouteListDeliveryFreeBalanceViewModel
					.ObservableDeliveryFreeBalanceOperations =
						new GenericObservableList<DeliveryFreeBalanceOperation>();

				return;
			}

			TargetRouteListDeliveryFreeBalanceViewModel
				.ObservableDeliveryFreeBalanceOperations =
					TargetRouteList.ObservableDeliveryFreeBalanceOperations;
		}

		private void SelectNewOrdersForRouteListEnRoute()
		{
			var excludeOrdersIds =
				TargetRouteListAddresses
					.Where(addressNode => addressNode.RouteListItem != null)
					.Select(addressNode => addressNode.OrderId)
					.Union(SourceRouteListAddresses
						.Select(addressNode => addressNode.OrderId))
					.ToArray();

			var ordersForRouteListJournalPage = OpenLegacyOrderForRouteListJournalViewModelHandler(filter =>
			{
				filter.RestrictFilterDateType = OrdersDateFilterType.DeliveryDate;
				filter.RestrictStatus = OrderStatus.Accepted;
				filter.RestrictWithoutSelfDelivery = true;
				filter.RestrictOnlySelfDelivery = false;
				filter.RestrictHideService = true;
				filter.FilterClosingDocumentDeliverySchedule = false;
				filter.ExceptIds = excludeOrdersIds;
			});
		}

		public void OnOrderSelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			foreach(var selectedNode in e.SelectedNodes)
			{
				AddOrderToSourceAddresses(selectedNode.Id);
			}

			OnPropertyChanged(nameof(CanTransferAddress));
		}

		private void AddOrderToSourceAddresses(int orderId)
		{
			var order = UoW.GetById<Order>(orderId);

			SourceRouteListAddresses.Add(new RouteListItemNode
			{
				Order = order,
				AddressTransferType = AddressTransferType.FromFreeBalance
			});
		}

		private void FillObservableDriverBalance(GenericObservableList<EmployeeBalanceNode> observableDriverBalance, RouteList routeList)
		{
			observableDriverBalance.Clear();

			if(routeList is null)
			{
				return;
			}

			var driverTerminalBalance = _employeeNomenclatureMovementRepository.GetTerminalFromDriverBalance(UoW,
				routeList.Driver.Id,
				_nomenclatureSettings.NomenclatureIdForTerminal);

			if(driverTerminalBalance != null)
			{
				observableDriverBalance.Add(driverTerminalBalance);
			}
		}

		private void TransferAddresses()
		{
			var messages = new List<string>();

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title + " > перенос адресов"))
			{
				var sourceRouteListId = SourceRouteListId;
				var targetRouteListId = TargetRouteListId;

				try
				{
					#region Добавляемые принятые заказы (не переносимые из какого-либо МЛ)

					var ordersWithTransferTypesWithoutRouteList = SelectedSourceRouteListAddresses
						.Cast<RouteListItemNode>()
						.Where(x => x.Order != null);

					var ordersIdsWithTransferTypesWithoutRouteList = ordersWithTransferTypesWithoutRouteList
						.ToDictionary(
							x => x.OrderId,
							x => x.AddressTransferType);

					Result<IEnumerable<string>> ordersTransferResult = _routeListTransferService.TransferOrdersTo(
						unitOfWork,
						TargetRouteListId.Value,
						ordersIdsWithTransferTypesWithoutRouteList);

					#endregion

					Result<IEnumerable<string>> addressesTransferResult = null;

					var selectedToTransferNodesWithRouteListAddresses = SelectedSourceRouteListAddresses
						.Cast<RouteListItemNode>()
						.Where(x => x.RouteListItem != null);


					var routeListAddressesWithTransferTypes = selectedToTransferNodesWithRouteListAddresses
						.ToDictionary(
							x => x.AddressId.Value,
							x => x.AddressTransferType);

					if(IsSourceRouteListSelected)
					{
						addressesTransferResult =
							_routeListTransferService.TransferAddressesFrom(
								unitOfWork,
								SourceRouteListId.Value,
								TargetRouteListId.Value,
								routeListAddressesWithTransferTypes);
					}

					if((addressesTransferResult?.IsSuccess ?? true)
						&& ordersTransferResult.IsSuccess)
					{
						SourceRouteListId = null;
						TargetRouteListId = null;

						unitOfWork.Commit();

						TargetRouteListId = targetRouteListId;
						SourceRouteListId = sourceRouteListId;

						if(!IsSourceRouteListSelected)
						{
							var ordersToRemove = SourceRouteListAddresses
								.Where(x => ordersIdsWithTransferTypesWithoutRouteList.Keys.Contains(x.OrderId))
								.ToList();

							foreach(var orderToRemove in ordersToRemove)
							{
								SourceRouteListAddresses.Remove(orderToRemove);
							}

							ordersToRemove.Clear();
						}

						ShowTransferInformation(ordersTransferResult.Value);

						if(addressesTransferResult != null)
						{
							ShowTransferInformation(addressesTransferResult.Value);
						}

						var selectedToTransferNodes =
							ordersWithTransferTypesWithoutRouteList
							.Concat(selectedToTransferNodesWithRouteListAddresses);

						NotifyOfTransferTransfered(selectedToTransferNodes);

						return;
					}

					if(ordersTransferResult.IsFailure)
					{
						ShowTransferErrors(ordersTransferResult.Errors);
					}

					if(addressesTransferResult?.IsFailure ?? false)
					{
						ShowTransferErrors(addressesTransferResult.Errors);
					}
				}
				catch(Exception ex)
				{
					ShowErrorMessage($"Произошла ошибка при переносе адресов: {ex.Message}", "Ошибка");

					_logger.LogError(ex, "Произошла ошибка при переносе адресов");

					var transaction = unitOfWork.Session.GetCurrentTransaction();

					if(transaction != null && !(transaction?.WasRolledBack ?? true))
					{
						transaction.Rollback();
					}
				}
			}
		}

		private void ShowTransferErrors(IEnumerable<Error> errors)
		{
			var routeListNotFound = errors
				.Where(x => x.Code == Errors.Logistics.RouteListErrors.NotFound)
				.Select(x => x.Message)
				.ToList();

			var transferTypeNotSet = errors
				.Where(x => x.Code == Errors.Logistics.RouteListErrors.RouteListItem.TransferTypeNotSet)
				.Select(x => x.Message)
				.ToList();

			var transferRequiresLoadingWhenRouteListEnRoute = errors
				.Where(x => x.Code == Errors.Logistics.RouteListErrors.RouteListItem.TransferRequiresLoadingWhenRouteListEnRoute)
				.Select(x => x.Message)
				.ToList();

			var transferNotEnoughFreeBalance = errors
				.Where(x => x.Code == Errors.Logistics.RouteListErrors.RouteListItem.TransferNotEnoughtFreeBalance)
				.Select(x => x.Message)
				.ToList();

			var driverApiClientRequestIsNotSuccess = errors
				.Where(x => x.Code == DriverApiClientErrors.RequestIsNotSuccess(x.Message))
				.Select(x => x.Message)
				.ToList();

			var ordersWithCreatedUpdNeedToReload = errors
				.Where(x => x.Code == Errors.Logistics.RouteListErrors.RouteListItem.OrdersWithCreatedUpdNeedToReload)
				.Select(x => x.Message)
				.ToList();

			_interactiveService.ShowMessage(ImportanceLevel.Error,
				"Перенос не был осуществлен:\n" +
				string.Join(",\n",
					routeListNotFound) +
				string.Join(",\n",
					transferTypeNotSet) +
				string.Join(",\n",
					transferRequiresLoadingWhenRouteListEnRoute) +
				string.Join(",\n",
					transferNotEnoughFreeBalance) +
				string.Join(",\n",
					driverApiClientRequestIsNotSuccess) +
				string.Join(",\n",
					ordersWithCreatedUpdNeedToReload),
				"Ошибка при переносе адресов");
		}

		private void ShowTransferWarnings(IEnumerable<Error> errors)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning,
				string.Join(",\n",
					errors.Select(e => e.Message)),
				"Внимание!");
		}

		private void ShowTransferInformation(IEnumerable<string> messages)
		{
			if(!messages.Any())
			{
				return;
			}

			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Были выполнены следующие действия:\n" +
				$"* {string.Join("\n* ", messages)}");
		}

		private void RevertTransferAddresses()
		{
			var addressesToRevertIds = SelectedTargetRouteListAddresses
				.Cast<RouteListItemNode>()
				.Where(x => x.AddressStatus != RouteListItemStatus.Transfered)
				.Select(x => x.AddressId.Value)
				.ToList();

			var ordersToRestore = SelectedTargetRouteListAddresses
				.Cast<RouteListItemNode>()
				.Where(x => x.AddressStatus != RouteListItemStatus.Transfered)
				.Select(x => x.OrderId)
				.ToList();

			var selectedTargetAddresses = SelectedTargetRouteListAddresses
				.Cast<RouteListItemNode>();


			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title + " > возврат переноса адресов"))
			{
				try
				{
					var result = _routeListTransferService.RevertTransferedAddressesFrom(unitOfWork, TargetRouteListId.Value, SourceRouteListId, addressesToRevertIds);

					if(result.IsSuccess)
					{
						var targetRouteListId = TargetRouteListId;

						TargetRouteListId = null;

						var sourceRouteListId = SourceRouteListId;

						SourceRouteListId = null;

						unitOfWork.Commit();

						SourceRouteListId = sourceRouteListId;

						TargetRouteListId = targetRouteListId;

						if(!IsSourceRouteListSelected)
						{
							foreach(var orderIds in ordersToRestore)
							{
								var needShowInSource = !_routeListItemRepository.GetRouteListItemsForOrder(UoW, orderIds).Any();

								if(needShowInSource)
								{
									AddOrderToSourceAddresses(orderIds);
								}
							}
						}

						NotifyOfTransferTransfered(selectedTargetAddresses);
					}

					result.Match(
						ShowTransferInformation,
						ShowTransferErrors);
				}
				catch(Exception ex)
				{
					ShowErrorMessage($"Произошла ошибка при возврате переноса адресов: {ex.Message}", "Ошибка");

					_logger.LogError(ex, "Произошла ошибка при возврате переноса адресов");

					var transaction = unitOfWork.Session.GetCurrentTransaction();

					if(transaction != null && !(transaction?.WasRolledBack ?? true))
					{
						transaction.Rollback();
					}
				}
			}
		}

		private void NotifyOfTransferTransfered(IEnumerable<RouteListItemNode> selectedHandToHandNodes)
		{
			if(!selectedHandToHandNodes.Any())
			{
				return;
			}

			var notifyingErrors = new List<Error>();

			foreach(var node in selectedHandToHandNodes)
			{
				try
				{
					var isTransfer = node.RouteListItem != null;

					var notificationRequest = new NotificationRouteListChangesRequest
					{
						OrderId = node.OrderId,
						PushNotificationDataEventType = isTransfer && node.AddressTransferType == AddressTransferType.FromHandToHand
							? PushNotificationDataEventType.TransferAddressFromHandToHand
							: PushNotificationDataEventType.RouteListContentChanged
					};

					var result = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

					if(result.IsSuccess)
					{
						continue;
					}
					else
					{
						notifyingErrors.AddRange(result.Errors.Where(x => x.Code != 
                          $"{typeof(Errors.Common.DriverApiClientErrors).Namespace}" +
                          $".{typeof(Errors.Common.DriverApiClientErrors).Name}" +
                          $".{nameof(Errors.Common.DriverApiClientErrors.OrderWithGoodsTransferingIsTransferedNotNotified)}"));
					}
				}
				catch(Exception ex)
				{
					notifyingErrors.Add(DriverApiClientErrors.RequestIsNotSuccess(ex.Message));
				}
			}

			if(notifyingErrors.Any())
			{
				ShowTransferErrors(notifyingErrors);
			}
		}

		private void TransferTerminal()
		{
			var selectedNode = SourceRouteListDriverNomenclatureBalance
				.Cast<EmployeeBalanceNode>()
				.FirstOrDefault();

			if(selectedNode is null)
			{
				return;
			}

			if(selectedNode.Amount == 0)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Вы не можете передавать терминал, т.к. его нет на балансе у водителя.",
					"Ошибка");
				return;
			}

			var giveoutDocFrom = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, SourceRouteList?.Driver);

			if(giveoutDocFrom is DriverAttachedTerminalGiveoutDocument)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Нельзя передать терминал от водителя {SourceRouteList?.Driver.GetPersonNameWithInitials()}, " +
					$"к которому привязан терминал.\r\nВодителю {TargetRouteList?.Driver.GetPersonNameWithInitials()}, " +
					"которому передается заказ, необходима допогрузка", "Ошибка");
				return;
			}

			if(TargetRouteListDriverNomenclatureBalance
				.Any(x => x.NomenclatureId == _nomenclatureSettings.NomenclatureIdForTerminal
					&& x.Amount > 0))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"У водителя уже есть терминал для оплаты.",
					"Ошибка");
				return;
			}

			var terminal = UoW.GetById<Nomenclature>(selectedNode.NomenclatureId);

			var operationFrom = new EmployeeNomenclatureMovementOperation
			{
				Employee = SourceRouteList.Driver,
				Nomenclature = terminal,
				Amount = -1,
				OperationTime = DateTime.Now
			};

			var operationTo = new EmployeeNomenclatureMovementOperation
			{
				Employee = TargetRouteList.Driver,
				Nomenclature = terminal,
				Amount = 1,
				OperationTime = DateTime.Now
			};

			var driverTerminalTransferDocument = new AnotherDriverTerminalTransferDocument()
			{
				Author = _employeeService.GetEmployeeForUser(UoW, _userService.CurrentUserId),
				CreateDate = DateTime.Now,
				DriverFrom = SourceRouteList.Driver,
				DriverTo = TargetRouteList.Driver,
				RouteListFrom = SourceRouteList,
				RouteListTo = TargetRouteList,
				EmployeeNomenclatureMovementOperationFrom = operationFrom,
				EmployeeNomenclatureMovementOperationTo = operationTo
			};

			UoW.Save(driverTerminalTransferDocument);
			UoW.Save(operationFrom);
			UoW.Save(operationTo);
			UoW.Commit();

			FillObservableDriverBalance(SourceRouteListDriverNomenclatureBalance, SourceRouteList);
			FillObservableDriverBalance(TargetRouteListDriverNomenclatureBalance, TargetRouteList);
		}

		public void RevertTransferTerminal()
		{
			var selectedNode = TargetRouteListDriverNomenclatureBalance
				.Cast<EmployeeBalanceNode>()
				.FirstOrDefault();

			if(selectedNode is null)
			{
				return;
			}

			if(selectedNode.Amount == 0)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"Вы не можете передавать терминал, т.к. его нет на балансе у водителя.",
					"Ошибка");
				return;
			}

			if(SourceRouteListDriverNomenclatureBalance.Any(x =>
				x.NomenclatureId == _nomenclatureSettings.NomenclatureIdForTerminal && x.Amount > 0))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "У водителя уже есть терминал для оплаты.", "Ошибка");
				return;
			}

			var giveoutDocTo = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, SourceRouteList?.Driver);
			if(giveoutDocTo is DriverAttachedTerminalGiveoutDocument)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Нельзя вернуть терминал от водителя {SourceRouteList?.Driver.GetPersonNameWithInitials()}" +
					", к которому привязан терминал.",
					"Ошибка");
				return;
			}

			var terminal = UoW.GetById<Nomenclature>(selectedNode.NomenclatureId);

			var operationFrom = new EmployeeNomenclatureMovementOperation
			{
				Employee = SourceRouteList.Driver,
				Nomenclature = terminal,
				Amount = -1,
				OperationTime = DateTime.Now
			};

			var operationTo = new EmployeeNomenclatureMovementOperation
			{
				Employee = TargetRouteList.Driver,
				Nomenclature = terminal,
				Amount = 1,
				OperationTime = DateTime.Now
			};

			var driverTerminalTransferDocument = new AnotherDriverTerminalTransferDocument()
			{
				Author = _employeeService.GetEmployeeForUser(UoW, _userService.CurrentUserId),
				CreateDate = DateTime.Now,
				DriverFrom = SourceRouteList.Driver,
				DriverTo = TargetRouteList.Driver,
				RouteListFrom = SourceRouteList,
				RouteListTo = TargetRouteList,
				EmployeeNomenclatureMovementOperationFrom = operationFrom,
				EmployeeNomenclatureMovementOperationTo = operationTo
			};

			UoW.Save(driverTerminalTransferDocument);
			UoW.Save(operationFrom);
			UoW.Save(operationTo);
			UoW.Commit();

			FillObservableDriverBalance(TargetRouteListDriverNomenclatureBalance, SourceRouteList);
			FillObservableDriverBalance(SourceRouteListDriverNomenclatureBalance, TargetRouteList);
		}

		public override void Dispose()
		{
			SourceRouteListJournalFilterViewModel?.Dispose();
			TargetRouteListJournalFilterViewModel?.Dispose();
			OpenLegacyOrderForRouteListJournalViewModelHandler = null;
			base.Dispose();
		}
	}
}
