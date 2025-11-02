namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReportViewModel
	{
		public class PromosetNode
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public virtual bool IsSelected { get; set; }
		}
	}
}
