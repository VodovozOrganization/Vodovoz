using System;
using QS.Project.Filter;
using Vodovoz.Domain.Store;
using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureStockFilterViewModel : FilterViewModelBase<NomenclatureStockFilterViewModel>
	{
		private Warehouse _restrictWarehouse;
		private Warehouse _warehouse;
		private Employee _employeeStorage;
		private Car _carStorage;
		private bool? _restrictShowArchive;
		private bool? _restrictShowNomenclatureInstance;
		private bool _showArchive;
		private bool _showNomenclatureInstance;

		public NomenclatureStockFilterViewModel(IEntityAutocompleteSelectorFactory warehouseSelectorFactory)
		{
			WarehouseSelectorFactory = warehouseSelectorFactory ?? throw new ArgumentNullException(nameof(warehouseSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory WarehouseSelectorFactory { get; }
		public IEnumerable<int> ExcludedNomenclatureIds { get; set; }

		public Warehouse RestrictWarehouse
		{
			get => _restrictWarehouse;
			set
			{
				if(SetField(ref _restrictWarehouse, value))
				{
					Warehouse = _restrictWarehouse;
					OnPropertyChanged(nameof(CanChangeWarehouse));
				}
			}
		}

		public bool CanChangeWarehouse => RestrictWarehouse == null;

		public Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				if(SetField(ref _warehouse, value))
				{
					if(value == null)
					{
						return;
					}

					EmployeeStorage = null;
					CarStorage = null;
					Update();
				}
			}
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
						return;
					}

					Warehouse = null;
					CarStorage = null;
					Update();
				}
			}
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

		public bool CanShowInstanceStorages => ShowNomenclatureInstance;
		public bool CanChangeShowNomenclatureInstance => RestrictShowNomenclatureInstance == null;

		public bool? RestrictShowNomenclatureInstance
		{
			get => _restrictShowNomenclatureInstance;
			set
			{
				if(SetField(ref _restrictShowNomenclatureInstance, value) && _restrictShowNomenclatureInstance.HasValue)
				{
					ShowArchive = _restrictShowNomenclatureInstance.Value;
					OnPropertyChanged(nameof(CanChangeShowNomenclatureInstance));
				}
			}
		}

		public bool ShowNomenclatureInstance
		{
			get => _showNomenclatureInstance;
			set
			{
				if(UpdateFilterField(ref _showNomenclatureInstance, value))
				{
					OnPropertyChanged(nameof(CanShowInstanceStorages));
				}
			}
		}
	}
}
