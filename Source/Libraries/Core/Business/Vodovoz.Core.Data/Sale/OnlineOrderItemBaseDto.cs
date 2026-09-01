using System;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.Sale
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public abstract class OnlineOrderItemBaseDto
	{
		/// <summary>
		/// Id заказываемой позиции в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Тип заказываемой позиции
		/// </summary>
		public SaleItemType ItemType { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		public decimal CurrentPrice { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма со скидкой
		/// </summary>
		public decimal CurrentSum { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		/// <summary>
		/// Добавление фиксы
		/// </summary>
		/// <param name="fixedPrice">Фикса</param>
		public virtual void AddFixedPrice(decimal fixedPrice)
		{
			if(PriceWithoutDiscount is null)
			{
				PriceWithoutDiscount = fixedPrice;
			}
			
			Price = fixedPrice;
			CurrentPrice = fixedPrice;
			CurrentSum = Math.Round(CurrentPrice * Count, 2);
			IsFixedPrice = true;
		}
	}
}
