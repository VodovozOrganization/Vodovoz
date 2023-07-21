namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public partial class FastDeliveryOrderTransferViewModel
	{
		public class RouteListNode
		{
			public int RowNumber { get; set; }
			public int RouteListId { get; set; }
			public string CarRegistrationNumber { get; set; } = string.Empty;
			public string Name { get; set; }
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public string DriverFullName => LastName + " " + Name[0] + "." + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic[0] + ".");
		}
	}
}

