using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.HistoryLog.Domain;
using QS.HistoryLog.Repositories;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class FixedPricesViewModel : UoWWidgetViewModelBase, IDisposable
	{
		private readonly IFixedPricesModel _fixedPricesModel;
		private ITdiTab _parentTab;
		private ILifetimeScope _lifetimeScope;

		public FixedPricesViewModel(
			IUnitOfWork uow,
			IFixedPricesModel fixedPricesModel,
			ITdiTab parentTab,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			_fixedPricesModel = fixedPricesModel ?? throw new ArgumentNullException(nameof(fixedPricesModel));
			_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			fixedPricesModel.FixedPricesUpdated += (sender, args) => UpdateFixedPrices();
			UpdateFixedPrices();

			FixedPrices.PropertyChanged += OnFixedPricesPropertyChanged;
			UpdateNomenclatures();
		}

		private void OnFixedPricesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateNomenclatures();
		}

		private bool _canEdit = true;
		public virtual bool CanEdit
		{
			get => _canEdit;
			set => SetField(ref _canEdit, value);
		}

		private IDiffFormatter _diffFormatter;
		public virtual IDiffFormatter DiffFormatter
		{
			get => _diffFormatter;
			set => SetField(ref _diffFormatter, value);
		}

		private GenericObservableList<FixedPriceItemViewModel> _fixedPrices = new GenericObservableList<FixedPriceItemViewModel>();
		public virtual GenericObservableList<FixedPriceItemViewModel> FixedPrices
		{
			get => _fixedPrices;
			set => SetField(ref _fixedPrices, value);
		}

		private void UpdateFixedPrices()
		{
			FixedPrices.Clear();

			foreach(NomenclatureFixedPrice fixedPrice in _fixedPricesModel.FixedPrices)
			{
				var fixedPriceViewModel = new FixedPriceItemViewModel(fixedPrice, _fixedPricesModel);
				FixedPrices.Add(fixedPriceViewModel);
			}

			_fixedPricesModel.FixedPrices.ElementAdded += FixedPricesOnElementAdded;
			_fixedPricesModel.FixedPrices.ElementRemoved += FixedPricesOnElementRemoved;
		}

		private void UpdateNomenclatures()
		{
			var selectedNomenclature = SelectedNomenclature;
			FixedPriceNomenclatures.Clear();
			foreach(var fixedPrice in FixedPrices)
			{
				if(!FixedPriceNomenclatures.Contains(fixedPrice.NomenclatureFixedPrice.Nomenclature))
				{
					FixedPriceNomenclatures.Add(fixedPrice.NomenclatureFixedPrice.Nomenclature);
				}
			}

			if(FixedPriceNomenclatures.Contains(selectedNomenclature))
			{
				SelectedNomenclature = selectedNomenclature;
			}
		}

		private void UpdateFixedPricesByNomenclature()
		{
			FixedPricesByNomenclature.Clear();
			foreach(var fixedPrice in FixedPrices.OrderBy(p => p.MinCount))
			{
				if(fixedPrice.NomenclatureFixedPrice.Nomenclature == SelectedNomenclature)
				{
					FixedPricesByNomenclature.Add(fixedPrice);
				}
			}
		}

		private void FixedPricesOnElementAdded(object alist, int[] aidx)
		{
			foreach(var index in aidx)
			{
				var addedFixedPrice = _fixedPricesModel.FixedPrices[index];
				var addedFixedPriceViewModel = new FixedPriceItemViewModel(addedFixedPrice, _fixedPricesModel);

				if(FixedPrices.All(x => x.NomenclatureFixedPrice != addedFixedPrice))
				{
					FixedPrices.Add(addedFixedPriceViewModel);
				}
			}
		}

		private void FixedPricesOnElementRemoved(object alist, int[] aidx, object aobject)
		{
			var removedFixedPrice = (NomenclatureFixedPrice)aobject;
			var viewModelToRemove = FixedPrices.FirstOrDefault(x => x.NomenclatureFixedPrice == removedFixedPrice);

			if(viewModelToRemove != null)
			{
				FixedPrices.Remove(viewModelToRemove);
			}
		}

		private GenericObservableList<Nomenclature> _fixedPriceNomenclatures = new GenericObservableList<Nomenclature>();
		public virtual GenericObservableList<Nomenclature> FixedPriceNomenclatures
		{
			get => _fixedPriceNomenclatures;
			set => SetField(ref _fixedPriceNomenclatures, value);
		}

		private GenericObservableList<FixedPriceItemViewModel> _fixedPricesByNomenclature = new GenericObservableList<FixedPriceItemViewModel>();
		public virtual GenericObservableList<FixedPriceItemViewModel> FixedPricesByNomenclature
		{
			get => _fixedPricesByNomenclature;
			set => SetField(ref _fixedPricesByNomenclature, value);
		}

		private Nomenclature _selectedNomenclature;
		public virtual Nomenclature SelectedNomenclature
		{
			get => _selectedNomenclature;
			set
			{
				if(SetField(ref _selectedNomenclature, value))
				{
					UpdateFixedPricesByNomenclature();
				}
			}
		}

		private FixedPriceItemViewModel _selectedFixedPrice;
		public virtual FixedPriceItemViewModel SelectedFixedPrice
		{
			get => _selectedFixedPrice;
			set
			{
				if(SetField(ref _selectedFixedPrice, value))
				{
					UpdateFixedPriceHistory();
				}
			}
		}

		private IList<FieldChange> _selectedPriceChanges;
		public virtual IList<FieldChange> SelectedPriceChanges
		{
			get => _selectedPriceChanges;
			set => SetField(ref _selectedPriceChanges, value);
		}

		#region Commands

		#region Добавление номенклатуры
		private DelegateCommand _addNomenclatureCommand;
		public DelegateCommand AddNomenclatureCommand
		{
			get
			{
				if(_addNomenclatureCommand == null)
				{
					_addNomenclatureCommand = new DelegateCommand(() => SelectWaterNomenclature(), () => CanEdit);
					_addNomenclatureCommand.CanExecuteChangedWith(this, x => x.CanEdit);
				}
				return _addNomenclatureCommand;
			}
		}

		private void SelectWaterNomenclature()
		{
			(NavigationManager as ITdiCompatibilityNavigation)
				.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(_parentTab, filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.water;
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += OnWaterSelected;
				});
		}

		private void OnWaterSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedWaterNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

			if(selectedWaterNode == null)
			{
				return;
			}

			Nomenclature waterNomenclature = UoW.GetById<Nomenclature>(selectedWaterNode.Id);

			if(!FixedPrices.Any(p => p.NomenclatureFixedPrice.Nomenclature == waterNomenclature))
			{
				AddFixedPrice(waterNomenclature);
			}
		}

		private void AddFixedPrice(Nomenclature nomenclature)
		{
			decimal price = 0;
			if(nomenclature.DependsOnNomenclature != null)
			{
				var fixPrice = FixedPrices.FirstOrDefault(p => p.NomenclatureFixedPrice.Nomenclature.Id == nomenclature.DependsOnNomenclature.Id);
				price = fixPrice == null ? 0 : fixPrice.FixedPrice;
			}

			_fixedPricesModel.AddFixedPrice(nomenclature, price, 0);
		}
		#endregion

		#region Добавление фиксы
		private DelegateCommand _addFixedPriceCommand;
		public DelegateCommand AddFixedPriceCommand
		{
			get
			{
				if(_addFixedPriceCommand == null)
				{
					_addFixedPriceCommand = new DelegateCommand(() => AddFixedPrice(SelectedNomenclature), () => CanEdit && SelectedNomenclature != null);
					_addFixedPriceCommand.CanExecuteChangedWith(this, x => x.CanEdit, x => x.SelectedNomenclature);
				}
				return _addFixedPriceCommand;
			}
		}
		#endregion

		#region Удаление фиксы
		private DelegateCommand _removeFixedPriceCommand;
		public DelegateCommand RemoveFixedPriceCommand
		{
			get
			{
				if(_removeFixedPriceCommand == null)
				{
					_removeFixedPriceCommand = new DelegateCommand(RemoveFixedPrice,
						() => CanEdit && SelectedFixedPrice != null
					);
					_removeFixedPriceCommand.CanExecuteChangedWith(this, x => x.CanEdit, x => x.SelectedFixedPrice);
				}
				return _removeFixedPriceCommand;
			}
		}

		public INavigationManager NavigationManager { get; }

		private void RemoveFixedPrice()
		{
			if(SelectedFixedPrice == null)
			{
				return;
			}

			_fixedPricesModel.RemoveFixedPrice(SelectedFixedPrice.NomenclatureFixedPrice);
		}
		#endregion

		#endregion

		private void UpdateFixedPriceHistory()
		{
			List<FieldChange> fixedPricesChanges = new List<FieldChange>();
			if(SelectedFixedPrice != null)
			{
				var priceChanges = HistoryChangesRepository
					.GetFieldChanges<NomenclatureFixedPrice>(UoW, new[] { SelectedFixedPrice.NomenclatureFixedPrice.Id }, x => x.Price)
					.OrderBy(x => x.Entity.ChangeTime)
					.ToList();

				var countChanges = HistoryChangesRepository
					.GetFieldChanges<NomenclatureFixedPrice>(UoW, new[] { SelectedFixedPrice.NomenclatureFixedPrice.Id }, x => x.MinCount)
					.OrderBy(x => x.Entity.ChangeTime)
					.ToList();

				fixedPricesChanges.AddRange(priceChanges);
				fixedPricesChanges.AddRange(countChanges);
				fixedPricesChanges = fixedPricesChanges.OrderByDescending(x => x.Entity.ChangeTime).ToList();

				foreach(var change in fixedPricesChanges)
				{
					change.DiffFormatter = DiffFormatter;
				}
			}
			SelectedPriceChanges = fixedPricesChanges;
		}

		public void Dispose()
		{
			_lifetimeScope = null;
			_parentTab = null;
		}
	}
}
