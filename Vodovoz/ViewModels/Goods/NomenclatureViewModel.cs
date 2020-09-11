using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.ViewModels.Goods
{
	public class NomenclatureViewModel : EntityTabViewModelBase<Nomenclature>
	{
		private readonly IEmployeeService employeeService;
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		private Warehouse selectedWarehouse;
		public Warehouse SelectedWarehouse {
			get => selectedWarehouse;
			set => SetField(ref selectedWarehouse, value);
		}
		
		public NomenclatureViewModel(IEntityUoWBuilder uowBuilder,
		                             IUnitOfWorkFactory uowFactory,
		                             ICommonServices commonServices,
									 IEmployeeService employeeService,
		                             IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
		                             IEntityAutocompleteSelectorFactory counterpartySelectorFactory) : base(uowBuilder, uowFactory, commonServices) {

			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			
			ConfigureEntityPropertyChanges();
		}
		
		public bool VisibilityWaterCategoryItems =>
			Entity.Category == NomenclatureCategory.water && !Entity.IsDisposableTare; 
		
		public bool VisibilitySalesCategoriesItems =>
			Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category);
		
		public bool VisibilityMasterCategoryItems =>
			Entity.Category == NomenclatureCategory.master;
		
		public bool VisibilityDepositCategoryItems =>
			Entity.Category == NomenclatureCategory.deposit;

		public bool VisibilityAdditionalCategoryItems =>
			Entity.Category == NomenclatureCategory.additional;

		public bool VisibilityFuelCategoryItems =>
			Entity.Category == NomenclatureCategory.fuel;

		public bool SensitivityEquipmentCategoryItems =>
			Entity.Category == NomenclatureCategory.equipment;

		public bool SensitivityNotServiceOrDepositCategoryItems =>
			!(Entity.Category == NomenclatureCategory.service || Entity.Category == NomenclatureCategory.deposit);
		
		public bool IsEshopNomenclature => Entity?.ProductGroup?.ExportToOnlineStore ?? false;

		public bool SensitivityRadioPriceButton => Entity.DependsOnNomenclature == null;

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.Category,
				() => VisibilityWaterCategoryItems,
				() => VisibilitySalesCategoriesItems,
				() => VisibilityMasterCategoryItems,
				() => VisibilityDepositCategoryItems,
				() => VisibilityAdditionalCategoryItems,
				() => VisibilityFuelCategoryItems,
				() => SensitivityEquipmentCategoryItems,
				() => SensitivityNotServiceOrDepositCategoryItems
			);
			
			SetPropertyChangeRelation(
				e => e.IsDisposableTare,
				() => VisibilityWaterCategoryItems
			);
			
			SetPropertyChangeRelation(
				e => e.ProductGroup,
				() => IsEshopNomenclature
			);

			SetPropertyChangeRelation(
				e => e.DependsOnNomenclature,
				() => SensitivityRadioPriceButton
			);
		}

		public string GetUserEmployeeName()
		{
			if(Entity.CreatedBy == null) {
				return "";
			}

			var employee = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);

			if(employee == null) {
				return Entity.CreatedBy.Name;
			} else {
				return employee.ShortName;
			}
		}

		#region Commands

		private DelegateCommand removeWarehouseCommand = null;

		public DelegateCommand RemoveWarehouseCommand =>
			removeWarehouseCommand ?? (removeWarehouseCommand = new DelegateCommand(
					() => {
						Entity.RemoveWarehouse(SelectedWarehouse);
					},
					() => SelectedWarehouse != null
				)
			);

		#endregion
	}
}
