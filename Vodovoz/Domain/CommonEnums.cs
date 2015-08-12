using System.Data.Bindings;

namespace Vodovoz.Domain
{
	public enum VAT
	{
		[ItemTitleAttribute ("Без НДС")]
		No,
		[ItemTitleAttribute ("НДС 10%")]
		Vat10,
		[ItemTitleAttribute ("НДС 18%")]
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
		[ItemTitleAttribute ("Наличная")]
		cash,
		[ItemTitleAttribute ("Безналичная")]
		cashless
	}

	public class PaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public PaymentTypeStringType () : base (typeof(PaymentType))
		{
		}
	}
}
