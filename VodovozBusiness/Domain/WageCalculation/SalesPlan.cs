using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
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
		private IList<NomenclatureSalesPlanItem> _nomenclatureItemSalesPlans = new List<NomenclatureSalesPlanItem>();
		private IList<EquipmentKindSalesPlanItem> _equipmentKindItemSalesPlans = new List<EquipmentKindSalesPlanItem>();
		private IList<EquipmentTypeSalesPlanItem> _equipmentTypeItemSalesPlans = new List<EquipmentTypeSalesPlanItem>();
		private GenericObservableList<NomenclatureSalesPlanItem> _observableNomenclatureItemSalesPlan;
		private GenericObservableList<EquipmentKindSalesPlanItem> _observableEquipmentKindItemSalesPlans;
		private GenericObservableList<EquipmentTypeSalesPlanItem> _observableEquipmentTypeItemSalesPlans;
		private decimal _proceedsDay;
		private decimal _proceedsMonth;
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

		[Display(Name = "Выручка за день")]
		public virtual decimal ProceedsDay
		{
			get => _proceedsDay;
			set => SetField(ref _proceedsDay, value);
		}

		[Display(Name = "Выручка за месяц")]
		public virtual decimal ProceedsMonth
		{
			get => _proceedsMonth;
			set => SetField(ref _proceedsMonth, value);
		}

		public virtual string Title {
			get {
				return string.Format(
					"{3}: продажа - {1} бут., забор - {2} бут. (№{0})",
					Id,
					FullBottleToSell,
					EmptyBottlesToTake,
					Name
				);
			}
		}

		[Display(Name = "Номенклатуры")]
		public virtual IList<NomenclatureSalesPlanItem> NomenclatureItemSalesPlans
		{
			get => _nomenclatureItemSalesPlans;
			set => SetField(ref _nomenclatureItemSalesPlans, value);
		}

		[Display(Name = "Виды оборудования")]
		public virtual IList<EquipmentKindSalesPlanItem> EquipmentKindItemSalesPlans
		{
			get => _equipmentKindItemSalesPlans;
			set => SetField(ref _equipmentKindItemSalesPlans, value);
		}

		[Display(Name = "Типы оборудования")]
		public virtual IList<EquipmentTypeSalesPlanItem> EquipmentTypeItemSalesPlans
		{
			get => _equipmentTypeItemSalesPlans;
			set => SetField(ref _equipmentTypeItemSalesPlans, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.

		public virtual GenericObservableList<NomenclatureSalesPlanItem> ObservableNomenclatureItemSalesPlans => 
			_observableNomenclatureItemSalesPlan ?? 
			(_observableNomenclatureItemSalesPlan = new GenericObservableList<NomenclatureSalesPlanItem>(NomenclatureItemSalesPlans));
		public virtual GenericObservableList<EquipmentKindSalesPlanItem> ObservableEquipmentKindItemSalesPlans => 
			_observableEquipmentKindItemSalesPlans ?? (_observableEquipmentKindItemSalesPlans = new GenericObservableList<EquipmentKindSalesPlanItem>(EquipmentKindItemSalesPlans));
		public virtual GenericObservableList<EquipmentTypeSalesPlanItem> ObservableEquipmentTypeItemSalesPlans =>
			_observableEquipmentTypeItemSalesPlans ?? (_observableEquipmentTypeItemSalesPlans = new GenericObservableList<EquipmentTypeSalesPlanItem>(EquipmentTypeItemSalesPlans));

		public virtual void AddNomenclatureItem(NomenclatureSalesPlanItem nomenclatureSalesPlanItem)
		{
			if(!ObservableNomenclatureItemSalesPlans.Any(x => x.Nomenclature == nomenclatureSalesPlanItem.Nomenclature))
			{
				ObservableNomenclatureItemSalesPlans.Add(nomenclatureSalesPlanItem);
			}
		}

		public virtual void RemoveNomenclatureItem(NomenclatureSalesPlanItem nomenclatureSalesPlanItem)
		{
			if(ObservableNomenclatureItemSalesPlans.Contains(nomenclatureSalesPlanItem))
			{
				ObservableNomenclatureItemSalesPlans.Remove(nomenclatureSalesPlanItem);
			}
		}

		public virtual void AddEquipmentKind(EquipmentKindSalesPlanItem equipmentKindSalesPlanItem)
		{
			if(!ObservableEquipmentKindItemSalesPlans.Any(x => x.EquipmentKind == equipmentKindSalesPlanItem.EquipmentKind))
			{
				ObservableEquipmentKindItemSalesPlans.Add(equipmentKindSalesPlanItem);
			}
		}

		public virtual void RemoveEquipmentKindItem(EquipmentKindSalesPlanItem equipmentKindSalesPlanItem)
		{
			if(ObservableEquipmentKindItemSalesPlans.Contains(equipmentKindSalesPlanItem))
			{
				ObservableEquipmentKindItemSalesPlans.Remove(equipmentKindSalesPlanItem);
			}
		}

		public virtual void AddEquipmentType(EquipmentTypeSalesPlanItem equipmentTypeSalesPlanItem)
		{
			if(ObservableEquipmentTypeItemSalesPlans.Select(x => x.EquipmentType).Contains(equipmentTypeSalesPlanItem.EquipmentType))
			{
				return;
			}

			ObservableEquipmentTypeItemSalesPlans.Add(equipmentTypeSalesPlanItem);
		}

		public virtual void RemoveEquipmentTypeItem(EquipmentTypeSalesPlanItem equipmentTypeSalesPlanItem)
		{
			if(ObservableEquipmentTypeItemSalesPlans.Contains(equipmentTypeSalesPlanItem))
			{
				ObservableEquipmentTypeItemSalesPlans.Remove(equipmentTypeSalesPlanItem);
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(FullBottleToSell <= 0)
			{
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для продажи",
					new[] { this.GetPropertyName(o => o.FullBottleToSell) }
				);
			}

			if(EmptyBottlesToTake <= 0)
			{
				yield return new ValidationResult(
					"Должно быть указано планируемое количество бутылей для забора",
					new[] { this.GetPropertyName(o => o.EmptyBottlesToTake) }
				);
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Должно быть указано название",
					new[] { nameof(Name) }
				);
			}

			if(Name?.Length > 50)
			{
				yield return new ValidationResult(
					$"Превышена максимально допустимая длина названия ({Name.Length}/50).",
					new[] { nameof(Name) }
					);
			}
		}

		#endregion IValidatableObject implementation
	}
}
