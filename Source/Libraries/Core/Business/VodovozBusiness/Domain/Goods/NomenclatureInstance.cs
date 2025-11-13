using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Domain.Goods
{
	public abstract class NomenclatureInstance : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private decimal _purchasePrice;
		private DateTime _creationDate;
		private Nomenclature _nomenclature;
		
		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
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
		
		public abstract NomenclatureInstanceType Type { get; }
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Nomenclature is null)
			{
				yield return new ValidationResult("Необходимо выбрать номенклатуру");
			}
		}
	}
}
