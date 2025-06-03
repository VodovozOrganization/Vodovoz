using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "цены закупки ТМЦ",
		Nominative = "цена закупки ТМЦ",
		Accusative = "цену закупки ТМЦ",
		Genitive = "цену закупки ТМЦ"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class NomenclaturePurchasePrice : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const decimal _purchasePriceMax = 999999;
		private int _id;
		private NomenclatureEntity _nomenclature;
		private DateTime _startDate;
		private DateTime? _endDate;
		private decimal _purchasePrice;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
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

			if(PurchasePrice > _purchasePriceMax)
			{
				yield return new ValidationResult(
					$"Цена закупки не может быть больше {_purchasePriceMax}", new[] { nameof(PurchasePrice) });
			}

			if(Nomenclature == null)
			{
				yield return new ValidationResult("Номенклатура должна быть указана", new[] { nameof(Nomenclature) });
			}
		}
	}
}
