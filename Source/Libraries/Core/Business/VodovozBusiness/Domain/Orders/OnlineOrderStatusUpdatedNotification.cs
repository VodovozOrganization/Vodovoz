using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
	public class OnlineOrderStatusUpdatedNotification : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual OnlineOrder OnlineOrder { get; protected set; }
		public virtual int? HttpCode { get; set; }
		public virtual DateTime? SentDate { get; set; }
		
		public static OnlineOrderStatusUpdatedNotification Create(OnlineOrder onlineOrder)
		{
			return new OnlineOrderStatusUpdatedNotification
			{
				OnlineOrder = onlineOrder
			};
		}
	}
}
