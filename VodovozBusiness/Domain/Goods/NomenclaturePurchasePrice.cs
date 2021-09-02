using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
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
	public class NomenclaturePurchasePrice : PropertyChangedBase, IDomainObject
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
	}
}