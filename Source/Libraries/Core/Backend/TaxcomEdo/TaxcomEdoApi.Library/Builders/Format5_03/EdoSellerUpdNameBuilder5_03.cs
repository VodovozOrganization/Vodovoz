using TaxcomEdoApi.Library.Builders.Format5_01;

namespace TaxcomEdoApi.Library.Builders.Format5_03
{
	public class EdoSellerUpdNameBuilder5_03 : EdoUpdNameBuilder5_03
	{
		protected override string DocName => "ON_NSCHFDOPPR";
		public static EdoUpdNameBuilder5_03 Create() => new EdoSellerUpdNameBuilder5_03();
	}
}
