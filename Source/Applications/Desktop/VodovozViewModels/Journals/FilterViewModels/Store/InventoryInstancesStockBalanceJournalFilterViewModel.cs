using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Autofac;
using System;
using QS.Navigation;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using QS.ViewModels.Dialog;
using QS.Project.Journal;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class InventoryInstancesStockBalanceJournalFilterViewModel :
		FilterViewModelBase<InventoryInstancesStockBalanceJournalFilterViewModel>
	{
		private readonly ILifetimeScope _scope;
		private readonly DialogViewModelBase _journalViewModel;
		private Nomenclature _nomenclature;
		private Warehouse _warehouse;
		private Employee _employeeStorage;
		private Car _carStorage;
		private StorageType? _storageType;
		private INavigationManager _navigationManager;

		public InventoryInstancesStockBalanceJournalFilterViewModel(
			ILifetimeScope scope,
			DialogViewModelBase journalViewModel,
			Action<InventoryInstancesStockBalanceJournalFilterViewModel> filterParams = null)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_journalViewModel = journalViewModel ?? throw new ArgumentNullException(nameof(journalViewModel));
			
			ResolveInnerDependencies();
			CreateEntryViewModels();

			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
	 	}

		public bool CanChangeNomenclature { get; private set; } = true;

		public Nomenclature RestrictedNomenclature
		{
			get => _nomenclature;
			set
			{
				Nomenclature = value;
				CanChangeNomenclature = value == null;
			}
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set => UpdateFilterField(ref _warehouse, value);
		}

		public bool CanChangeWarehouseStorage { get; private set; } = true;

		public Warehouse RestrictedWarehouse
		{
			get => _warehouse;
			set
			{
				Warehouse = value;
				CanChangeWarehouseStorage = value == null;
			}
		}

		public Employee EmployeeStorage
		{
			get => _employeeStorage;
			set => UpdateFilterField(ref _employeeStorage, value);
		}

		public bool CanChangeEmployeeStorage { get; private set; } = true;

		public Employee RestrictedEmployeeStorage
		{
			get => _employeeStorage;
			set
			{
				EmployeeStorage = value;
				CanChangeEmployeeStorage = value == null;
			}
		}

		public Car CarStorage
		{
			get => _carStorage;
			set => UpdateFilterField(ref _carStorage, value);
		}

		public bool CanChangeCarStorage { get; private set; } = true;

		public Car RestrictedCarStorage
		{
			get => _carStorage;
			set
			{
				CarStorage = value;
				CanChangeCarStorage = value == null;
			}
		}

		public StorageType? StorageType
		{
			get => _storageType;
			set
			{
				if(SetField(ref _storageType, value))
				{
					switch(value)
					{
						case Domain.Documents.MovementDocuments.StorageType.Warehouse:
							SetAndRefilterAtOnce(
								x => x.EmployeeStorage = null,
								x => x.CarStorage = null);
							break;
						case Domain.Documents.MovementDocuments.StorageType.Employee:
							SetAndRefilterAtOnce(
								x => x.Warehouse = null,
								x => x.CarStorage = null);
							break;
						case Domain.Documents.MovementDocuments.StorageType.Car:
							SetAndRefilterAtOnce(
								x => x.Warehouse = null,
								x => x.EmployeeStorage = null);
							break;
						default:
							SetAndRefilterAtOnce(
								x => x.Warehouse = null,
								x => x.EmployeeStorage = null,
								x => x.CarStorage = null);
							break;
					}
					
					OnPropertyChanged(nameof(CanShowStorage));
					OnPropertyChanged(nameof(CanShowWarehouseStorage));
					OnPropertyChanged(nameof(CanShowEmployeeStorage));
					OnPropertyChanged(nameof(CanShowCarStorage));
					OnPropertyChanged(nameof(StorageLabel));
				}
			}
		}

		public bool CanChangeStorageType { get; private set; } = true;

		public StorageType? RestrictedStorageType
		{
			get => _storageType;
			set
			{
				StorageType = value;
				CanChangeStorageType = value == null;
			}
		}

		public string InventoryNumber { get; set; }

		public bool CanShowStorage => StorageType != null;
		public bool CanShowWarehouseStorage => StorageType == Domain.Documents.MovementDocuments.StorageType.Warehouse;
		public bool CanShowEmployeeStorage => StorageType == Domain.Documents.MovementDocuments.StorageType.Employee;
		public bool CanShowCarStorage => StorageType == Domain.Documents.MovementDocuments.StorageType.Car;

		public string StorageLabel
		{
			get
			{
				switch(StorageType)
				{
					case Domain.Documents.MovementDocuments.StorageType.Warehouse:
						return "Склад";
					case Domain.Documents.MovementDocuments.StorageType.Employee:
						return "Сотрудник";
					case Domain.Documents.MovementDocuments.StorageType.Car:
						return "Автомобиль";
					default:
						return string.Empty;
				}
			}
		}
		public IEntityEntryViewModel WarehouseStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel CarStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel NomenclatureEntryViewModel { get; private set; }
		public override bool IsShow { get; set; } = true;

		private void ResolveInnerDependencies()
		{
			_navigationManager = _scope.Resolve<INavigationManager>();
		}

		private void CreateEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryInstancesStockBalanceJournalFilterViewModel>(
				_journalViewModel, this, UoW, _navigationManager, _scope);

			WarehouseStorageEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.Finish();

			EmployeeStorageEntryViewModel = builder.ForProperty(x => x.EmployeeStorage)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();

			CarStorageEntryViewModel = builder.ForProperty(x => x.CarStorage)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();

			NomenclatureEntryViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelDialog<NomenclatureViewModel>()
				.UseViewModelJournalAndAutocompleter<InventoryNomenclaturesJournalViewModel>()
				.Finish();
		}
	}
}
