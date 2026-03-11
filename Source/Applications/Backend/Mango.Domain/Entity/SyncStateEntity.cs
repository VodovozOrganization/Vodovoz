using System;

namespace Mango.Domain.Entity
{
	public class SyncStateEntity
	{
		public string Source { get; set; } = string.Empty;
		
		public DateTime LastProcessedDate { get; set; }
		
		public DateTime UpdatedAtDate { get; set; }
	}
}
