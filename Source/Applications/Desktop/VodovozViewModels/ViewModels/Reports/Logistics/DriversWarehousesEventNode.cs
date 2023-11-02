﻿using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics
{
	public class DriversWarehousesEventNode
	{
		private DateTime _eventDateTime;
		public DateTime EventDateTime
		{
			get => _eventDateTime;
			set
			{
				_eventDateTime = value;
				EventDate = _eventDateTime.Date;
				EventTime = _eventDateTime.TimeOfDay;
			}
		}
		public DateTime EventDate { get; private set; }
		public TimeSpan EventTime { get; private set; }
		public string DriverFio { get; set; }
		public string CarModelWithNumber { get; set; }
		public int EventNameId { get; set; }
		public string EventName { get; set; }
		public decimal Distance { get; set; }
	}
}
