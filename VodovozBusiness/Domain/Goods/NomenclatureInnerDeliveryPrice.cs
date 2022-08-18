using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "стоимости доставки ТМЦ на склад",
		Nominative = "стоимость доставки ТМЦ на склад",
		Accusative = "стоимости доставки ТМЦ на склад",
		Genitive = "стоимости доставки ТМЦ на склад"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class NomenclatureInnerDeliveryPrice : PropertyChangedBase, IDomainObject, IValidatableObject
	{

		private Nomenclature _nomenclature;
		private DateTime _startDate;
		private DateTime? _endDate;
		private decimal _price;

		public virtual int Id { get; set; }

		[Display(Name = "Дата начала")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Стоимость доставки")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Price <= 0)
			{
				yield return new ValidationResult("Должна быть указана цена закупки", new[] { nameof(Price) });
			}

			if(Nomenclature == null)
			{
				yield return new ValidationResult("Номенклатура должна быть указана", new[] { nameof(Nomenclature) });
			}
		}
	}
}
