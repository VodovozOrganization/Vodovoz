using System;
using QSOrmProject;

namespace Vodovoz
{
	public class Document : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DateTime timeStamp;

		public virtual DateTime TimeStamp {
			get { return timeStamp; }
			set { SetField (ref timeStamp, value, () => TimeStamp); }
		}
	}
}

