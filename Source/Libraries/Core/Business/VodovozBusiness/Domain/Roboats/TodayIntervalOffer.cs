using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Roboats
{
	public class TodayIntervalOffer : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int DeliveryInterval { get; set; }
		public virtual int StartHour { get; set; }
	}
}
