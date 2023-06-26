using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum QRPaymentDTOStatus
	{
		WaitingForPayment,
		Paid,
		Cancelled
	}
}
