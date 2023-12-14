using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : EntityTabViewModelBase<DiscountReason>
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IProductGroupJournalFactory _productGroupJournalFactory;
		private readonly IEntityAutocompleteSelectorFactory _nomenclatureAutocompleteSelectorFactory;
		private ILifetimeScope _lifetimeScope;
		private Nomenclature _selectedNomenclature;
		private ProductGroup _selectedProductGroup;
		
		private DelegateCommand _addNomenclatureCommand;
		private DelegateCommand _addProductGroupCommand;
		private DelegateCommand _removeNomenclatureCommand;
		private DelegateCommand _removeProductGroupCommand;
		private DelegateCommand<bool> _updateSelectedCategoriesCommand;
		
		public DiscountReasonViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDiscountReasonRepository discountReasonRepository,
			IProductGroupJournalFactory productGroupJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_productGroupJournalFactory = productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory));
			_nomenclatureAutocompleteSelectorFactory =
				(nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory)))
				.GetDefaultNomenclatureSelectorFactory(_lifetimeScope);
			
			TabName = UoWGeneric.IsNew ? "Новое основание для скидки" : $"Основание для скидки \"{Entity.Name}\"";

			InitializeNomenclatureCategoriesList();
		}

		public bool IsNomenclatureSelected => SelectedNomenclature != null;
		public bool IsProductGroupSelected => SelectedProductGroup != null;
		public bool CanChangeDiscountReasonName => Entity.Id == 0;

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
		
		public IList<SelectableNomenclatureCategoryNode> SelectableNomenclatureCategoryNodes { get; private set; }

		public DelegateCommand AddNomenclatureCommand => _addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(
					() =>
					{
						var journalViewModel = _nomenclatureAutocompleteSelectorFactory.CreateAutocompleteSelector();
						journalViewModel.OnEntitySelectedResult += (s, ea) =>
						{
							var selectedNode = ea.SelectedNodes.FirstOrDefault();
							if(selectedNode == null)
							{
								return;
							}

							Entity.AddNomenclature(UoW.GetById<Nomenclature>(selectedNode.Id));
						};
						TabParent.AddSlaveTab(this, journalViewModel);
					}
				)
			);
		
		public DelegateCommand RemoveNomenclatureCommand => _removeNomenclatureCommand ?? (_removeNomenclatureCommand = new DelegateCommand(
					() =>
					{
						Entity.RemoveNomenclature(_selectedNomenclature);
					}
				)
			);

		public DelegateCommand AddProductGroupCommand => _addProductGroupCommand ?? (_addProductGroupCommand = new DelegateCommand(
				() =>
				{
					var journalViewModel = _productGroupJournalFactory.CreateProductGroupAutocompleteSelector();
					journalViewModel.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}

						Entity.AddProductGroup(UoW.GetById<ProductGroup>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, journalViewModel);
				}
			)
		);
		
		public DelegateCommand RemoveProductGroupCommand => _removeProductGroupCommand ?? (_removeProductGroupCommand = new DelegateCommand(
				() =>
				{
					Entity.RemoveProductGroup(_selectedProductGroup);
				}
			)
		);
		
		public DelegateCommand<bool> UpdateSelectedCategoriesCommand =>
			_updateSelectedCategoriesCommand ?? (_updateSelectedCategoriesCommand = new DelegateCommand<bool>(
					selected =>
					{
						foreach(var node in SelectableNomenclatureCategoryNodes)
						{
							node.IsSelected = selected;
							UpdateNomenclatureCategories(node);
						}
					}
				)
			);
		
		public override bool Save(bool close)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				if(_discountReasonRepository.ExistsActiveDiscountReasonWithName(
					uow, Entity.Id, Entity.Name, out var activeDiscountReasonWithSameName))
				{
					CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Уже существует основание для скидки с таким названием.\n" +
						"Сохранение текущего основания невозможно.\n" +
						"Существующее основание:\n" +
						$"Код: {activeDiscountReasonWithSameName.Id}\n" +
						$"Название: {activeDiscountReasonWithSameName.Name}");
					return false;
				}
			}
			
			return base.Save(close);
		}

		public void UpdateNomenclatureCategories(SelectableNomenclatureCategoryNode selectedCategory) =>
			Entity.UpdateNomenclatureCategories(selectedCategory);
		
		private void InitializeNomenclatureCategoriesList()
		{
			SelectableNomenclatureCategoryNodes = new GenericObservableList<SelectableNomenclatureCategoryNode>();
			var discountNomenclatureCategories = UoW.GetAll<DiscountReasonNomenclatureCategory>().ToList();
			
			foreach(var category in discountNomenclatureCategories)
			{
				SelectableNomenclatureCategoryNodes.Add(CreateNewNode(category));
			}
		}

		private SelectableNomenclatureCategoryNode CreateNewNode(DiscountReasonNomenclatureCategory discountNomenclatureCategory)
		{
			return new SelectableNomenclatureCategoryNode
			{
				DiscountReasonNomenclatureCategory = discountNomenclatureCategory,
				IsSelected = Entity.NomenclatureCategories.Contains(discountNomenclatureCategory)
			};
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
