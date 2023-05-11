﻿using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.DTOs
{
	public class PayBySmsRequestDto : IActionTimeTrackable
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}
