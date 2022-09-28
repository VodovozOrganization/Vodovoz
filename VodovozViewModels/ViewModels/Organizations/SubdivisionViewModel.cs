using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Permissions;

namespace Vodovoz.ViewModels.ViewModels.Organizations
{
	public class SubdivisionViewModel : EntityTabViewModelBase<Subdivision>
	{

		public event Action OnSavedEntity;

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory;

		private PresetSubdivisionPermissionsViewModel presetSubdivisionPermissionVM;
		public PresetSubdivisionPermissionsViewModel PresetSubdivisionPermissionVM {
			get { return presetSubdivisionPermissionVM; }
			set {
				if(value != presetSubdivisionPermissionVM && presetSubdivisionPermissionVM != null) {
					OnSavedEntity -= presetSubdivisionPermissionVM.SaveCommand.Execute;
					if(value != null)
						OnSavedEntity += value.SaveCommand.Execute;
				}
				SetField(ref presetSubdivisionPermissionVM, value); 
			}
		}

        private WarehousePermissionsViewModel warehousePermissionsVm;
        public WarehousePermissionsViewModel WarehousePermissionsVM
        {
            get => warehousePermissionsVm;
            set
            {
	            SetField(ref warehousePermissionsVm, value);
            }
        }

        public SubdivisionViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IPermissionRepository permissionRepository,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ISubdivisionRepository subdivisionRepository
		) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			SubdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			PresetSubdivisionPermissionVM = new PresetSubdivisionPermissionsViewModel(UoW, permissionRepository, Entity);
			var _warehousePermissionModel = new SubdivisionWarehousePermissionModel(UoW, Entity);
			WarehousePermissionsVM = new WarehousePermissionsViewModel(UoW, _warehousePermissionModel);
			WarehousePermissionsVM.CanEdit = PermissionResult.CanUpdate;
			EmployeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			SalesPlanSelectorFactory = (salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory)))
				.CreateSalesPlanAutocompleteSelectorFactory(nomenclatureSelectorFactory);
			ConfigureEntityChangingRelations();
			CreateCommands();
		}
		
		public ISubdivisionRepository SubdivisionRepository { get; }

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.GeographicGroup,
				() => GeographicGroup
			);

			SetPropertyChangeRelation(e => e.ParentSubdivision,
				() => GeographicGroupVisible
			);

			Entity.ObservableChildSubdivisions.ElementAdded += (aList, aIdx) => OnPropertyChanged(() => GeographicGroupVisible);
			Entity.ObservableChildSubdivisions.ElementRemoved += (aList, aIdx, aObject) => OnPropertyChanged(() => GeographicGroupVisible);
		}

		public override bool HasChanges => true;

		public override bool Save(bool close)
		{
			bool res = base.Save(close);
			OnSavedEntity?.Invoke();
			UoW.Commit();
			return res;
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool GeographicGroupVisible => Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();

		public virtual GeoGroup GeographicGroup {
			get => Entity.GeographicGroup;
			set {
				if(Entity.GeographicGroup == value) {
					return;
				}
				Entity.GeographicGroup = value;
				Entity.SetChildsGeographicGroup(Entity.GeographicGroup);
			}
		}

		public IEntityAutocompleteSelectorFactory SalesPlanSelectorFactory { get; }

		#region Commands

		private void CreateCommands()
		{
			CreateAddDocumentTypeCommand();
			CreateDeleteDocumentCommand();
		}

		#region AddDocumentTypeCommand

		public DelegateCommand<TypeOfEntity> AddDocumentTypeCommand { get; private set; }

		private void CreateAddDocumentTypeCommand()
		{
			AddDocumentTypeCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.AddDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion AddDocumentTypeCommand


		#region DeleteDocumentCommand

		public DelegateCommand<TypeOfEntity> DeleteDocumentCommand { get; private set; }

		private void CreateDeleteDocumentCommand()
		{
			DeleteDocumentCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.DeleteDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion DeleteDocumentCommand

		#endregion Commands
	}
}
