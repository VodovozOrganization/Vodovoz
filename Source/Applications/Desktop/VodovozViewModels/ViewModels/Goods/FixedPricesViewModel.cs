using System;
using QS.ViewModels;
using Vodovoz.Models;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Goods;
using QS.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using QS.HistoryLog.Repositories;
using System.Linq;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.HistoryLog.Domain;
using QS.Project.Journal;
using QS.Tdi;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class FixedPricesViewModel : UoWWidgetViewModelBase
	{
		private readonly IFixedPricesModel fixedPricesModel;
		private readonly INomenclatureJournalFactory nomenclatureSelectorFactory;
		private readonly ITdiTab parentTab;

		public FixedPricesViewModel(IUnitOfWork uow, IFixedPricesModel fixedPricesModel, INomenclatureJournalFactory nomenclatureSelectorFactory, ITdiTab parentTab)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.fixedPricesModel = fixedPricesModel ?? throw new ArgumentNullException(nameof(fixedPricesModel));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));

			fixedPricesModel.FixedPricesUpdated += (sender, args) => UpdateFixedPrices();
			UpdateFixedPrices();

			FixedPrices.PropertyChanged += OnFixedPricesPropertyChanged;
			UpdateNomenclatures();
		}

		private void OnFixedPricesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateNomenclatures();
		}

		private bool canEdit = true;
		public virtual bool CanEdit {
			get => canEdit;
			set => SetField(ref canEdit, value);
		}

		private IDiffFormatter diffFormatter;
		public virtual IDiffFormatter DiffFormatter {
			get => diffFormatter;
			set => SetField(ref diffFormatter, value);
		}

		private GenericObservableList<FixedPriceItemViewModel> fixedPrices = new GenericObservableList<FixedPriceItemViewModel>();
		public virtual GenericObservableList<FixedPriceItemViewModel> FixedPrices {
			get => fixedPrices;
			set => SetField(ref fixedPrices, value);
		}

		private void UpdateFixedPrices()
		{
			FixedPrices.Clear();
			foreach (NomenclatureFixedPrice fixedPrice in fixedPricesModel.FixedPrices) {
				var fixedPriceViewModel = new FixedPriceItemViewModel(fixedPrice, fixedPricesModel);
				FixedPrices.Add(fixedPriceViewModel);
			}

			fixedPricesModel.FixedPrices.ElementAdded += FixedPricesOnElementAdded;
			fixedPricesModel.FixedPrices.ElementRemoved += FixedPricesOnElementRemoved;
		}

		private void UpdateNomenclatures()
		{
			FixedPriceNomenclatures.Clear();
			foreach(var fixedPrice in FixedPrices)
			{
				if (!FixedPriceNomenclatures.Contains(fixedPrice.NomenclatureFixedPrice.Nomenclature))
				{
					FixedPriceNomenclatures.Add(fixedPrice.NomenclatureFixedPrice.Nomenclature);
				}
			}
		}

		private void UpdateFixedPricesByNomenclature()
		{
			FixedPricesByNomenclature.Clear();
			foreach(var fixedPrice in FixedPrices)
			{
				if(fixedPrice.NomenclatureFixedPrice.Nomenclature == SelectedNomenclature)
				{
					FixedPricesByNomenclature.Add(fixedPrice);
				}
			}
		}

		private void FixedPricesOnElementAdded(object alist, int[] aidx)
		{
			foreach (var index in aidx) {
				var addedFixedPrice = fixedPricesModel.FixedPrices[index];
				var addedFixedPriceViewModel = new FixedPriceItemViewModel(addedFixedPrice, fixedPricesModel);
				if (FixedPrices.All(x => x.NomenclatureFixedPrice != addedFixedPrice)) {
					FixedPrices.Add(addedFixedPriceViewModel);
				}
			}
		}
		
		private void FixedPricesOnElementRemoved(object alist, int[] aidx, object aobject)
		{
			var removedFixedPrice = (NomenclatureFixedPrice)aobject;
			var viewModelToRemove = FixedPrices.FirstOrDefault(x => x.NomenclatureFixedPrice == removedFixedPrice);
			if (viewModelToRemove != null) {
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

		private FixedPriceItemViewModel selectedFixedPrice;
		public virtual FixedPriceItemViewModel SelectedFixedPrice {
			get => selectedFixedPrice;
			set {
				if (SetField(ref selectedFixedPrice, value)) {
					UpdateFixedPriceHistory();
				}
			}
		}

		private IList<FieldChange> selectedPriceChanges;
		public virtual IList<FieldChange> SelectedPriceChanges {
			get => selectedPriceChanges;
			set => SetField(ref selectedPriceChanges, value);
		}

		#region Commands

		private DelegateCommand addFixedPriceCommand;
		public DelegateCommand AddFixedPriceCommand {
			get {
				if(addFixedPriceCommand == null) {
					addFixedPriceCommand = new DelegateCommand(() => SelectWaterNomenclature(), () => CanEdit);
					addFixedPriceCommand.CanExecuteChangedWith(this, x => x.CanEdit);
				}
				return addFixedPriceCommand;
			}
		}

		private void SelectWaterNomenclature()
		{
			var waterJournalFactory = nomenclatureSelectorFactory.GetWaterJournalFactory();
			var selector = waterJournalFactory.CreateAutocompleteSelector();
			selector.OnEntitySelectedResult += OnWaterSelected;
			parentTab.TabParent.AddSlaveTab(parentTab, selector);
		}

		private void OnWaterSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedWaterNode = e.SelectedNodes.FirstOrDefault();
			if(selectedWaterNode == null) {
				return;
			}
			Nomenclature waterNomenclature = UoW.GetById<Nomenclature>(selectedWaterNode.Id);
			AddFixedPrice(waterNomenclature);
		}

		private void AddFixedPrice(Nomenclature nomenclature)
		{
			decimal price = 0;
			if(nomenclature.DependsOnNomenclature != null) {
				var fixPrice = FixedPrices.FirstOrDefault(p => p.NomenclatureFixedPrice.Nomenclature.Id == nomenclature.DependsOnNomenclature.Id);
				price = fixPrice == null ? 0 : fixPrice.FixedPrice;
			}

			fixedPricesModel.AddOrUpdateFixedPrice(nomenclature, price, 0);
		}

		private DelegateCommand removeFixedPriceCommand;
		public DelegateCommand RemoveFixedPriceCommand {
			get {
				if(removeFixedPriceCommand == null) {
					removeFixedPriceCommand = new DelegateCommand(RemoveFixedPrice,
						() => CanEdit && SelectedFixedPrice != null
					);
					removeFixedPriceCommand.CanExecuteChangedWith(this, x => x.CanEdit, x => x.SelectedFixedPrice);
				}
				return removeFixedPriceCommand;
			}
		}

		private void RemoveFixedPrice()
		{
			if(SelectedFixedPrice == null) {
				return;
			}
			
			fixedPricesModel.RemoveFixedPrice(SelectedFixedPrice.NomenclatureFixedPrice);
		}

		#endregion

		private void UpdateFixedPriceHistory()
		{
			IList<FieldChange> fixedPricesChanges = new List<FieldChange>();
			if(SelectedFixedPrice != null) {
				fixedPricesChanges = HistoryChangesRepository
					.GetFieldChanges<NomenclatureFixedPrice>(UoW, new[] { SelectedFixedPrice.NomenclatureFixedPrice.Id }, x => x.Price)
					.OrderBy(x => x.Entity.ChangeTime)
					.ToList();
				foreach(var change in fixedPricesChanges) {
					change.DiffFormatter = DiffFormatter;
				}
			}
			SelectedPriceChanges = fixedPricesChanges;
		}
	}
}
