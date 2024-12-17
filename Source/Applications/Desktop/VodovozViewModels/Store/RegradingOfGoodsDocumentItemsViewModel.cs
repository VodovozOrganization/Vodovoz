using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsDocumentItemsViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly IInteractiveService _interactiveService;
		private IStockRepository _stockRepository;
		private readonly INavigationManager _navigationManager;
		private RegradingOfGoodsDocumentItem _newRow;
		private RegradingOfGoodsDocumentItem _fineEditItem;
		private IObservableList<RegradingOfGoodsDocumentItem> _items;
		private IUnitOfWork _unitOfWork;
		private RegradingOfGoodsDocumentViewModel _parentViewModel;
		private object _selectedItem;

		public RegradingOfGoodsDocumentItemsViewModel(
			IInteractiveService interactiveService,
			IStockRepository stockRepository,
			INavigationManager navigationManager)
		{
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_stockRepository = stockRepository
				?? throw new ArgumentNullException(nameof(stockRepository));
			_navigationManager = navigationManager
				?? throw new ArgumentNullException(nameof(navigationManager));

			AddItemCommand = new DelegateCommand(AddItem);
			ChangeOldNomenclatureCommand = new DelegateCommand(ChangeOldNomenclature);
			ChangeNewNomenclatureCommand = new DelegateCommand(ChangeNewNomenclature);
			DeleteItemCommand = new DelegateCommand(DeleteItem);
			AddFineCommand = new DelegateCommand(AddFine);
			DeleteFineCommand = new DelegateCommand(DeleteFine);
			FillFromTemplateCommand = new DelegateCommand(FillFromTemplate);
		}

		[PropertyChangedAlso(nameof(SelectedItem))]
		public object SelectedItemObject
		{
			get => _selectedItem;
			set => SetField(ref _selectedItem, value);
		}

		public RegradingOfGoodsDocumentItem SelectedItem => SelectedItemObject as RegradingOfGoodsDocumentItem;

		public List<RegradingOfGoodsReason> RegradingReasonsCache { get; } = new List<RegradingOfGoodsReason>();
		public List<CullingCategory> DefectTypesCache { get; } = new List<CullingCategory>();

		public IUnitOfWork UnitOfWork
		{
			get => _unitOfWork;
			private set
			{
				if(_unitOfWork != value && value != null)
				{
					_unitOfWork = value;
					LoadRegradingReasonsCache();
					LoadDefectTypesCache();
				}
			}
		}

		public IObservableList<RegradingOfGoodsDocumentItem> Items
		{
			get => _items;
			set => _items = value;
		}

		public Warehouse CurrentWarehouse { get; set; }

		public RegradingOfGoodsDocumentViewModel ParentViewModel
		{
			get => _parentViewModel;
			set
			{
				if(_parentViewModel != value)
				{
					_parentViewModel = value;
					if(value.Entity.Id != 0)
					{
						LoadStock();
					}
				}
			}
		}

		public DelegateCommand AddItemCommand { get; set; }
		public DelegateCommand ChangeOldNomenclatureCommand { get; set; }
		public DelegateCommand ChangeNewNomenclatureCommand { get; }
		public DelegateCommand DeleteItemCommand { get; set; }
		public DelegateCommand AddFineCommand { get; }
		public DelegateCommand DeleteFineCommand { get; }
		public DelegateCommand FillFromTemplateCommand { get; }

		private void LoadDefectTypesCache()
		{
			DefectTypesCache.Clear();

			DefectTypesCache.AddRange(UnitOfWork
				.GetAll<CullingCategory>()
				.OrderBy(c => c.Name));
		}

		private void LoadRegradingReasonsCache()
		{
			RegradingReasonsCache.Clear();

			RegradingReasonsCache.AddRange(UnitOfWork
				.GetAll<RegradingOfGoodsReason>()
				.OrderBy(c => c.Name));
		}

		public void SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			if(unitOfWork is null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			UnitOfWork = unitOfWork;
		}

		private void LoadStock()
		{
			var nomenclatureIds = Items
				.Select(x => x.NomenclatureOld.Id)
				.ToArray();

			if(CurrentWarehouse is null)
			{
				return;
			}

			var inStock =
				_stockRepository.NomenclatureInStock(
					UnitOfWork,
					nomenclatureIds,
					new[] { CurrentWarehouse.Id },
					ParentViewModel.Entity.TimeStamp);

			foreach(var item in Items)
			{
				if(inStock.ContainsKey(item.NomenclatureOld.Id))
				{
					item.AmountInStock = inStock[item.NomenclatureOld.Id];
				}
			}
		}

		private void AddItem()
		{
			var page = _navigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
				ParentViewModel,
				filter =>
				{
					filter.RestrictWarehouse = CurrentWarehouse;
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.TabName = "Выберите номенклатуру на замену";
					viewModel.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedNodes
							.Cast<NomenclatureStockJournalNode>()
							.FirstOrDefault();

						if(selectedNode == null)
						{
							return;
						}

						var nomenclature = UnitOfWork
							.GetById<Nomenclature>(selectedNode.Id);

						_newRow = new RegradingOfGoodsDocumentItem()
						{
							NomenclatureOld = nomenclature,
							AmountInStock = selectedNode.StockAmount
						};

						_navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
							viewModel,
							OpenPageOptions.AsSlave,
							nextViewModel =>
							{
								nextViewModel.SelectionMode = JournalSelectionMode.Single;
								nextViewModel.OnSelectResult += SelectNewNomenclature_ObjectSelected;
							});
					};
				});
		}

		private void ChangeOldNomenclature()
		{
			var page = _navigationManager
				.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(
					ParentViewModel,
					filter =>
					{
						filter.RestrictWarehouse = CurrentWarehouse;
					},
					OpenPageOptions.AsSlave,
					viewModel =>
					{
						viewModel.SelectionMode = JournalSelectionMode.Single;
						viewModel.TabName = "Изменить старую номенклатуру";
						viewModel.OnEntitySelectedResult += (s, ea) =>
						{
							var row = SelectedItem;

							var selectedNode = ea.SelectedNodes
								.Cast<NomenclatureStockJournalNode>()
								.FirstOrDefault();

							if(selectedNode == null)
							{
								return;
							}

							var nomenclature = UnitOfWork
								.GetById<Nomenclature>(selectedNode.Id);
							row.NomenclatureOld = nomenclature;
							row.AmountInStock = selectedNode.StockAmount;
						};
					});
		}

		private void ChangeNewNomenclature()
		{
			_navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
				ParentViewModel,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += ChangeNewNomenclature_OnEntitySelectedResult;
				});
		}

		private void SelectNewNomenclature_ObjectSelected(object sender, JournalSelectedEventArgs e)
		{
			var journalNode = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.FirstOrDefault();

			if(journalNode != null)
			{
				var nomenclature = UnitOfWork
					.GetById<Nomenclature>(journalNode.Id);

				if(!nomenclature.IsDefectiveBottle)
				{
					_newRow.Source = DefectSource.None;
					_newRow.TypeOfDefect = null;
				}

				_newRow.NomenclatureNew = nomenclature;

				ParentViewModel.Entity.AddItem(_newRow);
			}
		}

		private void ChangeNewNomenclature_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var row = SelectedItem;

			if(row == null)
			{
				return;
			}

			var id = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault()?.Id;

			if(id == null)
			{
				return;
			}

			var nomenclature = UnitOfWork.Session.Get<Nomenclature>(id);
			row.NomenclatureNew = nomenclature;

			if(sender is NomenclaturesJournalViewModel nomenclaturesJournalViewModel)
			{
				nomenclaturesJournalViewModel.OnSelectResult -= ChangeNewNomenclature_OnEntitySelectedResult;
			}
		}

		private void DeleteItem()
		{
			var row = SelectedItem;

			if(row.WarehouseIncomeOperation.Id == 0)
			{
				UnitOfWork.Delete(row.WarehouseIncomeOperation);
			}

			if(row.WarehouseWriteOffOperation.Id == 0)
			{
				UnitOfWork.Delete(row.WarehouseWriteOffOperation);
			}

			if(row.Id != 0)
			{
				UnitOfWork.Delete(row);
			}

			Items.Remove(row);
		}

		private void AddFine()
		{
			var selected = SelectedItem;

			if(selected.Fine != null)
			{
				_navigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
					ParentViewModel,
					EntityUoWBuilder.ForOpen(selected.Fine.Id),
					OpenPageOptions.AsSlave,
					viewModel =>
					{
						viewModel.Entity.TotalMoney = selected.SumOfDamage;
						viewModel.EntitySaved += OnFineExistEntitySaved;
					});
			}
			else
			{
				_navigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(
					ParentViewModel,
					EntityUoWBuilder.ForCreate(),
					OpenPageOptions.AsSlave,
					viewModel =>
					{
						viewModel.Entity.FineReasonString = "Недостача";
						viewModel.Entity.TotalMoney = selected.SumOfDamage;
						viewModel.EntitySaved += OnFineNewEntitySaved;
					});
			}

			_fineEditItem = selected;
		}

		private void OnFineNewEntitySaved(object sender, EntitySavedEventArgs e)
		{
			_fineEditItem.Fine = e.Entity as Fine;
			_fineEditItem = null;
		}

		private void OnFineExistEntitySaved(object sender, EntitySavedEventArgs e)
		{
			UnitOfWork.Session.Refresh(_fineEditItem.Fine);
		}

		private void DeleteFine()
		{
			var item = SelectedItem;
			UnitOfWork.Delete(item.Fine);
			item.Fine = null;
		}

		private void FillFromTemplate()
		{
			_navigationManager.OpenViewModel<RegradingOfGoodsTemplateJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave, viewModel =>
			{
				viewModel.SelectionMode = JournalSelectionMode.Single;
				viewModel.OnSelectResult += OnTemplateToFillSelected;
			});
		}

		private void OnTemplateToFillSelected(object sender, JournalSelectedEventArgs e)
		{
			if(Items.Count > 0)
			{
				if(_interactiveService.Question("Текущий список будет очищен. Продолжить?"))
				{
					Items.Clear();
				}
				else
				{
					return;
				}
			}

			var template = UnitOfWork
				.GetById<RegradingOfGoodsTemplate>(e.SelectedObjects.Cast<RegradingOfGoodsTemplate>().FirstOrDefault().Id);

			foreach(var item in template.Items)
			{
				ParentViewModel.Entity.AddItem(new RegradingOfGoodsDocumentItem()
				{
					NomenclatureNew = item.NomenclatureNew,
					NomenclatureOld = item.NomenclatureOld
				});
			}

			LoadStock();
		}

		public void Dispose()
		{
			ParentViewModel = null;
			UnitOfWork = null;
		}
	}
}
