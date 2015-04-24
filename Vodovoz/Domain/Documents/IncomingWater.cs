using System;
using QSOrmProject;

namespace Vodovoz
{
	public class IncomingWater: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		int amount;

		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		DateTime date;

		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}
	}
}

