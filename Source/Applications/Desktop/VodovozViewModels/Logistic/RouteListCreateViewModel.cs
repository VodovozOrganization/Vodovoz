using Autofac;
using Microsoft.Extensions.Logging;
using NHibernate;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Profitability;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using Vodovoz.Models;
using Vodovoz.Services.Logistics;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListCreateViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly ILogger<RouteListCreateViewModel> _logger;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IValidator _validator;
		private readonly IValidationViewFactory _validationViewFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListService _routeListService;
		private readonly IGenericRepository<RouteListSpecialConditionType> _routeListSpecialConditionTypeRepository;
		private readonly IDeliveryShiftRepository _deliveryShiftRepository;
		private readonly IAdditionalLoadingModel _additionalLoadingModel;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		private bool _canClose = true;
		private Employee _oldDriver;
		private DateTime _previousSelectedDate;

		public event EventHandler DocumentPrinted; 

		public RouteListCreateViewModel(
			ILogger<RouteListCreateViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			INavigationManager navigation,
			IValidator validator,
			IValidationViewFactory validationViewFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IEmployeeRepository employeeRepository,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			IRouteListService routeListService,
			IGenericRepository<RouteListSpecialConditionType> routeListSpecialConditionTypeRepository,
			IDeliveryShiftRepository deliveryShiftRepository,
			IAdditionalLoadingModel additionalLoadingModel,
			IRouteListProfitabilityController routeListProfitabilityController)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_validationViewFactory = validationViewFactory ?? throw new ArgumentNullException(nameof(validationViewFactory));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_routeListSpecialConditionTypeRepository = routeListSpecialConditionTypeRepository ?? throw new ArgumentNullException(nameof(routeListSpecialConditionTypeRepository));
			_deliveryShiftRepository = deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository));
			_additionalLoadingModel = additionalLoadingModel ?? throw new ArgumentNullException(nameof(additionalLoadingModel));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));

			if(uowBuilder.IsNewEntity)
			{
				Entity.Logistician = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(Entity.Logistician is null)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error, "Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
					FailInitialize = true;
					return;
				}

				Entity.Date = DateTime.Now;
			}

			CanEditFixedPrice = _currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.RouteList.CanChangeRouteListFixedPrice);
			CanСreateRoutelistInPastPeriod = _currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.RouteList.CanCreateRouteListInPastPeriod);
			IsLogistician = _currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.IsLogistician);
			IsCashier = _currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier);
			CanReadRouteListProfitability = _currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.RouteList.CanReadRouteListProfitability);
			CanOpenOrder = _currentPermissionService.ValidateEntityPermission(typeof(Order)).CanRead;

			_previousSelectedDate = Entity.Date;

			if(Entity.Id > 0)
			{
				//Нужно только для быстрой загрузки данных диалога. Проверено на МЛ из 200 заказов. Разница в скорости в несколько раз.
				var orders = UoW.Session.QueryOver<RouteListItem>()
					.Where(x => x.RouteList == Entity)
					.Fetch(SelectMode.Fetch, x => x.Order)
					.Fetch(SelectMode.ChildFetch, x => x.Order.OrderItems)
					.List();
			}

			DeliveryShiftsCache = _deliveryShiftRepository.ActiveShifts(UoW).ToList();

			SpecialConditions = _routeListService.GetSpecialConditionsFor(UoW, Entity.Id);

			var specialConditionsTypesIds = SpecialConditions.Select(x => x.RouteListSpecialConditionTypeId);

			SpecialConditionsTypes = specialConditionsTypesIds.Any()
				? _routeListSpecialConditionTypeRepository
					.Get(UoW, x => specialConditionsTypesIds.Contains(x.Id))
				: Enumerable.Empty<RouteListSpecialConditionType>();

			var hasAccessToDriverTerminal = IsLogistician || IsCashier;
			var baseDoc = _routeListRepository.GetLastTerminalDocumentForEmployee(UoW, Entity.Driver);

			DriverTerminalCondition = (HasAccessToDriverTerminal
				&& baseDoc is DriverAttachedTerminalGiveoutDocument
				&& baseDoc.CreationDate.Date <= Entity?.Date)
					? $"Состояние терминала: {Entity.DriverTerminalCondition?.GetEnumDisplayName() ?? "неизвестно"}"
					: "";

			_logger.LogDebug("Пересчитываем рентабельность МЛ");
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			_logger.LogDebug("Закончили пересчет рентабельности МЛ");

			RouteListProfitabilities = new GenericObservableList<RouteListProfitability>
			{
				Entity.RouteListProfitability
			};

			Entity.ObservableGeographicGroups.ListContentChanged += ObservableGeographicGroups_ListContentChanged;

			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));

			AcceptCommand = new DelegateCommand(AcceptHandler);
			RevertToNewCommand = new DelegateCommand(ReturnToNewHandler);

			AddAdditionalLoadingCommand = new DelegateCommand(AddAdditionalLoad);
			RemoveAdditionalLoadingCommand = new DelegateCommand(RemoveAdditionalLoad);

			PrintCommand = new DelegateCommand<RouteListPrintableDocuments>(PrintSelectedDocument);
			ShowPrintTimeCommand = new DelegateCommand(ShowPrintTime);

			CarViewModel = CreateCarViewModel();
			DriverViewModel = CreateDriverViewModel();
			ForwarderViewModel = CreateForwarderViewModel();
			LogisticianViewModel = CreateLogisticianViewModel();

			Entity.PropertyChanged += OnRouteListPropertyChanged;
		}

		public Action<bool> DisableItemsUpdateDelegate { get; set; }
		public List<DeliveryShift> DeliveryShiftsCache { get; private set; }
		public string DriverTerminalCondition { get; private set; }

		public GenericObservableList<RouteListProfitability> RouteListProfitabilities { get; }

		public IEnumerable<RouteListSpecialCondition> SpecialConditions { get; private set; }
		public IEnumerable<RouteListSpecialConditionType> SpecialConditionsTypes { get; private set; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand AcceptCommand { get; }
		public DelegateCommand RevertToNewCommand { get; }
		public DelegateCommand AddAdditionalLoadingCommand { get; }
		public DelegateCommand RemoveAdditionalLoadingCommand { get; }
		public DelegateCommand<RouteListPrintableDocuments> PrintCommand { get; }
		public DelegateCommand ShowPrintTimeCommand { get; }

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanEditFixedPrice { get; }
		public bool CanСreateRoutelistInPastPeriod { get; }
		public bool IsLogistician { get; }
		public bool IsCashier { get; }
		public bool CanReadRouteListProfitability { get; }
		public bool CanOpenOrder { get; }

		public bool HasAccessToDriverTerminal => IsLogistician || IsCashier;

		public bool AskSaveOnClose => PermissionResult.CanCreate && Entity.Id == 0 || PermissionResult.CanUpdate;

		public string ClosingSubdivisionName => Entity.ClosingSubdivision is null ? "Нет" : Entity.ClosingSubdivision.Name;

		public bool CanAddAdditionalLoad =>
			RouteList.NotLoadedRouteListStatuses.Contains(Entity.Status)
			&& Entity.AdditionalLoadingDocument is null
			&& CanAccept
			&& Entity.Date != default
			&& Entity.Car != null;

		public bool CanRemoveAdditionalLoad =>
			CanAccept
			&& Entity.AdditionalLoadingDocument != null;

		public bool AdditionalLoadItemsVisible => Entity.AdditionalLoadingDocument != null;

		public bool CanSave => IsLogistician && (CanCreate && UoW.IsNew || CanUpdate);

		public bool CanAccept => Entity.Status == RouteListStatus.New
			&& CanSave;

		public bool CanRevertToNew => Entity.Status != RouteListStatus.New
			&& RouteList.NotLoadedRouteListStatuses.Contains(Entity.Status)
			&& CanSave;

		public bool CanChangeDriver => CanAccept
			&& Entity.Car != null;

		public bool CanChangeForwarder => CanAccept
			&& ((Entity.Car is null || Entity.Date == default)
				|| (!Entity.GetCarVersion.IsCompanyCar
					|| Entity.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
					&& Entity.CanAddForwarder));

		public bool CanChangeFixedPrice => Entity.HasFixedShippingPrice
			&& Entity.Status != RouteListStatus.Closed;

		public bool CanChangeIsFixPrice =>
			CanEditFixedPrice && Entity.Status != RouteListStatus.Closed;

		#region EEVM

		public IEntityEntryViewModel CarViewModel { get; }

		public IEntityEntryViewModel CreateCarViewModel()
		{
			return new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.UseViewModelDialog<CarViewModel>()
				.Finish();
		}

		public IEntityEntryViewModel DriverViewModel { get; }

		public IEntityEntryViewModel CreateDriverViewModel()
		{
			return new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
					filter.RestrictCategory = EmployeeCategory.driver;
					filter.CanChangeStatus = false;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
		}

		public IEntityEntryViewModel ForwarderViewModel { get; }

		public IEntityEntryViewModel CreateForwarderViewModel()
		{
			return new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Forwarder)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
					filter.RestrictCategory = EmployeeCategory.forwarder;
					filter.CanChangeStatus = false;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
		}

		public IEntityEntryViewModel LogisticianViewModel { get; }

		public IInteractiveService InteractiveService => _interactiveService;

		public IEntityEntryViewModel CreateLogisticianViewModel()
		{
			 var logisticianViewModel = new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Logistician)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Status = EmployeeStatus.IsWorking;
					filter.RestrictCategory = EmployeeCategory.office;
					filter.CanChangeStatus = false;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			logisticianViewModel.IsEditable = false;

			return logisticianViewModel;
		}

		#endregion EEVM

		public void ObservableGeographicGroups_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(ClosingSubdivisionName));
		}

		private void OnRouteListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.AdditionalLoadingDocument)
				|| e.PropertyName == nameof(Entity.Car)
				|| e.PropertyName == nameof(Entity.Date)
				|| e.PropertyName == nameof(Entity.Status))
			{
				OnPropertyChanged(nameof(CanAddAdditionalLoad));
				OnPropertyChanged(nameof(CanRemoveAdditionalLoad));
				OnPropertyChanged(nameof(AdditionalLoadItemsVisible));
			}

			if(e.PropertyName == nameof(Entity.Date))
			{
				OnPropertyChanged(nameof(CanChangeForwarder));
			}

			if(e.PropertyName == nameof(Entity.Car)
				|| e.PropertyName == nameof(Entity.Status))
			{
				OnPropertyChanged(nameof(CanChangeDriver));
				OnPropertyChanged(nameof(CanChangeForwarder));
			}

			if(e.PropertyName == nameof(Entity.Status))
			{
				OnPropertyChanged(nameof(CanAccept));
				OnPropertyChanged(nameof(CanRevertToNew));
				OnPropertyChanged(nameof(CanChangeFixedPrice));
				OnPropertyChanged(nameof(CanChangeIsFixPrice));
				OnPropertyChanged(nameof(CanRemoveAdditionalLoad));
			}
		}

		private void AddAdditionalLoad()
		{
			var document = _additionalLoadingModel.CreateAdditionLoadingDocument(UoW, Entity);
			if(document != null)
			{
				Entity.AdditionalLoadingDocument = document;
			}
		}

		private void RemoveAdditionalLoad()
		{
			UoW.Delete(Entity.AdditionalLoadingDocument);
			Entity.AdditionalLoadingDocument = null;
		}

		public void OnDatepickerDateDateChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Date < DateTime.Today.AddDays(-1) && !CanСreateRoutelistInPastPeriod)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нельзя выставлять дату ранее вчерашнего дня!");
				Entity.Date = _previousSelectedDate;
			}
			else
			{
				_additionalLoadingModel.ReloadActiveFlyers(UoW, Entity, _previousSelectedDate);
				_previousSelectedDate = Entity.Date;
			}
		}

		public void OnCarChangedByUser(object sender, EventArgs e)
		{
			var isCompanyCar = Entity.GetCarVersion?.IsCompanyCar ?? false;

			Entity.Driver = Entity.Car?.Driver != null
				&& Entity.Car?.Driver.Status != EmployeeStatus.IsFired
					? Entity.Car?.Driver
					: null;

			DriverViewModel.IsEditable = Entity.Driver == null || isCompanyCar;

			if(!isCompanyCar
				|| Entity.Car?.CarModel.CarTypeOfUse == CarTypeOfUse.Largus
				&& Entity.CanAddForwarder)
			{
				Entity.Forwarder = Entity.Forwarder;
			}
			else
			{
				Entity.Forwarder = null;
			}
		}

		private void PrintSelectedDocument(RouteListPrintableDocuments choise)
		{
			var page = NavigationManager.OpenViewModel<DocumentsPrinterViewModel>(this);

			page.ViewModel.ConfigureForRouteListDocumentsPrint(UoW, Entity, choise);

			page.ViewModel.DocumentsPrinted += RaiseDocumentPrinted;
		}

		private void RaiseDocumentPrinted(object sender, EventArgs e)
		{
			DocumentPrinted?.Invoke(sender, e);
		}

		protected override bool BeforeSave()
		{
			_logger.LogInformation("Вызван метод сохранения МЛ {RouteListId}...", Entity.Id);

			if(!Entity.IsDriversDebtInPermittedRangeVerification())
			{
				return false;
			}

			var contextItems = new Dictionary<object, object>
			{
				{nameof(IRouteListItemRepository), _routeListItemRepository}
			};

			var context = new ValidationContext(Entity, null, contextItems);
			var validator = new ObjectValidator(_validationViewFactory);

			if(!validator.Validate(Entity, context))
			{
				return false;
			}

			if(Entity.AdditionalLoadingDocument != null && !Entity.AdditionalLoadingDocument.Items.Any())
			{
				UoW.Delete(Entity.AdditionalLoadingDocument);
				Entity.AdditionalLoadingDocument = null;
			}

			if(_oldDriver != Entity.Driver)
			{
				if(_oldDriver != null)
				{
					var selfDriverTerminalTransferDocument = _routeListRepository.GetSelfDriverTerminalTransferDocument(UoW, _oldDriver, Entity);

					if(selfDriverTerminalTransferDocument != null)
					{
						UoW.Delete(selfDriverTerminalTransferDocument);
					}
				}

				_oldDriver = Entity.Driver;
			}

			UoW.Session.Flush();

			_logger.LogDebug("Пересчитываем рентабельность МЛ");
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			_logger.LogDebug("Закончили пересчет рентабельности МЛ");
			UoW.Save(Entity.RouteListProfitability);

			_logger.LogInformation("Сохраняем маршрутный лист {RouteListId}...", Entity.Id);
			UoWGeneric.Save();
			_logger.LogInformation("Ok");
			return true;
		}

		public bool CanClose()
		{
			if(!_canClose)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь завершения работы задачи и повторите");
			}

			return _canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			_canClose = isSensetive;
		}

		private void ReturnToNewHandler()
		{
			if(!Save())
			{
				return;
			}

			using(var transaction = UoW.Session.BeginTransaction())
			{
				try
				{
					Result result = _routeListService.TryChangeStatusToNew(UoW, Entity);

					SetSensetivity(false);

					result.Match(() =>
					{
						transaction.Commit();
						GlobalUowEventsTracker.OnPostCommit((IUnitOfWorkTracked)UoW);
					},
					ShowErrors);

					SetSensetivity(true);
				}
				catch(Exception ex)
				{
					if(!transaction.WasCommitted
					   && !transaction.WasRolledBack
					   && transaction.IsActive
					   && UoW.Session.Connection.State == ConnectionState.Open)
					{
						try
						{
							transaction.Rollback();
						}
						catch { }
					}

					transaction.Dispose();

					_logger.LogError(ex, "Произошла ошибка во время возвращения Маршрутного листа в статус нового {RouteListId}: {Message}.", Entity.Id, ex.Message);

					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						$"Возникла ошибка при возвращения Маршрутного листа в статус нового {Entity.Id}, МЛ был сохранён, но не возвращен в статус нового.\n" +
						$"Будет произведена попытка переоткрытия вкладки.\n" +
						$"Ошибка: {ex.Message}\n{ex.StackTrace}");

					Close(false, CloseSource.Self);

					NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(Entity.Id));
				}
			}
		}

		private void AcceptHandler()
		{
			if(!Save())
			{
				return;
			}

			var beforeAcceptValidation = _routeListService.ValidateForAccept(Entity);

			bool skipOverfillValidation = false;

			var overfillErrorsCodes = Errors.Logistics.RouteList.OverfilledErrorCodes;

			var overfillErrorsMessages = beforeAcceptValidation.Errors.Select(x => x.Message).ToArray();

			if(beforeAcceptValidation.IsFailure)
			{
				if(!beforeAcceptValidation.Errors.All(error => overfillErrorsCodes.Contains(error.Code))
					|| !_currentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.RouteList.CanConfirmOverweighted)
					|| !_interactiveService.Question(
						"Вы уверены что хотите подтвердить маршрутный лист?\n" +
						string.Join("\n", overfillErrorsMessages),
						"Требуется подтверждение!"))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Error,
						 "Маршрутный лист не был переведен в путь: \n" +
						 string.Join("\n", overfillErrorsMessages));

					return;
				}

				skipOverfillValidation = true;
			}

			var confirmRecalculateRoute = false;

			if((!Entity.PrintsHistory?.Any() ?? true) || _interactiveService.Question(
				"Этот маршрутный лист уже был когда-то напечатан. При новом построении маршрута порядок адресов может быть другой. При продолжении обязательно перепечатайте этот МЛ.\nПерестроить маршрут?", "Перестроить маршрут?"))
			{
				confirmRecalculateRoute = true;
			}

			var confirmSendOnClosing = false;

			if(Entity.GetCarVersion.IsCompanyCar
				&& Entity.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck
				&& !Entity.NeedToLoad
				&& _interactiveService.Question(
					$"Маршрутный лист для транспортировки на склад, перевести машрутный лист сразу в статус '{RouteListStatus.OnClosing.GetEnumDisplayName()}'?"))
			{
				confirmSendOnClosing = true;
			}

			var confirmSenEnRoute = false;

			var needTerminal = Entity.Addresses.Any(x => x.Order.PaymentType == PaymentType.Terminal);

			if(!Entity.NeedToLoad
				&& !needTerminal
				&& _interactiveService.Question($"Для маршрутного листа нет необходимости грузится на складе. Перевести маршрутный лист сразу в статус '{RouteListStatus.EnRoute.GetEnumDisplayName()}'?"))
			{
				confirmSenEnRoute = true;
			}

			using(var transaction = UoW.Session.BeginTransaction())
			{
				try
				{
					Result<IEnumerable<string>> result = _routeListService.TryChangeStatusToAccepted(
						UoW,
						Entity,
						DisableItemsUpdateDelegate,
						_validator,
						skipOverfillValidation,
						confirmRecalculateRoute,
						confirmSendOnClosing,
						confirmSenEnRoute);

					SetSensetivity(false);

					result.Match(() =>
					{
						transaction.Commit();
						GlobalUowEventsTracker.OnPostCommit((IUnitOfWorkTracked)UoW);

						if(result.Value.Any())
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, string.Join("\n", result.Value));
						}
					},
					ShowErrors);

					SetSensetivity(true);
				}
				catch(Exception ex)
				{
					if(!transaction.WasCommitted
					   && !transaction.WasRolledBack
					   && transaction.IsActive
					   && UoW.Session.Connection.State == ConnectionState.Open)
					{
						try
						{
							transaction.Rollback();
						}
						catch { }
					}

					transaction.Dispose();

					_logger.LogError(ex, "Произошла ошибка во время подтверждения МЛ {RouteListId}: {Message}.", Entity.Id, ex.Message);

					_interactiveService.ShowMessage(ImportanceLevel.Warning,
						$"Возникла ошибка при подтверждении МЛ {Entity.Id}, МЛ был сохранён, но не подтверждён.\n" +
						$"Будет произведена попытка переоткрытия вкладки.\n" +
						$"Ошибка: {ex.Message}\n{ex.StackTrace}");

					Close(false, CloseSource.Self);

					NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(Entity.Id));
				}
			}
		}

		protected override bool BeforeValidation()
		{
			var contextItemsEnroute = new Dictionary<object, object>
			{
				{ "NewStatus", RouteListStatus.EnRoute },
				{ nameof(IRouteListItemRepository), _routeListItemRepository }
			};

			ValidationContext = new ValidationContext(Entity, null, contextItemsEnroute);

			return base.BeforeValidation();
		}

		private void ShowErrors(IEnumerable<Error> errors)
		{
			var errorsStrings = errors.Select(x => $"{x.Message} : {x.Code}").ToArray();

			_interactiveService.ShowMessage(ImportanceLevel.Error, string.Join("\n", errorsStrings));
		}

		private void ShowPrintTime()
		{
			var history = _routeListRepository.GetPrintsHistory(UoW, Entity);
			if(history?.Any() ?? false)
			{
				var message = "<b>№\t| Дата и время печати\t| Тип документа</b>";
				for(var i = 0; i < history.Count; i++)
				{
					var item = history[i];
					message += $"\n{i + 1}\t| {item.PrintingTime.ToShortDateString()}" +
							   $" {item.PrintingTime.ToShortTimeString()}\t\t| {item.DocumentType.GetEnumDisplayName()}";
				}
				_interactiveService.ShowMessage(ImportanceLevel.Info, message, $"История печати МЛ №: {Entity.Id}");
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "МЛ не печатался ранее");
			}
		}
	}
}
