using Autofac;
using Microsoft.Extensions.Logging;
using QS.Attachments.ViewModels.Widgets;
using QS.Commands;
using QS.Dialog.ViewModels;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using System.Threading;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Factories;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarViewModel : EntityTabViewModelBase<Car>
	{
		private readonly IRouteListsWageController _routeListsWageController;
		private readonly ILogger<CarViewModel> _logger;
		private bool _canChangeBottlesFromAddress;
		private DelegateCommand _addGeoGroupCommand;

		private IPage<GeoGroupJournalViewModel> _gooGroupPage = null;

		private AttachmentsViewModel _attachmentsViewModel;
		private string _driverInfoText;

		public CarViewModel(
			ILogger<CarViewModel> logger,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IAttachmentsViewModelFactory attachmentsViewModelFactory,
			ICarVersionsViewModelFactory carVersionsViewModelFactory,
			IOdometerReadingsViewModelFactory odometerReadingsViewModelFactory,
			IRouteListsWageController routeListsWageController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListsWageController = routeListsWageController ?? throw new ArgumentNullException(nameof(routeListsWageController));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			TabName = "Автомобиль";

			AttachmentsViewModel = attachmentsViewModelFactory.CreateNewAttachmentsViewModel(Entity.ObservableAttachments);
			CarVersionsViewModel = (carVersionsViewModelFactory ?? throw new ArgumentNullException(nameof(carVersionsViewModelFactory)))
				.CreateCarVersionsViewModel(Entity);
			OdometerReadingsViewModel = (odometerReadingsViewModelFactory ?? throw new ArgumentNullException(nameof(odometerReadingsViewModelFactory)))
				.CreateOdometerReadingsViewModel(Entity);

			CanChangeBottlesFromAddress = commonServices.PermissionService.ValidateUserPresetPermission(
				Vodovoz.Permissions.Logistic.Car.CanChangeCarsBottlesFromAddress,
				commonServices.UserService.CurrentUserId);

			CanEditCarModel = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(CarModel)).CanUpdate;
			CanChangeCarModel = Entity.Id == 0 || commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.Car.CanChangeCarModel);
			CanEditFuelCardNumber = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.Car.CanChangeFuelCardNumber);

			CarModelViewModel = new CommonEEVMBuilderFactory<Car>(this, Entity, UoW, NavigationManager, LifetimeScope)
				.ForProperty(x => x.CarModel)
				.UseViewModelJournalAndAutocompleter<CarModelJournalViewModel>()
				.UseViewModelDialog<CarModelViewModel>()
				.Finish();

			DriverViewModel = new CommonEEVMBuilderFactory<Car>(this, Entity, UoW, NavigationManager, LifetimeScope)
			.ForProperty(x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			FuelTypeViewModel = new CommonEEVMBuilderFactory<Car>(this, Entity, UoW, NavigationManager, LifetimeScope)
				.ForProperty(x => x.FuelType)
				.UseViewModelJournalAndAutocompleter<FuelTypeJournalViewModel>()
				.UseViewModelDialog<FuelTypeViewModel>()
				.Finish();

			Entity.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(Entity.Driver) && Entity.Driver != null)
				{
					OnDriverChanged();
				}
			};
			OnDriverChanged();
		}

		public string DriverInfoText
		{
			get => _driverInfoText;
			set => SetField(ref _driverInfoText, value);
		}

		public bool CanChangeBottlesFromAddress
		{
			get => _canChangeBottlesFromAddress;
			set => SetField(ref _canChangeBottlesFromAddress, value);
		}

		public AttachmentsViewModel AttachmentsViewModel
		{
			get => _attachmentsViewModel;
			set => SetField(ref _attachmentsViewModel, value);
		}

		public bool CanEditCarModel { get; }
		public bool CanChangeCarModel { get; }
		public bool CanEditFuelCardNumber { get; }

		public IEntityEntryViewModel CarModelViewModel { get; }
		public IEntityEntryViewModel DriverViewModel { get; }
		public IEntityEntryViewModel FuelTypeViewModel { get; }

		public ILifetimeScope LifetimeScope { get; }
		public CarVersionsViewModel CarVersionsViewModel { get; }
		public OdometerReadingsViewModel OdometerReadingsViewModel { get; }

		public override bool Save(bool close)
		{
			var routeLists = CarVersionsViewModel.GetAllAffectedRouteLists(UoW);
			if(!routeLists.Any())
			{
				return base.Save(close);
			}

			if(!CommonServices.InteractiveService.Question($"Будет пересчитана зарплата в {routeLists.Count} МЛ. Продолжить?"))
			{
				return false;
			}

			_logger.LogInformation("Запущен пересчёт зарплаты в МЛ");

			IPage<ProgressWindowViewModel> progressWindow = null;
			var cts = new CancellationTokenSource();
			bool isProgressWindowClosed = false;

			void OnProgressWindowClosed(object sender, PageClosedEventArgs args)
			{
				cts.Cancel();
				isProgressWindowClosed = true;
			}

			try
			{
				progressWindow = NavigationManager.OpenViewModel<ProgressWindowViewModel>(this);
				progressWindow.PageClosed += OnProgressWindowClosed;
				var progressBarDisplayable = progressWindow.ViewModel.Progress;

				_routeListsWageController.ProgressBarDisplayable = progressBarDisplayable;
				_routeListsWageController.RecalculateRouteListsWage(UoW, routeLists, cts.Token);
				_logger.LogInformation("Пересчёт зарплаты в МЛ завершён");

				progressBarDisplayable.Update("Сохранение...");

				foreach(var routeList in routeLists)
				{
					UoW.Save(routeList);
				}

				return base.Save(close);
			}
			catch(OperationCanceledException)
			{
				_logger.LogDebug("Пересчёт зарплаты в МЛ был отменён");
				return false;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Пересчёт зарплаты в МЛ был отменён: {ExceptionMessage}", e.Message);
				CommonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Error, $"Пересчёт зарплаты в МЛ был отменён: {e.Message}");
				return false;
			}
			finally
			{
				if(progressWindow != null)
				{
					progressWindow.PageClosed -= OnProgressWindowClosed;
					if(!isProgressWindowClosed)
					{
						NavigationManager.ForceClosePage(progressWindow);
					}
				}
			}
		}

		private void OnDriverChanged()
		{
			if(Entity.Driver == null)
			{
				return;
			}

			var docs = Entity.Driver.GetMainDocuments();
			if(docs.Any())
			{
				DriverInfoText =
					$"\tПаспорт: {docs.First().PassportSeria} № {docs.First().PassportNumber}\n" +
					$"\tАдрес регистрации: {Entity.Driver.AddressRegistration}";
			}
			else
			{
				DriverInfoText = "Главный документ отсутствует";
			}
		}

		#region Add GeoGroup

		public DelegateCommand AddGeoGroupCommand
		{
			get
			{
				if(_addGeoGroupCommand == null)
				{
					_addGeoGroupCommand = new DelegateCommand(AddGeoGroup);
				}

				return _addGeoGroupCommand;
			}
		}

		private void AddGeoGroup()
		{
			if(_gooGroupPage != null)
			{
				NavigationManager.SwitchOn(_gooGroupPage);
				return;
			}

			_gooGroupPage = NavigationManager.OpenViewModel<GeoGroupJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel => 
				{
					viewModel.SelectionMode = JournalSelectionMode.Multiple;
					viewModel.DisableChangeEntityActions();
				});

			_gooGroupPage.ViewModel.OnSelectResult += OnJournalGeoGroupsSelectedResult;
			_gooGroupPage.PageClosed += OnGeoGroupPagePageClosed;
		}

		private void OnGeoGroupPagePageClosed(object sender, PageClosedEventArgs e)
		{
			_gooGroupPage.PageClosed -= OnGeoGroupPagePageClosed;
			_gooGroupPage.ViewModel.OnSelectResult -= OnJournalGeoGroupsSelectedResult;

			_gooGroupPage = null;
		}

		private void OnJournalGeoGroupsSelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selected = e.SelectedObjects.Cast<GeoGroupJournalNode>();
			if(!selected.Any())
			{
				return;
			}
			foreach(var item in selected)
			{
				if(!Entity.ObservableGeographicGroups.Any(x => x.Id == item.Id))
				{
					var group = UoW.GetById<GeoGroup>(item.Id);
					Entity.ObservableGeographicGroups.Add(group);
				}
			}
		}

		#endregion Add GeoGroup
	}
}
