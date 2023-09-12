using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Widgets.Users
{
	public class WarehousesUserSelectionViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private Warehouse _selectedWarehouse;
		private DelegateCommand _addWarehouseCommand;
		private DelegateCommand _removeWarehouseCommand;

		public WarehousesUserSelectionViewModel(
			IUnitOfWork unitOfWork,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEnumerable<int> warehousesIds)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			ObservableWarehouses = new GenericObservableList<Warehouse>(GetWarehousesByIds(warehousesIds));
		}

		#region Свойства

		public virtual GenericObservableList<Warehouse> ObservableWarehouses { get; set; }

		[PropertyChangedAlso(nameof(CanRemoveWarehouse))]
		public Warehouse SelectedWarehouse
		{
			get => _selectedWarehouse;
			set => SetField(ref _selectedWarehouse, value);
		}

		#endregion

		private List<Warehouse> GetWarehousesByIds(IEnumerable<int> warehousesIds)
		{
			var warehouses = _unitOfWork.GetAll<Warehouse>()
				.Where(w => warehousesIds.Contains(w.Id))
				.ToList();

			return warehouses;
		}

		#region Commands

		#region Add waarehouse
		public DelegateCommand AddWarehouseCommand
		{
			get
			{
				if(_addWarehouseCommand == null)
				{
					_addWarehouseCommand = new DelegateCommand(AddWarehouse, () => CanAddWarehouse);
					_addWarehouseCommand.CanExecuteChangedWith(this, x => x.CanAddWarehouse);
				}
				return _addWarehouseCommand;
			}
		}

		public bool CanAddWarehouse => true;

		private void AddWarehouse()
		{
			var warehouseJournalVM = _navigationManager.OpenViewModel<WarehouseJournalViewModel>(null).ViewModel;

			warehouseJournalVM.SelectionMode = JournalSelectionMode.Single;
			warehouseJournalVM.OnEntitySelectedResult += (s, e) =>
			{
				if(s is Warehouse warehouseToAdd)
				{
					ObservableWarehouses.Add(warehouseToAdd);
				}
			};
		}
		#endregion Add warehouse

		#region Remove warehouse

		public DelegateCommand RemoveWarehouseCommand
		{
			get
			{
				if(_removeWarehouseCommand == null)
				{
					_removeWarehouseCommand = new DelegateCommand(RemoveWarehouse, () => CanRemoveWarehouse);
					_removeWarehouseCommand.CanExecuteChangedWith(this, x => x.CanRemoveWarehouse);
				}
				return _removeWarehouseCommand;
			}
		}

		public bool CanRemoveWarehouse => SelectedWarehouse != null;

		private void RemoveWarehouse()
		{
			if(SelectedWarehouse != null && ObservableWarehouses.Contains(SelectedWarehouse))
			{
				ObservableWarehouses.Remove(SelectedWarehouse);
			}
		}

		#endregion Remove warehouse

		#endregion
	}
}
