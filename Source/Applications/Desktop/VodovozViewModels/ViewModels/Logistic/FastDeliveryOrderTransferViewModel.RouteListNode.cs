namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public partial class FastDeliveryOrderTransferViewModel
	{
		public sealed class RouteListNode
		{
			public int RowNumber { get; set; }
			public int RouteListId { get; set; }
			public string CarRegistrationNumber { get; set; }
			public string DriverFullName { get; set; }
			public decimal Distance { get; set; }
		}
	}
}
