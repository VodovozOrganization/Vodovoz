﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class DiscountReasonViewModel : EntityTabViewModelBase<DiscountReason>
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private ILifetimeScope _lifetimeScope;
		private Nomenclature _selectedNomenclature;
		private ProductGroup _selectedProductGroup;
		private ProductGroupsJournalViewModel _selectProductGroupJournalViewModel;

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
			INavigationManager navigationManager)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			
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

		public ProductGroupsJournalViewModel SelectProductGroupJournalViewModel
		{
			get => _selectProductGroupJournalViewModel;
			set
			{
				if(_selectProductGroupJournalViewModel != null)
				{
					_selectProductGroupJournalViewModel.OnSelectResult -= OnProductGroupSelected;
				}

				SetField(ref _selectProductGroupJournalViewModel, value);

				_selectProductGroupJournalViewModel.OnSelectResult += OnProductGroupSelected;
			}
		}


		public IList<SelectableNomenclatureCategoryNode> SelectableNomenclatureCategoryNodes { get; private set; }

		public DelegateCommand AddNomenclatureCommand => _addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(
					() =>
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

					SelectProductGroupJournalViewModel = selectGroupPage.ViewModel;
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

		private void OnProductGroupSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.FirstOrDefault();

			if(selectedNode == null)
			{
				return;
			}

			if(!(selectedNode is ProductGroupsJournalNode selectedProductNode))
			{
				return;
			}

			Entity.AddProductGroup(UoW.GetById<ProductGroup>(selectedProductNode.Id));
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

			if(_selectProductGroupJournalViewModel != null)
			{
				_selectProductGroupJournalViewModel.OnSelectResult -= OnProductGroupSelected;
			}

			base.Dispose();
		}
	}
}
