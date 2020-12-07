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
		private readonly INomenclatureSelectorFactory nomenclatureSelectorFactory;
		private readonly ITdiTab parentTab;

		public FixedPricesViewModel(IUnitOfWork uow, IFixedPricesModel fixedPricesModel, INomenclatureSelectorFactory nomenclatureSelectorFactory, ITdiTab parentTab)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			this.fixedPricesModel = fixedPricesModel ?? throw new ArgumentNullException(nameof(fixedPricesModel));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));

			fixedPricesModel.FixedPricesUpdated += (sender, args) => UpdateFixedPrices();
			UpdateFixedPrices();
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

			fixedPricesModel.AddOrUpdateFixedPrice(nomenclature, price);
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
