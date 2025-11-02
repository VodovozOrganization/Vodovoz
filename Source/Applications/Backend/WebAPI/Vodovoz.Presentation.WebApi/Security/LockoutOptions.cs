using System;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class LockoutOptions
	{
		public TimeSpan DefaultLockout { get;set; }
		public int MaxFailedAccessAttempts { get; set; }
		public bool AllowedForNewUsers { get; set; }
	}
}
