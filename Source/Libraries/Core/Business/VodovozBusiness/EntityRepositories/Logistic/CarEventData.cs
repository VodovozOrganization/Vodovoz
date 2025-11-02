using System;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarEventData
	{
		public int EventId { get; set; }
		public int CarId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string EventTypeShortName { get; set; }
	}
}
