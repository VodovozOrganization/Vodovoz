using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : EntityTabViewModelBase<DiscountReason>, IAskSaveOnCloseViewModel
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private ILifetimeScope _lifetimeScope;
		private Nomenclature _selectedNomenclature;
		private ProductGroup _selectedProductGroup;
		private ProductGroupsJournalViewModel _selectProductGroupJournalViewModel;

		private int _currentPage;
		private bool _hasOrderMinSum;
		private bool _discountInfoTabActive;
		private bool _promoCodeSettingsTabActive;
		private bool _hasPromoCodeDurationTime;
		private bool _selectedAllCategories;

		public DiscountReasonViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository,
			INavigationManager navigationManager)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			
			TabName = IsNewEntity ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";

			SetPermissions();
			InitializeCommands();
			InitializeNomenclatureCategoriesList();
			InitializeHasOrderMinSum();
			InitializeHasPromoCodeDurationTime();
		}

		public bool IsNewEntity => Entity.Id == 0;
		public bool AskSaveOnClose => CanEditDiscountReason;
		public bool CanEditDiscountReason => (IsNewEntity && PermissionResult.CanCreate) || PermissionResult.CanUpdate;
		public bool CanRemoveNomenclature => IsNomenclatureSelected && CanEditDiscountReason;
		public bool IsNomenclatureSelected => SelectedNomenclature != null;
		public bool CanRemoveProductGroup => IsProductGroupSelected && CanEditDiscountReason;
		public bool IsProductGroupSelected => SelectedProductGroup != null;
		public bool CanChangeDiscountReasonName => IsNewEntity && CanEditDiscountReason;

		public Nomenclature SelectedNomenclature
		{
			get => _selectedNomenclature;
			set
			{
				if(SetField(ref _selectedNomenclature, value))
				{
					OnPropertyChanged(nameof(IsNomenclatureSelected));
				}
			} 
		}
		
		public ProductGroup SelectedProductGroup
		{
			get => _selectedProductGroup;
			set
			{
				if(SetField(ref _selectedProductGroup, value))
				{
					OnPropertyChanged(nameof(IsProductGroupSelected));
				}
			} 
		}
		
		public bool SelectedAllCategories
		{
			get => _selectedAllCategories;
			set
			{
				if(SetField(ref _selectedAllCategories, value))
				{
					UpdateSelectedCategories(_selectedAllCategories);
				}
			}
		}

		public IList<SelectableNomenclatureCategoryNode> SelectableNomenclatureCategoryNodes { get; private set; }
		
		public ICommand SaveCommand { get; private set; }
		public ICommand CloseCommand { get; private set; }
		public ICommand AddProductGroupCommand { get; private set; }
		public ICommand RemoveProductGroupCommand { get; private set; }
		public ICommand AddNomenclatureCommand { get; private set; }
		public ICommand RemoveNomenclatureCommand { get; private set; }

		public int CurrentPage
		{
			get => _currentPage;
			set => SetField(ref _currentPage, value);
		}

		public bool HasOrderMinSum
		{
			get => _hasOrderMinSum;
			set
			{
				if(SetField(ref _hasOrderMinSum, value) && !value)
				{
					Entity.ResetOrderMinSum();
				}
			}
		}

		public bool HasPromoCodeDurationTime
		{
			get => _hasPromoCodeDurationTime;
			set
			{
				if(SetField(ref _hasPromoCodeDurationTime, value) && !value)
				{
					Entity.ResetTimeDuration();
				}
			}
		}

		public bool CanEditPromoCode { get; private set; }
		public bool CanChangePromoCodeName => IsNewEntity && CanEditPromoCode;
		public bool CanChangeIsPromoCode => IsNewEntity && CanEditPromoCode;

		public bool DiscountInfoTabActive
		{
			get => _discountInfoTabActive;
			set
			{
				if(SetField(ref _discountInfoTabActive, value) && value)
				{
					CurrentPage = 0;
				}
			}
		}
		
		public bool PromoCodeSettingsTabActive
		{
			get => _promoCodeSettingsTabActive;
			set
			{
				if(SetField(ref _promoCodeSettingsTabActive, value) && value)
				{
					CurrentPage = 1;
				}
			}
		}

		public void UpdateNomenclatureCategories(SelectableNomenclatureCategoryNode selectedCategory) =>
			Entity.UpdateNomenclatureCategories(selectedCategory);
		
		private void SetPermissions()
		{
			CanEditPromoCode = CommonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Permissions.DiscountReasonPermissions.CanEditPromoCode)
				&& CanEditDiscountReason;
		}
		
		private void InitializeCommands()
		{
			SaveCommand = new DelegateCommand(SaveAndClose);
			CloseCommand = new DelegateCommand(()=> Close(false, CloseSource.Cancel));
			AddProductGroupCommand = new DelegateCommand(AddProductGroup);
			RemoveProductGroupCommand = new DelegateCommand(RemoveProductGroup);
			AddNomenclatureCommand = new DelegateCommand(AddNomenclature);
			RemoveNomenclatureCommand = new DelegateCommand(RemoveNomenclature);
		}
		
		private void UpdateSelectedCategories(bool value)
		{
			foreach(var node in SelectableNomenclatureCategoryNodes)
			{
				node.IsSelected = value;
				UpdateNomenclatureCategories(node);
			}
		}
		
		private void AddNomenclature()
		{
			NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(this,
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
					vm.OnSelectResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						Entity.AddNomenclature(UoW.GetById<Nomenclature>(selectedNode.Id));
					};
				});
		}

		private void RemoveNomenclature()
		{
			Entity.RemoveNomenclature(_selectedNomenclature);
		}

		private void AddProductGroup()
		{
			var selectGroupPage = NavigationManager.OpenViewModel<ProductGroupsJournalViewModel, Action<ProductGroupsJournalFilterViewModel>>(
				this,
				filter =>
				{
					filter.IsGroupSelectionMode = true;
				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Single;
				});
					
			if(_selectProductGroupJournalViewModel != null)
			{
				_selectProductGroupJournalViewModel.OnSelectResult -= OnProductGroupSelected;
			}

			_selectProductGroupJournalViewModel = selectGroupPage.ViewModel;
			_selectProductGroupJournalViewModel.OnSelectResult += OnProductGroupSelected;
		}

		private void RemoveProductGroup()
		{
			Entity.RemoveProductGroup(_selectedProductGroup);
		}

		private void OnProductGroupSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.FirstOrDefault();

			if(!(selectedNode is ProductGroupsJournalNode selectedProductNode))
			{
				return;
			}

			Entity.AddProductGroup(UoW.GetById<ProductGroup>(selectedProductNode.Id));
		}
		
		private void InitializeNomenclatureCategoriesList()
		{
			SelectableNomenclatureCategoryNodes = new GenericObservableList<SelectableNomenclatureCategoryNode>();
			var discountNomenclatureCategories = UoW.GetAll<DiscountReasonNomenclatureCategory>().ToList();
			
			foreach(var category in discountNomenclatureCategories)
			{
				SelectableNomenclatureCategoryNodes.Add(
					SelectableNomenclatureCategoryNode.Create(
						category,
						Entity.NomenclatureCategories.Contains(category)));
			}
		}

		private void InitializeHasOrderMinSum()
		{
			HasOrderMinSum = Entity.HasOrderMinSum;
		}

		private void InitializeHasPromoCodeDurationTime()
		{
			HasPromoCodeDurationTime = Entity.HasPromoCodeDurationTime;
		}

		public override void Dispose()
		{
			_lifetimeScope = null;

			if(_selectProductGroupJournalViewModel != null)
			{
				_selectProductGroupJournalViewModel.OnSelectResult -= OnProductGroupSelected;
			}

			base.Dispose();
		}
	}
}
