namespace TaxcomEdoApi.Library.Builders.Format5_01
{
	public class EdoCustomerUpdNameBuilder5_01 : EdoUpdNameBuilder5_01
	{
		protected override string DocName => "ON_NSCHFDOPPOK";
		public static EdoUpdNameBuilder5_01 Create() => new EdoCustomerUpdNameBuilder5_01();
	}
}
