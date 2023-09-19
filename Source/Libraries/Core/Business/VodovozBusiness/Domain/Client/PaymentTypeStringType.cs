namespace Vodovoz.Domain.Client
{
	public class PaymentTypeStringType : NHibernate.Type.EnumStringType
	{
		public PaymentTypeStringType() : base(typeof(PaymentType)) { }
	}
}
