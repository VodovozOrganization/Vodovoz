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
		NominativePlural = "цены себестоимости ТМЦ",
		Nominative = "цена себестоимости ТМЦ",
		Accusative = "цену себестоимости ТМЦ",
		Genitive = "цену себестоимости ТМЦ"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class NomenclatureCostPrice : PropertyChangedBase, IDomainObject, IValidatableObject
	{

		private Nomenclature _nomenclature;
		private DateTime _startDate;
		private DateTime? _endDate;
		private decimal _costPrice;

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

		[Display(Name = "Цена закупки")]
		public virtual decimal CostPrice
		{
			get => _costPrice;
			set => SetField(ref _costPrice, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(CostPrice <= 0)
			{
				yield return new ValidationResult("Должна быть указана цена себестоимости", new[] { nameof(CostPrice) });
			}

			if(Nomenclature == null)
			{
				yield return new ValidationResult("Номенклатура должна быть указана", new[] { nameof(Nomenclature) });
			}
		}
	}
}
