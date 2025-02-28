namespace TaxcomEdoApi.Library.Builders
{
	public class EdoCustomerUpdNameBuilder : EdoUpdNameBuilder
	{
		protected override string DocName => "ON_NSCHFDOPPOK";
		public static EdoUpdNameBuilder Create() => new EdoCustomerUpdNameBuilder();
	}
}
