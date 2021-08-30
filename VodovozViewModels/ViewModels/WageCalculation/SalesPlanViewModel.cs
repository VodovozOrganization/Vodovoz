using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Journal.Actions.ViewModels;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class SalesPlanViewModel : EntityTabViewModelBase<SalesPlan>
	{
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;
		private DelegateCommand _addNomenclatureItemCommand;
		private DelegateCommand<NomenclatureSalesPlanItem> _removeNomenclatureItemCommand;
		private DelegateCommand _addEquipmentKindItemCommand;
		private DelegateCommand<EquipmentKindSalesPlanItem> _removeEquipmentKindItemCommand;
		private DelegateCommand _addEquipmentTypeItemCommand;
		private DelegateCommand<EquipmentTypeSalesPlanItem> _removeEquipmentTypeItemCommand;


		public SalesPlanViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INomenclatureSelectorFactory nomenclatureSelectorFactory) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
		}

		public Array EquipmentTypes => Enum.GetValues(typeof(EquipmentType));
		public EquipmentType EquipmentType { get; set; }

		#region Commands

		public DelegateCommand AddNomenclatureItemCommand =>
			_addNomenclatureItemCommand ?? (_addNomenclatureItemCommand = new DelegateCommand(() =>
				{
					var nomenclatureSelector = _nomenclatureSelectorFactory.CreateNomenclatureSelector();
					nomenclatureSelector.OnEntitySelectedResult += (sender, e) =>
					{
						foreach(var nomenclature in UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id)))
						{
							Entity.AddNomenclatureItem(new NomenclatureSalesPlanItem() { Nomenclature = nomenclature, SalesPlan = Entity });
						}
					};
					TabParent.AddSlaveTab(this, nomenclatureSelector);
				},
				() => true
			));

		public DelegateCommand<NomenclatureSalesPlanItem> RemoveNomenclatureItemCommand => _removeNomenclatureItemCommand ?? (_removeNomenclatureItemCommand =
			new DelegateCommand<NomenclatureSalesPlanItem>((nomenclatureItem) =>
				{
					Entity.RemoveNomenclatureItem(nomenclatureItem);
				},
				(nomenclatureItem) => true
			));

		public DelegateCommand AddEquipmentTypeItemCommand =>
			_addEquipmentTypeItemCommand ?? (_addEquipmentTypeItemCommand = new DelegateCommand(() =>
				{
					Entity.AddEquipmentType(new EquipmentTypeSalesPlanItem() { EquipmentType = EquipmentType, SalesPlan = Entity });
				},
				() => true
			));

		public DelegateCommand<EquipmentTypeSalesPlanItem> RemoveEquipmentTypeItemCommand => _removeEquipmentTypeItemCommand ?? (_removeEquipmentTypeItemCommand =
			new DelegateCommand<EquipmentTypeSalesPlanItem>((equipmentTypeItem) =>
				{
					Entity.RemoveEquipmentTypeItem(equipmentTypeItem);
				},
				(equipmentTypeItem) => true
			));

		public DelegateCommand AddEquipmentKindItemCommand =>
			_addEquipmentKindItemCommand ?? (_addEquipmentKindItemCommand = new DelegateCommand(() =>
				{
					var equipmentKindSelector = new EquipmentKindJournalViewModel(
						new EntitiesJournalActionsViewModel(CommonServices.InteractiveService), UnitOfWorkFactory, CommonServices)
					{
						SelectionMode = JournalSelectionMode.Multiple
					};

					equipmentKindSelector.OnEntitySelectedResult += (sender, e) =>
					{
						foreach(var equipmentKind in UoW.GetById<EquipmentKind>(e.SelectedNodes.Select(x => x.Id)))
						{
							Entity.AddEquipmentKind(new EquipmentKindSalesPlanItem() { EquipmentKind = equipmentKind, SalesPlan = Entity });
						}
					};

					TabParent.AddSlaveTab(this, equipmentKindSelector);
				},
				() => true
			));

		public DelegateCommand<EquipmentKindSalesPlanItem> RemoveEquipmentKindItemCommand => _removeEquipmentKindItemCommand ?? (_removeEquipmentKindItemCommand =
			new DelegateCommand<EquipmentKindSalesPlanItem>((equipmentKindItem) =>
				{
					Entity.RemoveEquipmentKindItem(equipmentKindItem);
				},
				(equipmentKindItem) => true
			));


		#endregion
	}
}