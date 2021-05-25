using DriverAPI.Library.Models;
using System;

namespace DriverAPI.Models
{
	public class ChangeOrderPaymentTypeRequestModel
	{
		public int OrderId { get; set; }
		public APIPaymentType NewPaymentType { get; set; }
		public DateTime ActionTime { get; set; }
	}
}
