using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo.Upd
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "Товары",
		Nominative = "Товар")]
	public class UpdInventPosition : PropertyChangedBase, IDomainObject
	{

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id { get; set; }

		/// <summary>
		/// Назначен на
		/// </summary>
		[Display(Name = "Назначен на")]
		public virtual OrderItemEntity AssignedOrderItem { get; set; }

		/// <summary>
		/// Название товара
		/// </summary>
		[Display(Name = "Название товара")]
		public virtual string Name { get; set; }

		/// <summary>
		/// Услуга
		/// </summary>
		[Display(Name = "Услуга")]
		public virtual bool IsService { get; set; }

		/// <summary>
		/// Ед измерения
		/// </summary>
		[Display(Name = "Ед измерения")]
		public virtual string UnitName { get; set; }

		/// <summary>
		/// Код ОКЕИ
		/// </summary>
		[Display(Name = "Код ОКЕИ")]
		public virtual string OKEI { get; set; }

		/// <summary>
		/// Код продукта
		/// </summary>
		[Display(Name = "Код продукта")]
		public virtual int NomenclatureId { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Count { get; set; }

		/// <summary>
		/// Цена
		/// </summary>
		[Display(Name = "Цена")]
		public virtual decimal Price { get; set; }

		/// <summary>
		/// Включая НДС
		/// </summary>
		[Display(Name = "Включая НДС")]
		public virtual decimal IncludeVat { get; set; }

		/// <summary>
		/// Налоговая ставка
		/// </summary>
		[Display(Name = "Налоговая ставка")]
		public virtual decimal? ValueAddedTax { get; set; }

		/// <summary>
		/// Скидка в рублях
		/// </summary>
		[Display(Name = "Скидка в рублях")]
		public virtual decimal DiscountMoney { get; set; }

		/// <summary>
		/// Код ТМЦ в системе покупателя
		/// </summary>
		[Display(Name = "Код ТМЦ в системе покупателя")]
		public virtual int? SpecialCustomerNomenclatureId { get; set; }

		/// <summary>
		/// Коды маркировки
		/// </summary>
		[Display(Name = "Коды маркировки")]
		public virtual IList<UpdProductCode> Codes { get; set; } = new List<UpdProductCode>();

		#region Рассчетные

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);

		#endregion
	}
}
