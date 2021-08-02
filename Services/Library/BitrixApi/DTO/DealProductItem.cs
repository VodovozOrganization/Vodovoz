using Newtonsoft.Json;

namespace BitrixApi.DTO
{
	public class DealProductItem
	{
		[JsonProperty("OWNER_ID")]
		public string OwnerId { get; set; }

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
		public int Count { get; set; }

		[JsonProperty("DISCOUNT_TYPE_ID")]
		public int DiscountTypeId { get; set; }

		[JsonProperty("DISCOUNT_RATE")]
		public int DiscountRate { get; set; }

		[JsonProperty("DISCOUNT_SUM")]
		public int DiscountSum { get; set; }

		[JsonProperty("TAX_RATE")]
		public int TaxRate { get; set; }

		[JsonProperty("TAX_INCLUDED")]
		public string TaxIncluded { get; set; } // Bool

		[JsonProperty("CUSTOMIZED")]
		public string Customized { get; set; }

		[JsonProperty("MEASURE_CODE")]
		public string MeasureCode { get; set; }

		[JsonProperty("MEASURE_NAME")]
		public string MeasureName { get; set; } // шт
	}
}
