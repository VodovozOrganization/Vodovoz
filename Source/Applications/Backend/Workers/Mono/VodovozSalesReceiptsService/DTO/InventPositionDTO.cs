using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Vodovoz.Domain.Goods;

namespace VodovozSalesReceiptsService.DTO
{
	[DataContract]
	public class InventPositionDTO
	{
		[DataMember(IsRequired = true)]
		string name;
		public string Name {
			get => name;
			set => name = value;
		}

		[DataMember(IsRequired = true)]
		decimal price;
		public decimal PriceWithoutDiscount {
			get => price;
			set => price = value;
		}

		[DataMember(IsRequired = true)]
		decimal quantity;
		public decimal Quantity {
			get => quantity;
			set => quantity = value;
		}

		[DataMember(IsRequired = true)]
		string productMark;
		public string ProductMark
		{
			get => productMark;
			set => productMark = value;
		}

		[DataMember(IsRequired = true)]
		int vatTag;
		public int VatTag {
			get => vatTag;
			set {
				vatTag = value;
				switch(value) {
					case 1104:
						vat = VAT.No;
						break;
					case 1103:
						vat = VAT.Vat10;
						break;
					case 1102:
						vat = VAT.Vat20;
						break;
					case 1105:
						vat = VAT.No;
						break;
				}
			}
		}

		[DataMember]
		decimal discSum;
		public decimal DiscSum {
			get => discSum;
			set => discSum = value;
		}

		VAT vat;
		public VAT Vat {
			get => vat;
			set {
				vat = value;
				switch(value) {
					case VAT.No:
						vatTag = 1104;
						break;
					case VAT.Vat10:
						vatTag = 1103;
						break;
					case VAT.Vat20:
						vatTag = 1102;
						break;
				}
			}
		}
	}
}

namespace VodovozSalesReceiptsService
{
	/// <summary>
	/// Тег НДС согласно ФЗ-54
	/// </summary>
	public enum VatTag
	{
		[Display(Name = "НДС 0%")]
		Vat0 = 1104,
		[Display(Name = "НДС 10%")]
		Vat10 = 1103,
		[Display(Name = "НДС 20%")]
		Vat20 = 1102,
		[Display(Name = "НДС не облагается")]
		VatFree = 1105,
		[Display(Name = "НДС с рассч. ставкой 10%")]
		VatEstimatedRate10 = 1107,
		[Display(Name = "НДС с рассч. ставкой 20%")]
		VatEstimatedRate20 = 1106
	}
}
