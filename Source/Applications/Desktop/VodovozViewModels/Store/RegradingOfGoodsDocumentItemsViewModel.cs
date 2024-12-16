using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsDocumentItemsViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private IStockRepository _stockRepository;
		private readonly INavigationManager _navigationManager;
		private RegradingOfGoodsDocumentItem _newRow;
		private RegradingOfGoodsDocumentItem _fineEditItem;
		private ICollection<RegradingOfGoodsDocumentItem> _items;

		public RegradingOfGoodsDocumentItemsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IStockRepository stockRepository,
			INavigationManager navigationManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_stockRepository = stockRepository
				?? throw new ArgumentNullException(nameof(stockRepository));
			_navigationManager = navigationManager
				?? throw new ArgumentNullException(nameof(navigationManager));

			using(IUnitOfWork uow = unitOfWorkFactory.CreateWithoutRoot("Диалог документа пересортицы товаров -> загрузка аэшей"))
			{
				DefectTypesCache = uow
					.GetAll<CullingCategory>()
					.OrderBy(c => c.Name)
					.ToList();

				RegradingReasonsCache = uow
					.GetAll<RegradingOfGoodsReason>()
					.OrderBy(c => c.Name)
					.ToList();
			}

			AddItemCommand = new DelegateCommand(AddItem);
		}

		public IList<RegradingOfGoodsReason> RegradingReasonsCache { get; }
		public IList<CullingCategory> DefectTypesCache { get; }

		public ICollection<RegradingOfGoodsDocumentItem> Items
		{
			get => _items;
			set => _items = value;
		}

		public Warehouse CurrentWarehouse { get; set; }

		public RegradingOfGoodsDocumentViewModel ParentViewModel { get; set; }

		public DelegateCommand AddItemCommand { get; set; }

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

						var nomenclature = ParentViewModel.UoW
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

		private void SelectNewNomenclature_ObjectSelected(object sender, JournalSelectedEventArgs e)
		{
			var journalNode = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.FirstOrDefault();

			if(journalNode != null)
			{
				var nomenclature = ParentViewModel.UoW
					.GetById<Nomenclature>(journalNode.Id);

				if(!nomenclature.IsDefectiveBottle)
				{
					_newRow.Source = DefectSource.None;
					_newRow.TypeOfDefect = null;
				}

				_newRow.NomenclatureNew = nomenclature;

				// TODO: Тренняк!!! Убрать!!!

				ParentViewModel.Entity.AddItem(_newRow);
			}
		}
	}
}
