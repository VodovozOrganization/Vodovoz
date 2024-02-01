using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Search;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Client
{
	public class SupplierPricesWidgetViewModel : EntityWidgetViewModelBase<Counterparty>, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _navigationManager;
		private ITdiTab _dialogTab;
		private bool _canRemove = false;

		public event EventHandler ListContentChanged;

		public IJournalSearch Search { get; private set; }

		public SupplierPricesWidgetViewModel(
			Counterparty entity,
			IUnitOfWork uow,
			ITdiTab dialogTab,
			ICommonServices commonServices,
			ITdiCompatibilityNavigation navigationManager)
			: base(entity, commonServices)
		{
			_dialogTab = dialogTab ?? throw new ArgumentNullException(nameof(dialogTab));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));

			CreateCommands();
			RefreshPrices();

			Search = new SearchViewModel();
			Search.OnSearch += (sender, e) => RefreshPrices();
			Entity.ObservableSuplierPriceItems.ElementAdded += (aList, aIdx) => RefreshPrices();
			Entity.ObservableSuplierPriceItems.ElementRemoved += (aList, aIdx, aObject) => RefreshPrices();
		}

		private void CreateCommands()
		{
			CreateAddItemCommand();
			CreateRemoveItemCommand();
			CreateEditItemCommand();
		}

		private void RefreshPrices()
		{
			Entity.SupplierPriceListRefresh(Search?.SearchValues);
			ListContentChanged?.Invoke(this, new EventArgs());
		}


		public bool CanAdd { get; set; } = true;
		public bool CanEdit { get; set; } = false;//задача редактирования пока не актуальна

		public bool CanRemove
		{
			get => _canRemove;
			set => SetField(ref _canRemove, value);
		}

		#region Commands

		#region AddItemCommand

		public DelegateCommand AddItemCommand { get; private set; }

		private void CreateAddItemCommand()
		{
			AddItemCommand = new DelegateCommand(
				() =>
				{
					var existingNomenclatures =
						Entity.ObservableSuplierPriceItems.Select(i => i.NomenclatureToBuy.Id).Distinct();

					var journalViewModel =
						_navigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
							_dialogTab,
							filter =>
							{
								filter.HidenByDefault = true;
								filter.RestrictedExcludedIds = existingNomenclatures.ToArray();
							},
							OpenPageOptions.AsSlave,
							vm =>
							{
								vm.SelectionMode = JournalSelectionMode.Single;
							}
						).ViewModel;

					journalViewModel.OnSelectResult += (sender, e) =>
					{
						var selectedObject = e.SelectedObjects.FirstOrDefault();
						if(!(selectedObject is NomenclatureJournalNode selectedNode))
						{
							return;
						}

						Entity.AddSupplierPriceItems(UoW.GetById<Nomenclature>(selectedNode.Id));
					};
				},
				() => true
			);
		}

		#endregion AddItemCommand

		#region RemoveItemCommand

		public DelegateCommand<ISupplierPriceNode> RemoveItemCommand { get; private set; }

		private void CreateRemoveItemCommand()
		{
			RemoveItemCommand = new DelegateCommand<ISupplierPriceNode>(
				n => Entity.RemoveNomenclatureWithPrices(n.NomenclatureToBuy.Id),
				n => CanRemove
			);
		}

		#endregion RemoveItemCommand

		#region EditItemCommand

		public DelegateCommand EditItemCommand { get; private set; }

		private void CreateEditItemCommand()
		{
			EditItemCommand = new DelegateCommand(
				() => throw new NotImplementedException(nameof(EditItemCommand)),
				() => false
			);
		}

		#endregion EditItemCommand

		#endregion Commands

		public void Dispose()
		{
			_dialogTab = null;
		}
	}
}
