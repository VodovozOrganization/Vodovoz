using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Store
{
	public class RegradingOfGoodsTemplateItemsViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly INavigationManager _navigationManager;

		private RegradingOfGoodsTemplateItem _newRow;
		private object _selectedItem;
		private IObservableList<RegradingOfGoodsTemplateItem> _items;

		public RegradingOfGoodsTemplateItemsViewModel(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager
				?? throw new ArgumentNullException(nameof(navigationManager));

			AddItemCommand = new DelegateCommand(AddItem);

			ChangeNewNomenclatureCommand = new DelegateCommand(ChangeNewNomenclature, () => CanChangeSelectedItem);
			ChangeNewNomenclatureCommand.CanExecuteChangedWith(this, x => CanChangeSelectedItem);

			ChangeOldNomenclatureCommand = new DelegateCommand(ChangeOldNomenclature, () => CanChangeSelectedItem);
			ChangeOldNomenclatureCommand.CanExecuteChangedWith(this, x => CanChangeSelectedItem);

			DeleteItemCommand = new DelegateCommand(DeleteItem, () => CanChangeSelectedItem);
			DeleteItemCommand.CanExecuteChangedWith(this, x => CanChangeSelectedItem);

		}

		public DelegateCommand AddItemCommand { get; }
		public DelegateCommand ChangeNewNomenclatureCommand { get; set; }
		public DelegateCommand ChangeOldNomenclatureCommand { get; }
		public DelegateCommand DeleteItemCommand { get; }
		public RegradingOfGoodsTemplateViewModel ParentViewModel { get; set; }
		public IUnitOfWork UnitOfWork { get; private set; }

		public IObservableList<RegradingOfGoodsTemplateItem> Items
		{
			get => _items;
			set => _items = value;
		}

		[PropertyChangedAlso(nameof(SelectedItem))]
		[PropertyChangedAlso(nameof(CanChangeSelectedItem))]
		public object SelectedItemObject
		{
			get => _selectedItem;
			set => SetField(ref _selectedItem, value);
		}

		public RegradingOfGoodsTemplateItem SelectedItem => SelectedItemObject as RegradingOfGoodsTemplateItem;

		public bool CanChangeSelectedItem => SelectedItem != null;

		public void SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			UnitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		private void AddItem()
		{
			var page = _navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
				ParentViewModel,
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Выберите номенклатуру на замену";
					vievModel.OnSelectResult += OldNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void OldNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.FirstOrDefault();

			if(node == null)
			{
				return;
			}

			var nomenclature = UnitOfWork.GetById<Nomenclature>(node.Id);

			_newRow = new RegradingOfGoodsTemplateItem()
			{
				NomenclatureOld = nomenclature
			};

			_navigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				ParentViewModel,
				filter =>
				{
					filter.RestrictArchive = true;
					filter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
				},
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Выберите новую номенклатуру";
					vievModel.OnSelectResult += NewNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void NewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.FirstOrDefault();

			if(node == null)
			{
				_newRow = null;
				return;
			}

			var nomenclature = UnitOfWork.GetById<Nomenclature>(node.Id);

			_newRow.NomenclatureNew = nomenclature;
			ParentViewModel.Entity.AddItem(_newRow);
		}

		private void ChangeNewNomenclature()
		{
			_navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
				ParentViewModel,
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Изменить новую номенклатуру";
					vievModel.OnSelectResult += ChangeNewNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void ChangeNewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var row = SelectedItem;
			var node = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

			if(node == null || row == null)
			{
				return;
			}

			var nomenclature = UnitOfWork.GetById<Nomenclature>(node.Id);
			row.NomenclatureNew = nomenclature;
		}

		private void ChangeOldNomenclature()
		{
			_navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
				ParentViewModel,
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Изменить старую номенклатуру";
					vievModel.OnSelectResult += ChangeOldNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void ChangeOldNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var row = SelectedItem;
			var node = e.SelectedObjects
				.Cast<NomenclatureJournalNode>()
				.FirstOrDefault();

			if(node == null || row == null)
			{
				return;
			}

			var nomenclature = UnitOfWork.GetById<Nomenclature>(node.Id);
			row.NomenclatureOld = nomenclature;
		}

		private void DeleteItem()
		{
			var row = SelectedItem;
			if(row.Id != 0)
			{
				UnitOfWork.Delete(row);
			}

			Items.Remove(row);
		}

		public void Dispose()
		{
			ParentViewModel = null;
			UnitOfWork = null;
		}
	}
}
