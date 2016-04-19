using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	public enum VAT
	{
		[Display (Name = "Без НДС")]
		[Value1c("БезНДС")]
		No,
		[Display (Name = "НДС 10%")]
		[Value1c("НДС10")]
		Vat10,
		[Display (Name = "НДС 18%")]
		[Value1c("НДС18")]
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
		[Display (Name = "Наличная", ShortName = "нал." )]
		cash,
		[Display (Name = "Безналичная", ShortName = "б/н." )]
		cashless,
		[Display (Name="Бартер", ShortName = "бар." )]
		barter
	}

	public class PaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public PaymentTypeStringType () : base (typeof(PaymentType))
		{
		}
	}
}
