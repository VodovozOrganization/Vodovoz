using System;
using System.Collections.Generic;

namespace Vodovoz
{
	public class CustomerContact : QSContacts.Contact
	{
		public virtual IList<DeliveryPoint> DeliveryPoints { get; set; }

		public string PointCurator {
			get{
				if (DeliveryPoints == null || DeliveryPoints.Count <= 0)
					return String.Empty;
				if (DeliveryPoints.Count == 1)
					return DeliveryPoints [0].Name;
				else
					return String.Format ("{0} и еще {1}", DeliveryPoints [0].Name, DeliveryPoints.Count);
			}
		}

		public CustomerContact (): base()
		{
		}
	}
}

