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
		NominativePlural = "цены закупки/себестоимости ТМЦ",
		Nominative = "цена закупки/себестоимости ТМЦ",
		Accusative = "цену закупки/себестоимости ТМЦ",
		Genitive = "цену закупки/себестоимости ТМЦ"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class NomenclatureCostPurchasePrice : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private Nomenclature _nomenclature;
		private DateTime _startDate;
		private DateTime? _endDate;
		private decimal _purchasePrice;

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
		public virtual decimal PurchasePrice
		{
			get => _purchasePrice;
			set => SetField(ref _purchasePrice, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(PurchasePrice <= 0)
			{
				yield return new ValidationResult("Должна быть указана цена закупки", new[] { nameof(PurchasePrice) });
			}
		}
	}
}
