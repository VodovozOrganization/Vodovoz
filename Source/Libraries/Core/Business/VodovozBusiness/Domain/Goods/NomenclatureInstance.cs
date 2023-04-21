using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	public abstract class NomenclatureInstance : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private decimal _purchasePrice;
		private decimal _costPrice;
		private decimal _innerDeliveryPrice;
		private DateTime? _creationDate;
		private Nomenclature _nomenclature;
		
		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}
		
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}
		
		[Display(Name = "Цена закупки")]
		public virtual decimal PurchasePrice
		{
			get => _purchasePrice;
			set => SetField(ref _purchasePrice, value);
		}
		
		[Display(Name = "Себестоимость")]
		public virtual decimal CostPrice
		{
			get => _costPrice;
			set => SetField(ref _costPrice, value);
		}
		
		[Display(Name = "Стоимость доставки на склад")]
		public virtual decimal InnerDeliveryPrice
		{
			get => _innerDeliveryPrice;
			set => SetField(ref _innerDeliveryPrice, value);
		}

		public abstract NomenclatureInstanceType Type { get; }
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Nomenclature == null)
			{
				yield return new ValidationResult("Необходимо выбрать номенклатуру");
			}
		}
	}

	public enum NomenclatureInstanceType
	{
		InventoryNomenclatureInstance
	}
}
