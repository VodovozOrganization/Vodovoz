using Newtonsoft.Json;
using System;

namespace BitrixApi.DTO
{
	public class Product
	{
		[JsonProperty("ID")] 
		public int Id { get; set; }

		[JsonProperty("NAME")] 
		public string Name { get; set; }

		[JsonProperty("ACTIVE")] 
		public string Active { get; set; }

		[JsonProperty("TIMESTAMP_X")] 
		public DateTime TimeStampX { get; set; }

		[JsonProperty("DATE_CREATE")]
		public DateTime DateCreate { get; set; }

		[JsonProperty("MODIFIED_BY")]
		public int ModifiedBy { get; set; }

		[JsonProperty("CREATED_BY")]
		public int CreatedBy { get; set; }

		[JsonProperty("CATALOG_ID")]
		public int CatalogId { get; set; }

		[JsonProperty("SECTION_ID")] 
		public int SectionId { get; set; }

		[JsonProperty("PRICE")]
		public decimal Price { get; set; }

		[JsonProperty("CURRENCY_ID")]
		public string CurrencyId { get; set; }

		[JsonProperty("VAT_INCLUDED")]
		public string VatIncluded { get; set; }

		[JsonProperty("MEASURE")]
		public int Measure { get; set; }

		[JsonProperty("PROPERTY_174")]
		public ProductCategory Category { get; set; }

		[JsonProperty("PROPERTY_176")]
		public OurProductInfo IsOurProduct { get; set; }

		[JsonProperty("PROPERTY_178")]
		public NomenclatureInfo NomenclatureInfo { get; set; }

		[JsonProperty("PROPERTY_180")]
		public PromosetInfo PromosetInfo { get; set; }
	}
}
