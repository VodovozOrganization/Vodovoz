using Newtonsoft.Json;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class InventPosition
	{
		private int _vatTag;
		private decimal _vat;

		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; set; }

		[JsonProperty("quantity", Required = Required.Always)]
		public decimal Quantity { get; set; }

		[JsonProperty("price", Required = Required.Always)]
		public decimal PriceWithoutDiscount { get; set; }

		[JsonProperty("discSum")]
		public decimal DiscSum { get; set; }

		[JsonProperty("productMark", Required = Required.Always)]
		public string ProductMark { get; set; }

		[JsonProperty("vatTag", Required = Required.Always)]
		public int VatTag
		{
			get => _vatTag;
			set {
				_vatTag = value;
				switch(value) {
					case 1104:
						_vat = 0;
						break;
					case 1103:
						_vat = 10;
						break;
					case 1102:
						_vat = 20;
						break;
					case 1105:
						_vat = 0;
						break;
				}
			}
		}

		public decimal Vat {
			get => _vat;
			set {
				_vat = value;
				switch(value) {
					case 0:
						_vatTag = 1104;
						break;
					case 10:
						_vatTag = 1103;
						break;
					case 20:
						_vatTag = 1102;
						break;
				}
			}
		}

		/// <summary>
		/// Разрешительный режим
		/// </summary>
		[JsonProperty("industryRequisite", Required = Required.Always)]
		public IndustryRequisite IndustryRequisite { get; set; }
	}
}
