using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.PermissionExtensions;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Warehouses
{
	public class WriteOffDocumentViewModel : EntityTabViewModelBase<WriteoffDocument>
	{
		private bool _canChangeDocumentType;
		private WriteoffDocumentItem _selectedItem;
		private INomenclatureRepository _nomenclatureRepository;
		private readonly ILifetimeScope _scope;
		private readonly CommonMessages _commonMessages;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly StoreDocumentHelper _storeDocumentHelper;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IEntityExtendedPermissionValidator _extendedPermissionValidator;

		private DelegateCommand _printCommand;
		private DelegateCommand _addNomenclatureCommand;
		private DelegateCommand _addOrEditFineCommand;
		private DelegateCommand _addInventoryInstanceCommand;
		private DelegateCommand _deleteItemCommand;
		private DelegateCommand _deleteFineCommand;

		public WriteOffDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IReportViewOpener reportViewOpener,
			ILifetimeScope scope,
			CommonMessages commonMessages,
			IEmployeeRepository employeeRepository,
			StoreDocumentHelper storeDocumentHelper,
			IEntityExtendedPermissionValidator extendedPermissionValidator)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_storeDocumentHelper = storeDocumentHelper ?? throw new ArgumentNullException(nameof(storeDocumentHelper));
			_extendedPermissionValidator =
				extendedPermissionValidator ?? throw new ArgumentNullException(nameof(extendedPermissionValidator));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));

			Init();
			SetPermissions();
			SetViewModels();
			SetOtherProperties();
			SetPropertyChangeRelations();
		}

		public bool CanChangeDocumentType
		{
			get => _canChangeDocumentType;
			set => SetField(ref _canChangeDocumentType, value);
		}
		
		public WriteoffDocumentItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				if(SetField(ref _selectedItem, value))
				{
					OnPropertyChanged(nameof(CanDeleteItem));
					OnPropertyChanged(nameof(CanAddOrDeleteFine));
					OnPropertyChanged(nameof(AddOrEditFineTitle));
				};
			}
		}

		public bool CanEditDocument { get; private set; }
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; private set; }
		public bool CanChangeStorage => CanEditDocument && !Entity.ObservableItems.Any();
		public bool CanShowWarehouseStorage => Entity.WriteOffType == WriteOffType.warehouse;
		public bool CanShowEmployeeStorage => Entity.WriteOffType == WriteOffType.Employee;
		public bool CanShowCarStorage => Entity.WriteOffType == WriteOffType.Car;
		public bool CanDeleteItem => SelectedItem != null;
		public bool CanAddOrDeleteFine => SelectedItem?.Fine != null;
		public string AddOrEditFineTitle => SelectedItem?.Fine != null ? "Изменить штраф" : "Добавить штраф";
		public bool CanChangeItems =>
			CanEditDocument
			&& (Entity.WriteOffFromWarehouse != null
			    || Entity.WriteOffFromEmployee != null
			    || Entity.WriteOffFromCar != null);
		public IEnumerable<Warehouse> Warehouses { get; private set; }
		public IList<CullingCategory> CullingCategories { get; private set; }
		
		public IEntityEntryViewModel ResponsibleEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel WriteOffFromEmployeeViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
			() =>
			{
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(WriteoffDocument), "акта выбраковки"))
				{
					Save();
				}

				var reportInfo = new QS.Report.ReportInfo
				{
					Title = $"Акт выбраковки №{Entity.Id} от {Entity.TimeStamp:d}",
					Identifier = "Store.WriteOff",
					Parameters = new Dictionary<string, object>
					{
						{ "writeoff_id", Entity.Id }
					}
				};

				_reportViewOpener.OpenReport(this, reportInfo);
			}
			));

		public DelegateCommand AddNomenclatureCommand => _addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(
			() =>
			{
				var filter = _scope.Resolve<NomenclatureStockFilterViewModel>();
				filter.SetAndRefilterAtOnce(
					x => x.RestrictWarehouse = Entity.WriteOffFromWarehouse);
				
				var page = NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, NomenclatureStockFilterViewModel>(
					this, filter, OpenPageOptions.AsSlave);

				page.ViewModel.SelectionMode = JournalSelectionMode.Single;
				page.ViewModel.OnEntitySelectedResult += (s, ea) =>
				{
					var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
					
					if(selectedNode == null)
					{
						return;
					}
					
					var nomenclature = NomenclatureRepository.GetNomenclature(UoW, selectedNode.Id);
					
					if(Entity.Items.Any(x => x.Nomenclature.Id == nomenclature.Id))
					{
						return;
					}
					
					Entity.AddItem(nomenclature, 0, selectedNode.StockAmount);
					FireItemsChanged();
				};
			}));

		public DelegateCommand AddInventoryInstanceCommand =>
			_addInventoryInstanceCommand ?? (_addInventoryInstanceCommand = new DelegateCommand(
				() =>
				{
					FireItemsChanged();
				}));
		
		public DelegateCommand DeleteItemCommand => _deleteItemCommand ?? (_deleteItemCommand = new DelegateCommand(
			() =>
			{
				FireItemsChanged();
			}));
		
		public DelegateCommand AddOrEditFineCommand => _addOrEditFineCommand ?? (_addOrEditFineCommand = new DelegateCommand(
			() =>
			{
				FineViewModel fineViewModel;
				if(SelectedItem.Fine != null)
				{
					fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(SelectedItem.Fine.Id), OpenPageOptions.AsSlave).ViewModel;
					fineViewModel.EntitySaved += OnFineSaved;
				}
				else
				{
					fineViewModel = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave).ViewModel;
					fineViewModel.Entity.FineReasonString = "Недостача";
					fineViewModel.EntitySaved += OnNewFineSaved;
				}
				fineViewModel.Entity.TotalMoney = SelectedItem.SumOfDamage;
			}));
		
		public DelegateCommand DeleteFineCommand => _deleteFineCommand ?? (_deleteFineCommand = new DelegateCommand(
			() =>
			{
				UoW.Delete(SelectedItem.Fine);
				SelectedItem.Fine = null;
				OnPropertyChanged(nameof(AddOrEditFineTitle));
				OnPropertyChanged(nameof(CanAddOrDeleteFine));
			}));

		private INomenclatureRepository NomenclatureRepository =>
			_nomenclatureRepository ?? (_nomenclatureRepository = _scope.Resolve<INomenclatureRepository>());
		
		protected override bool BeforeValidation() => Entity.CanEdit;

		protected override bool BeforeSave()
		{
			Entity.LastEditor = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.LastEditedTime = DateTime.Now;
			
			if(Entity.LastEditor == null)
			{
				ShowErrorMessage(
					"Ваш пользователь не привязан к действующему сотруднику," +
					" вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}
			return true;
		}

		private void Init()
		{
			if(Entity.Id == 0)
			{
				Entity.Author = Entity.ResponsibleEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				if(Entity.Author == null)
				{
					ShowErrorMessage(
						"Ваш пользователь не привязан к действующему сотруднику," +
					    " вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
					FailInitialize = true;
					return;
				}

				//Entity.WriteoffWarehouse = _storeDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissionsType.WriteoffEdit);
			}
			else
			{
				CanChangeDocumentType = false;
			}
			
			if(_storeDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissionsType.WriteoffEdit, Entity.WriteOffFromWarehouse))
			{
				FailInitialize = true;
			}
		}
		
		private void SetPermissions()
		{
			CanEditDocument = _storeDocumentHelper.CanEditDocument(WarehousePermissionsType.WriteoffEdit, Entity.WriteOffFromWarehouse);
			UserHasOnlyAccessToWarehouseAndComplaints =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			Entity.CanEdit =
				_extendedPermissionValidator.Validate(
					typeof(WriteoffDocument), UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
		}
		
		private void SetViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<WriteoffDocument>(this, Entity, UoW, NavigationManager, _scope);
			
			ResponsibleEmployeeViewModel = builder.ForProperty(x => x.ResponsibleEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			WriteOffFromEmployeeViewModel = builder.ForProperty(x => x.WriteOffFromEmployee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
			
			CarViewModel = builder.ForProperty(x => x.WriteOffFromCar)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
				.Finish();
		}
		
		private void SetOtherProperties()
		{
			Warehouses = _storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.WriteoffEdit);
			CullingCategories = _scope.Resolve<ICullingCategoryRepository>().GetAllCullingCategories(UoW);
		}
		
		private void SetPropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.WriteOffType,
				() => CanShowWarehouseStorage,
				() => CanShowEmployeeStorage,
				() => CanShowCarStorage);
			
			SetPropertyChangeRelation(
				x => x.WriteOffFromWarehouse,
				() => CanChangeItems);
			SetPropertyChangeRelation(
				x => x.WriteOffFromEmployee,
				() => CanChangeItems);
			SetPropertyChangeRelation(
				x => x.WriteOffFromCar,
				() => CanChangeItems);
		}
		
		private void OnNewFineSaved(object sender, EntitySavedEventArgs e)
		{
			SelectedItem.Fine = e.GetEntity<Fine>();
			OnPropertyChanged(nameof(AddOrEditFineTitle));
			OnPropertyChanged(nameof(CanAddOrDeleteFine));
		}

		private void OnFineSaved(object sender, EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(SelectedItem?.Fine);
			OnPropertyChanged(nameof(CanAddOrDeleteFine));
		}
		
		private void FireItemsChanged()
		{
			OnPropertyChanged(nameof(CanChangeStorage));
			OnPropertyChanged(nameof(CanDeleteItem));
			OnPropertyChanged(nameof(AddOrEditFineTitle));
			OnPropertyChanged(nameof(CanAddOrDeleteFine));
		}
	}
}
