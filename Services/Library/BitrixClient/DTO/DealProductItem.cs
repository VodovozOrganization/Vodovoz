using Newtonsoft.Json;

namespace Bitrix.DTO
{
	public class DealProductItem
	{
		[JsonProperty("OWNER_ID")]
		public uint OwnerId { get; set; }

		[JsonProperty("OWNER_TYPE")]
		public string OwnerType { get; set; }

		[JsonProperty("PRODUCT_ID")]
		public uint ProductId { get; set; }

		[JsonProperty("PRODUCT_NAME")]
		public string ProductName { get; set; }

		[JsonProperty("PRODUCT_DESCRIPTION")]
		public string ProductDescription { get; set; }

		[JsonProperty("PRICE")]
		public decimal Price { get; set; }

		[JsonProperty("PRICE_EXCLUSIVE")]
		public decimal PriceExclusive { get; set; }

		[JsonProperty("PRICE_NETTO")]
		public decimal PriceNetto { get; set; }

		[JsonProperty("SECTION_ID")]
		public string PriceAccount { get; set; }

		[JsonProperty("QUANTITY")]
		public decimal Count { get; set; }

		[JsonProperty("DISCOUNT_TYPE_ID")]
		public int DiscountTypeId { get; set; }

		[JsonProperty("DISCOUNT_RATE")]
		public decimal DiscountRate { get; set; }

		[JsonProperty("DISCOUNT_SUM")]
		public decimal DiscountSum { get; set; }

		[JsonProperty("TAX_RATE")]
		public decimal TaxRate { get; set; }

		[JsonProperty("TAX_INCLUDED")]
		public string TaxIncluded { get; set; } // Bool

		[JsonProperty("CUSTOMIZED")]
		public string Customized { get; set; }

		[JsonProperty("MEASURE_CODE")]
		public uint MeasureCode { get; set; }

		[JsonProperty("MEASURE_NAME")]
		public string MeasureName { get; set; } // шт
	}
}
