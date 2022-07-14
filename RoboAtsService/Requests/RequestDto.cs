namespace RoboAtsService.Requests
{
	public class RequestDto
	{
		/// <summary>
		/// CID
		/// </summary>
		public string ClientPhone { get; set; }

		/// <summary>
		/// request
		/// </summary>
		public string RequestType { get; set; }

		/// <summary>
		/// show
		/// </summary>
		public string RequestSubType { get; set; }

		/// <summary>
		/// order_id
		/// </summary>
		public string OrderId { get; set; }

		/// <summary>
		/// address_id
		/// </summary>
		public string AddressId { get; set; }

		/// <summary>
		/// add
		/// </summary>
		public string IsAddOrder { get; set; }



		/// <summary>
		/// return
		/// </summary>
		public string ReturnBottlesCount { get; set; }

		/// <summary>
		/// date
		/// </summary>
		public string Date { get; set; }

		/// <summary>
		/// time
		/// </summary>
		public string Time { get; set; }

		/// <summary>
		/// fullorder
		/// </summary>
		public string IsFullOrder { get; set; }

		/// <summary>
		/// terminal
		/// </summary>
		public string IsTerminal { get; set; }

		/// <summary>
		/// bill
		/// </summary>
		public string BanknoteForReturn { get; set; }

		/// <summary>
		/// waterquantity
		/// </summary>
		public string WaterQuantity { get; set; }
	}
}
