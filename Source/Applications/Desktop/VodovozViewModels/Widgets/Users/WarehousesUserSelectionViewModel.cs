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
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Widgets.Users
{
	public partial class WarehousesUserSelectionViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;

		private WarehouseNode _selectedWarehouse;
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

			ObservableWarehouses = new GenericObservableList<WarehouseNode>(GetWarehousesNodesByIds(warehousesIds));
		}

		#region Свойства

		public virtual GenericObservableList<WarehouseNode> ObservableWarehouses { get; set; }

		[PropertyChangedAlso(nameof(CanRemoveWarehouse))]
		public WarehouseNode SelectedWarehouse
		{
			get => _selectedWarehouse;
			set => SetField(ref _selectedWarehouse, value);
		}

		#endregion

		private List<WarehouseNode> GetWarehousesNodesByIds(IEnumerable<int> warehousesIds)
		{
			var warehouses = _unitOfWork.GetAll<Warehouse>()
				.Where(w => warehousesIds.Contains(w.Id))
				.Select(w => new WarehouseNode { WarehouseId = w.Id, WarehouseName = w.Name })
				.ToList();

			return warehouses;
		}

		private bool IsWarehouseAlreadyAdded(int warehouseId) =>
			ObservableWarehouses
			.Where(w => w.WarehouseId == warehouseId)
			.Any();

		#region Commands

		#region Add warehouse
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
				if(e.SelectedNodes.FirstOrDefault() is JournalEntityNodeBase entityNode)
				{
					if(entityNode.EntityType != typeof(Warehouse))
					{
						return;
					}

					var addWarehouseId = entityNode.Id;
					var addWarehouseName = entityNode.Title;

					bool isWarehouseInList = IsWarehouseAlreadyAdded(addWarehouseId);

					if(isWarehouseInList)
					{
						_commonServices.InteractiveService.ShowMessage(
							QS.Dialog.ImportanceLevel.Info,
							$"Выбранный склад \"{addWarehouseName}\" уже добавлен в список");

						return;
					}

					var newWarehouseNode = new WarehouseNode
					{
						WarehouseId = addWarehouseId,
						WarehouseName = addWarehouseName
					};

					ObservableWarehouses.Add(newWarehouseNode);
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
