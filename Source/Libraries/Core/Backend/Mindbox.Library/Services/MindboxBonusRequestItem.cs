namespace Mindbox.Library.Services
{
	/// <summary>
	/// Позиция для расчёта бонусов в Mindbox
	/// </summary>
	public sealed class MindboxBonusRequestItem
	{
		public MindboxBonusRequestItem(string itemKey, decimal basePrice)
		{
			ItemKey = itemKey;
			BasePrice = basePrice;
		}

		/// <summary>
		/// Ключ товара в Mindbox
		/// </summary>
		public string ItemKey { get; }

		/// <summary>
		/// Базовая цена за 1 шт.
		/// </summary>
		public decimal BasePrice { get; }
	}
}
