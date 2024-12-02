using Microsoft.AspNetCore.Mvc;
using System;

namespace RoboAtsService.Contracts.Requests
{
	/// <summary>
	/// Запрос к старым эндпоинтам
	/// </summary>
	public class RequestDto
	{
		/// <summary>
		/// CID
		/// </summary>
		[BindProperty(Name = "CID")]
		public string ClientPhone { get; set; }

		/// <summary>
		/// CALL_UUID
		/// </summary>
		[BindProperty(Name = "CALL_UUID")]
		public Guid CallGuid { get; set; }

		/// <summary>
		/// request
		/// </summary>
		[BindProperty(Name = "request")]
		public string RequestType { get; set; }

		/// <summary>
		/// show
		/// </summary>
		[BindProperty(Name = "show")]
		public string RequestSubType { get; set; }

		/// <summary>
		/// order_id
		/// </summary>
		[BindProperty(Name = "order_id")]
		public string OrderId { get; set; }

		/// <summary>
		/// address_id
		/// </summary>
		[BindProperty(Name = "address_id")]
		public string AddressId { get; set; }

		/// <summary>
		/// add
		/// </summary>
		[BindProperty(Name = "add")]
		public string IsAddOrder { get; set; }

		/// <summary>
		/// return
		/// </summary>
		[BindProperty(Name = "return")]
		public string ReturnBottlesCount { get; set; }

		/// <summary>
		/// date
		/// </summary>
		[BindProperty(Name = "date")]
		public string Date { get; set; }

		/// <summary>
		/// time
		/// </summary>
		[BindProperty(Name = "time")]
		public string Time { get; set; }

		/// <summary>
		/// fullorder
		/// </summary>
		[BindProperty(Name = "fullorder")]
		public string IsFullOrder { get; set; }

		/// <summary>
		/// payment_type
		/// </summary>
		[BindProperty(Name = "payment_type")]
		public string PaymentType { get; set; }

		/// <summary>
		/// bill
		/// </summary>
		[BindProperty(Name = "bill")]
		public string BanknoteForReturn { get; set; }

		/// <summary>
		/// waterquantity
		/// </summary>
		[BindProperty(Name = "waterquantity")]
		public string WaterQuantity { get; set; }
	}
}
