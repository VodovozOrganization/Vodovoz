using System;
using System.Collections.Generic;

namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Информация о товаре
	/// </summary>
	public class ProductInfo
	{
		/// <summary>
		/// Название товара
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Услуга
		/// </summary>
		public bool IsService { get; set; }
		/// <summary>
		/// Ед измерения
		/// </summary>
		public string UnitName { get; set; }
		/// <summary>
		/// Код ОКЕИ
		/// </summary>
		public string OKEI { get; set; }
		/// <summary>
		/// Код продукта
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Включая НДС
		/// </summary>
		public decimal IncludeVat { get; set; }
		/// <summary>
		/// Налоговая ставка
		/// </summary>
		public decimal? ValueAddedTax { get; set; }
		/// <summary>
		/// Скидка в рублях
		/// </summary>
		public virtual decimal DiscountMoney { get; set; }
		/// <summary>
		/// Коды маркировки для Честного знака
		/// </summary>
		public IEnumerable<ProductCodeInfo> TrueMarkCodes { get; set; }

		/// <summary>
		/// Информационные поля фактов хозяйственной жизни товара (ИнфПолФХЖ2)
		/// </summary>
		public IEnumerable<ProductEconomicLifeFactsInfo> EconomicLifeFacts { get; set; }

		public decimal Sum => Math.Round(Price * Count - DiscountMoney, 2);
		public decimal SumWithoutVat => Math.Round(Price * Count - IncludeVat - DiscountMoney, 2);
		public decimal PriceWithoutVat =>
			Count == 0
				? 0
				: Math.Round((Price * Count - IncludeVat - DiscountMoney) / Count, 2);
	}
}
