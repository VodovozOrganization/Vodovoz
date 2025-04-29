namespace TaxcomEdoApi.Library.Builders.Format5_01
{
	public class EdoSellerUpdNameBuilder5_01 : EdoUpdNameBuilder5_01
	{
		protected override string DocName => "ON_NSCHFDOPPR";
		public static EdoUpdNameBuilder5_01 Create() => new EdoSellerUpdNameBuilder5_01();
	}
}
