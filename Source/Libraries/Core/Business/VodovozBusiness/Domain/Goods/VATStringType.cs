namespace Vodovoz.Domain.Goods
{
	public class VATStringType : NHibernate.Type.EnumStringType
	{
		public VATStringType() : base(typeof(VAT)) { }
	}
}
