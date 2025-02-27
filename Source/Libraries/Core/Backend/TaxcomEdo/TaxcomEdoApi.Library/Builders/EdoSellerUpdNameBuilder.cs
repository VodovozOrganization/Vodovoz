namespace TaxcomEdoApi.Library.Builders
{
	public class EdoSellerUpdNameBuilder : EdoUpdNameBuilder
	{
		protected override string DocName => "ON_NSCHFDOPPR";
		public static EdoUpdNameBuilder Create() => new EdoSellerUpdNameBuilder();
	}
}
