using System;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Транзакция выдачи топлива
	/// </summary>
	public class TransactionDto
	{
		/// <summary>
		/// ID транзакции
		/// </summary>
		[JsonPropertyName("id")]
		public long Id { get; set; }

		/// <summary>
		/// Дата время транзакции
		/// </summary>
		[JsonPropertyName("timestamp")]
		public DateTime TransactionDate { get; set; }

		/// <summary>
		/// ID карты
		/// </summary>
		[JsonPropertyName("card_id")]
		public string CardId { get; set; }

		/// <summary>
		/// ID точки обслуживания
		/// </summary>
		[JsonPropertyName("poi_id")]
		public string SalePointId { get; set; }

		/// <summary>
		/// ID терминала
		/// </summary>
		[JsonPropertyName("terminal_id")]
		public string TerminalId { get; set; }

		/// <summary>
		/// ID типа транзакции
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }

		/// <summary>
		/// ID продукта
		/// </summary>
		[JsonPropertyName("product_id")]
		public string ProductId { get; set; }

		/// <summary>
		/// ID категории продукта
		/// </summary>
		[JsonPropertyName("product_category_id")]
		public string ProductCategoryId { get; set; }

		/// <summary>
		/// ID валюты
		/// </summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		/// <summary>
		/// ID чека
		/// </summary>
		[JsonPropertyName("check_id")]
		public long CheckId { get; set; }

		/// <summary>
		/// ID прямой транзакции
		/// Тип точно не известен. В документации указан string, иногда прилетает long, в остальных случаях null
		/// </summary>
		[JsonPropertyName("stor_transaction_id")]
		public object StorTransactionId { get; set; }

		/// <summary>
		/// Признак сторнирования
		/// </summary>
		[JsonPropertyName("is_storno")]
		public bool IsStorno { get; set; }

		/// <summary>
		/// Признак ручной корректировки
		/// </summary>
		[JsonPropertyName("is_manual_corrention")]
		public bool IsManualCorrention { get; set; }

		/// <summary>
		/// Количество единиц товара
		/// </summary>
		[JsonPropertyName("qty")]
		public decimal Quantity { get; set; }

		/// <summary>
		/// Цена со скидкой клиента
		/// </summary>
		[JsonPropertyName("price")]
		public decimal Price { get; set; }

		/// <summary>
		/// Цена без скидки
		/// </summary>
		[JsonPropertyName("price_no_discount")]
		public decimal PriceNoDiscount { get; set; }

		/// <summary>
		/// Сумма со скидкой клиента
		/// </summary>
		[JsonPropertyName("sum")]
		public decimal Sum { get; set; }

		/// <summary>
		/// Сумма без скидки
		/// </summary>
		[JsonPropertyName("sum_no_discount")]
		public decimal SumNoDiscount { get; set; }

		/// <summary>
		/// Сумма скидки
		/// </summary>
		[JsonPropertyName("discount")]
		public decimal Discount { get; set; }

		/// <summary>
		/// Курс пересчёта
		/// </summary>
		[JsonPropertyName("exchange_rate")]
		public long ExchangeRate { get; set; }

		/// <summary>
		/// Номер карты
		/// </summary>
		[JsonPropertyName("card_number")]
		public string CardNumber { get; set; }

		/// <summary>
		/// Тип платежа
		/// </summary>
		[JsonPropertyName("payment_type")]
		public string PaymentType { get; set; }
	}
}
