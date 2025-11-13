using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureStockFilterViewModel : FilterViewModelBase<NomenclatureStockFilterViewModel>
	{
		private readonly DialogViewModelBase _journal;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private Warehouse _restrictWarehouse;
		private Warehouse _warehouse;
		private Employee _restrictEmployeeStorage;
		private Employee _employeeStorage;
		private Car _restrictCarStorage;
		private Car _carStorage;
		private bool? _restrictShowArchive;
		private bool? _restrictShowNomenclatureInstance;
		private bool _showArchive;
		private bool _showNomenclatureInstance;
		private bool _canChangeWarehouse = true;
		private bool _canChangeEmployeeStorage = true;
		private bool _canChangeCarStorage = true;
		private bool _canChangeShowNomenclatureInstance = true;

		public NomenclatureStockFilterViewModel(
			DialogViewModelBase journal,
			INavigationManager navigationManager,
			ILifetimeScope scope)
		{
			_journal = journal ?? throw new ArgumentNullException(nameof(journal));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			
			CreateStorageViewModels();
		}

		public IEnumerable<int> ExcludedNomenclatureIds { get; set; }

		public Warehouse RestrictWarehouse
		{
			get => _restrictWarehouse;
			set
			{
				if(SetField(ref _restrictWarehouse, value))
				{
					Warehouse = _restrictWarehouse;
					BlockChangeStorages();
				}
			}
		}

		public bool CanChangeWarehouse
		{
			get => _canChangeWarehouse;
			set => SetField(ref _canChangeWarehouse, value);
		}

		public Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(SetField(ref _warehouse, value))
				{
					if(value == null)
					{
						Update();
						return;
					}

					EmployeeStorage = null;
					CarStorage = null;
					Update();
				}
			}
		}
		
		public Employee RestrictEmployeeStorage
		{
			get => _restrictEmployeeStorage;
			set
			{
				if(SetField(ref _restrictEmployeeStorage, value))
				{
					EmployeeStorage = _restrictEmployeeStorage;
					BlockChangeStorages();
				}
			}
		}
		
		public bool CanChangeEmployeeStorage
		{
			get => _canChangeEmployeeStorage;
			set => SetField(ref _canChangeEmployeeStorage, value);
		}

		public Employee EmployeeStorage
		{
			get => _employeeStorage;
			set
			{
				if(SetField(ref _employeeStorage, value))
				{
					if(value == null)
					{
						Update();
						return;
					}

					Warehouse = null;
					CarStorage = null;
					Update();
				}
			}
		}
		
		public Car RestrictCarStorage
		{
			get => _restrictCarStorage;
			set
			{
				if(SetField(ref _restrictCarStorage, value))
				{
					CarStorage = _restrictCarStorage;
					BlockChangeStorages();
				}
			}
		}

		public bool CanChangeCarStorage
		{
			get => _canChangeCarStorage;
			set => SetField(ref _canChangeCarStorage, value);
		}

		public Car CarStorage
		{
			get => _carStorage;
			set
			{
				if(SetField(ref _carStorage, value))
				{
					if(value == null)
					{
						Update();
						return;
					}

					Warehouse = null;
					EmployeeStorage = null;
					Update();
				}
			}
		}

		public bool? RestrictShowArchive
		{
			get => _restrictShowArchive;
			set
			{
				if(SetField(ref _restrictShowArchive, value) && _restrictShowArchive.HasValue)
				{
					ShowArchive = _restrictShowArchive.Value;
					OnPropertyChanged(nameof(CanChangeShowArchive));
				}
			}
		}

		public bool CanChangeShowArchive => RestrictShowArchive == null;

		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}

		public bool CanChangeShowNomenclatureInstance
		{
			get => _canChangeShowNomenclatureInstance;
			set => SetField(ref _canChangeShowNomenclatureInstance, value);
		}

		public bool? RestrictShowNomenclatureInstance
		{
			get => _restrictShowNomenclatureInstance;
			set
			{
				if(SetField(ref _restrictShowNomenclatureInstance, value) && _restrictShowNomenclatureInstance.HasValue)
				{
					ShowNomenclatureInstance = _restrictShowNomenclatureInstance.Value;
					CanChangeShowNomenclatureInstance = false;
				}
			}
		}

		public bool ShowNomenclatureInstance
		{
			get => _showNomenclatureInstance;
			set => UpdateFilterField(ref _showNomenclatureInstance, value);
		}
		
		public IEntityEntryViewModel WarehouseEntryViewModel { get; private set; }
		public IEntityEntryViewModel EmployeeStorageEntryViewModel { get; private set; }
		public IEntityEntryViewModel CarStorageEntryViewModel { get; private set; }

		private void CreateStorageViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<NomenclatureStockFilterViewModel>(_journal, this, UoW, _navigationManager, _scope);
			
			WarehouseEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelDialog<WarehouseViewModel>()
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.Finish();
			
			EmployeeStorageEntryViewModel = builder.ForProperty(x => x.EmployeeStorage)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					f => f.Status = EmployeeStatus.IsWorking)
				.Finish();
			
			CarStorageEntryViewModel = builder.ForProperty(x => x.CarStorage)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
		}

		private void BlockChangeStorages()
		{
			CanChangeWarehouse = false;
			CanChangeEmployeeStorage = false;
			CanChangeCarStorage = false;
			CanChangeShowNomenclatureInstance = false;
		}
	}
}
