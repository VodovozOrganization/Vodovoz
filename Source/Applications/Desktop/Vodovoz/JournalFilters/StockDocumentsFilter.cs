using Autofac;
using Autofac.Core.Lifetime;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz
{
	[OrmDefaultIsFiltered(true)]
	public partial class StockDocumentsFilter : RepresentationFilterBase<StockDocumentsFilter>, ISingleUoWDialog, INotifyPropertyChanged
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private Warehouse _warehouse;
		private Employee _driver;
		private Warehouse _restrictWarehouse;
		private Employee _restrictDriver;
		private INavigationManager _navigationManager;

		public event PropertyChangedEventHandler PropertyChanged;

		protected override void ConfigureWithUow()
		{
			_navigationManager = _lifetimeScope.Resolve<INavigationManager>();

			enumcomboDocumentType.ItemsEnum = typeof(DocumentType);
			enumcomboDocumentType.HiddenItems = new[] { DocumentType.DeliveryDocument as object };

			WarehouseViewModel = new LegacyEEVMBuilderFactory<StockDocumentsFilter>(TdiTab, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseJournalViewModel>()
				.Finish();

			DriverViewModel = new LegacyEEVMBuilderFactory<StockDocumentsFilter>(TdiTab, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.RestrictCategory = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			   && !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin)
			{
				WarehouseViewModel.IsEditable = false;
			}

			if(CurrentUserSettings.Settings.DefaultWarehouse != null)
			{
				WarehouseViewModel.Entity = UoW.GetById<Warehouse>(CurrentUserSettings.Settings.DefaultWarehouse.Id);
			}

			entityentryDriver.ViewModel = DriverViewModel;
			entityentryWarehouse.ViewModel = WarehouseViewModel;

			dateperiodDocs.StartDate = DateTime.Today.AddDays(-7);
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);

			comboMovementStatus.ItemsEnum = typeof(MovementDocumentStatus);
		}

		public StockDocumentsFilter(IUnitOfWork uow, ITdiTab tdiTab) : this()
		{
			TdiTab = tdiTab;
			UoW = uow;
		}

		public StockDocumentsFilter()
		{
			this.Build();
		}

		public ITdiTab TdiTab { get; set; }

		public DocumentType? RestrictDocumentType
		{
			get => enumcomboDocumentType.SelectedItem as DocumentType?;
			set
			{
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		}

		public MovementDocumentStatus? RestrictMovementStatus
		{
			get => comboMovementStatus.SelectedItem as MovementDocumentStatus?;
			set
			{
				comboMovementStatus.SelectedItem = value;
				comboMovementStatus.Sensitive = false;
			}
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(_warehouse != value)
				{
					_warehouse = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Warehouse)));
					OnRefiltered();
				}
			}
		}

		public Warehouse RestrictWarehouse
		{
			get => _restrictWarehouse;
			set
			{
				if(_restrictWarehouse != value)
				{
					_restrictWarehouse = value;

					if(value != null)
					{
						Warehouse = value;
					}

					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RestrictWarehouse)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanChangeWarehouse)));
				}
			}
		}

		public bool CanChangeWarehouse => RestrictWarehouse is null;

		public Employee Driver
		{
			get => _driver;
			set
			{
				if(_driver != value)
				{
					_driver = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Driver)));
					OnRefiltered();
				}
			}
		}

		public Employee RestrictDriver
		{
			get => _restrictDriver;
			set
			{
				if(_restrictDriver != value)
				{
					_restrictDriver = value;

					if(value != null)
					{
						Driver = value;
					}

					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RestrictDriver)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanChangeDriver)));
				}
			}
		}

		public bool CanChangeDriver => RestrictDriver is null;


		public DateTime? RestrictStartDate
		{
			get => dateperiodDocs.StartDateOrNull;
			set
			{
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate
		{
			get => dateperiodDocs.EndDateOrNull;
			set
			{
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public IEntityEntryViewModel WarehouseViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }

		protected void OnEnumcomboDocumentTypeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
			ylabelMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
			comboMovementStatus.Visible = RestrictDocumentType == DocumentType.MovementDocument;
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnComboMovementStatusChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected override void OnDestroyed()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.OnDestroyed();
		}
	}
}
