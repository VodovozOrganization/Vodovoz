using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	public enum VAT
	{
		[Display (Name = "Без НДС")]
		No,
		[Display (Name = "НДС 10%")]
		Vat10,
		[Display (Name = "НДС 18%")]
		Vat18
	}

	public class VATStringType : NHibernate.Type.EnumStringType
	{
		public VATStringType () : base (typeof(VAT))
		{
		}
	}

	public enum PaymentType
	{
		[Display (Name = "Наличная")]
		cash,
		[Display (Name = "Безналичная")]
		cashless,
		[Display (Name="Бартер")]
		barter
	}

	public class PaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public PaymentTypeStringType () : base (typeof(PaymentType))
		{
		}
	}
}
