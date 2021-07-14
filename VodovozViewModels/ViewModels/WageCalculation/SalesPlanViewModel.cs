using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class SalesPlanViewModel : EntityTabViewModelBase<SalesPlan>
	{
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory;
		private DelegateCommand _addNomenclatureCommand;
		private DelegateCommand _addEquipmentKindCommand;
		private DelegateCommand _addEquipmentTypeCommand;

		public SalesPlanViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INomenclatureSelectorFactory nomenclatureSelectorFactory) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			//var f= Enum.GetValues(typeof(EquipmentType));
		}

		public Array EquipmentTypes => Enum.GetValues(typeof(EquipmentType));
		public EquipmentType EquipmentType { get; set; }

		/*	#region AttachNomenclatureCommand

			public DelegateCommand AttachNomenclatureCommand { get; private set; }

			private void CreateAttachNomenclatureCommand()
			{
				AttachNomenclatureCommand = new DelegateCommand(
					() => {
						var nomenclatureFilter = new NomenclatureFilterViewModel();
						var nomenclatureJournalViewModel = new NomenclaturesJournalViewModel(
							nomenclatureFilter,
							_unitOfWorkFactory,
							_commonServices,
							_employeeSelectorFactory
						);
						nomenclatureJournalViewModel.SelectionMode = JournalSelectionMode.Single;
						nomenclatureJournalViewModel.OnEntitySelectedResult += (sender, e) => {
							var selectedNode = e.SelectedNodes.FirstOrDefault();
							if(selectedNode == null)
							{
								return;
							}
							Entity.AddNomenclature(UoW.GetById<Nomenclature>(selectedNode.Id));
						};
						TabParent.AddSlaveTab(this, nomenclatureJournalViewModel);
					},
					() => true
				);
			}

			#endregion AttachNomenclatureCommand*/


		public DelegateCommand AddNomenclatureCommand =>
			_addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(() =>
				{
					var nomenclatureSelector = _nomenclatureSelectorFactory.CreateNomenclatureSelector();
					nomenclatureSelector.OnEntitySelectedResult += (sender, e) =>
					{
						//foreach(var nomenclature in UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id)))
						//{
						//	Entity.AddNomenclature(nomenclature);
						//}
						foreach(var nomenclature in UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id)))
						{
							Entity.AddNomenclature(new NomenclatureItemSalesPlan(){Nomenclature = nomenclature, SalesPlan = Entity});
						}
					};
					TabParent.AddSlaveTab(this, nomenclatureSelector);
				},
				() => true
			));

		public DelegateCommand AddEquipmentTypeCommand =>
			_addEquipmentTypeCommand ?? (_addEquipmentTypeCommand = new DelegateCommand(() =>
				{
					//Entity.AddEquipmentType(EquipmentType);
					Entity.AddEquipmentType(new EquipmentTypeItemSalesPlan(){EquipmentType = EquipmentType, SalesPlan = Entity});
				},
				() => true
			));
		

		/*public DelegateCommand AddEquipmentKindCommand =>
			_addEquipmentKindCommand ?? (_addEquipmentKindCommand = new DelegateCommand(() =>
				{
					var equipmentKindSelector = _equipmentKindSelectorFactory.CreateEquipmentKindSelector();
					equipmentKindSelector.OnEntitySelectedResult += (sender, e) =>
					{
						foreach(var equipmentKind in UoW.GetById<EquipmentKind>(e.SelectedNodes.Select(x => x.Id)))
						{
							Entity.AddEquipmentKind(equipmentKind);
						}
					};
					TabParent.AddSlaveTab(this, equipmentKindSelector);
				},
				() => true
			));

		public DelegateCommand AddNomenclatureCommand =>
			_addNomenclatureCommand ?? (_addNomenclatureCommand = new DelegateCommand(() =>
				{
					var nomenclatureSelector = _nomenclatureSelectorFactory.CreateNomenclatureSelector();
					nomenclatureSelector.OnEntitySelectedResult += (sender, e) =>
					{
						foreach(var nomenclature in UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id)))
						{
							Entity.AddNomenclature(nomenclature);
						}
					};
					TabParent.AddSlaveTab(this, nomenclatureSelector);
				},
				() => true
			));*/

	}
}