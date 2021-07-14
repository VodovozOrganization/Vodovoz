using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.WageCalculation
{
	[
		Appellative(
			Gender = GrammaticalGender.Feminine,
			NominativePlural = "планы продаж",
			Nominative = "план продаж"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class SalesPlan : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		//private IList<Nomenclature> _nomenclatures = new List<Nomenclature>();
		private IList<NomenclatureItemSalesPlan> _nomenclatureItemSalesPlans = new List<NomenclatureItemSalesPlan>();
		private IList<EquipmentKind> _equipmentKinds = new List<EquipmentKind>();
		//private IList<EquipmentType> _equipmentTypes = new List<EquipmentType>();
		private IList<EquipmentTypeItemSalesPlan> _equipmentTypeItemSalesPlans = new List<EquipmentTypeItemSalesPlan>();
		//private GenericObservableList<Nomenclature> _observableNomenclatures;
		private GenericObservableList<NomenclatureItemSalesPlan> _observableNomenclatureItemSalesPlan;
		private GenericObservableList<EquipmentKind> _observableEquipmentKinds;
		//private GenericObservableList<EquipmentType> _observableEquipmentTypes;
		private GenericObservableList<EquipmentTypeItemSalesPlan> _observableEquipmentTypeItemSalesPlans;
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		bool isArchive;
		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		int fullBottlesToSell;
		[Display(Name = "Кол-во полных бутылей для продажи")]
		public virtual int FullBottleToSell {
			get => fullBottlesToSell;
			set => SetField(ref fullBottlesToSell, value);
		}

		int emptyBottlesToTake;
		[Display(Name = "Кол-во пустых бутылей для забора")]
		public virtual int EmptyBottlesToTake {
			get => emptyBottlesToTake;
			set => SetField(ref emptyBottlesToTake, value);
		}

		public virtual string Title {
			get {
				return string.Format(
					"продажа - {1} бут., забор - {2} бут. (№{0})",
					Id,
					FullBottleToSell,
					EmptyBottlesToTake
				);
			}
		}

		//[Display(Name = "Номенклатуры")]
		//public virtual IList<Nomenclature> Nomenclatures
		//{
		//	get => _nomenclatures;
		//	set => SetField(ref _nomenclatures, value);
		//}

		[Display(Name = "Номенклатуры")]
		public virtual IList<NomenclatureItemSalesPlan> NomenclatureItemSalesPlans
		{
			get => _nomenclatureItemSalesPlans;
			set => SetField(ref _nomenclatureItemSalesPlans, value);
		}

		[Display(Name = "Виды оборудования")]
		public virtual IList<EquipmentKind> EquipmentKinds
		{
			get => _equipmentKinds;
			set => SetField(ref _equipmentKinds, value);
		}

		//[Display(Name = "Типы оборудования")]
		//public virtual IList<EquipmentType> EquipmentTypes
		//{
		//	get => _equipmentTypes;
		//	set => SetField(ref _equipmentTypes, value);
		//}

		[Display(Name = "Типы оборудования")]
		public virtual IList<EquipmentTypeItemSalesPlan> EquipmentTypeItemSalesPlans
		{
			get => _equipmentTypeItemSalesPlans;
			set => SetField(ref _equipmentTypeItemSalesPlans, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.

		public virtual GenericObservableList<NomenclatureItemSalesPlan> ObservableNomenclatureItemSalesPlans => 
			_observableNomenclatureItemSalesPlan ?? 
			(_observableNomenclatureItemSalesPlan = new GenericObservableList<NomenclatureItemSalesPlan>(NomenclatureItemSalesPlans));
		public virtual GenericObservableList<EquipmentKind> ObservableEquipmentKinds => 
			_observableEquipmentKinds ?? (_observableEquipmentKinds = new GenericObservableList<EquipmentKind>(EquipmentKinds));
		public virtual GenericObservableList<EquipmentTypeItemSalesPlan> ObservableEquipmetTypeItemSalesPlans =>
			_observableEquipmentTypeItemSalesPlans ?? (_observableEquipmentTypeItemSalesPlans = new GenericObservableList<EquipmentTypeItemSalesPlan>(EquipmentTypeItemSalesPlans));

		//public virtual void AddNomenclature(Nomenclature nomenclature)
		//{
		//	if(ObservableNomenclatures.Contains(nomenclature))
		//	{
		//		return;
		//	}

		//	ObservableNomenclatures.Add(nomenclature);
		//}

		public virtual void AddNomenclature(NomenclatureItemSalesPlan nomenclature)
		{
			if(ObservableNomenclatureItemSalesPlans.Contains(nomenclature))
			{
				return;
			}

			ObservableNomenclatureItemSalesPlans.Add(nomenclature);
		}

		public virtual void AddEquipmentKind(EquipmentKind equipmentKind)
		{
			if(ObservableEquipmentKinds.Contains(equipmentKind))
			{
				return;
			}

			ObservableEquipmentKinds.Add(equipmentKind);
		}

		public virtual void AddEquipmentType(EquipmentTypeItemSalesPlan equipmentTypeItemSalesPlan)
		{
			if(ObservableEquipmetTypeItemSalesPlans.Contains(equipmentTypeItemSalesPlan))
			{
				return;
			}

			ObservableEquipmetTypeItemSalesPlans.Add(equipmentTypeItemSalesPlan);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FullBottleToSell <= 0)
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для продажи",
					new[] { this.GetPropertyName(o => o.FullBottleToSell) }
				);

			if(EmptyBottlesToTake <= 0)
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для забора",
					new[] { this.GetPropertyName(o => o.EmptyBottlesToTake) }
				);
		}

		#endregion IValidatableObject implementation
	}
}
