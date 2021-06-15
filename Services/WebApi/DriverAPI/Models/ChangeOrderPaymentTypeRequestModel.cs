using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.Models
{
	public class ChangeOrderPaymentTypeRequestModel
	{
		public int OrderId { get; set; }
		public PaymentDtoType NewPaymentType { get; set; }
		public DateTime ActionTime { get; set; }
	}
}
