using System;

namespace ModulKassa
{
	public class CashboxSetting
	{
		public int CashBoxId { get; set; }
		public string RetailPointName { get; set; }
		public string BaseUrl { get; set; }
		public Guid UserId { get; set; }
		public string Password { get; set; }
		public bool IsTestMode { get; set; }
		public int CheckIntervalMin { get; set; }
	}
}
