using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	public class ExternalCreatingDeliveryPoint : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string UniqueKey { get; set; }
		public virtual int Source { get; set; }
		public virtual DateTime CreatingDate { get; set; }
	}
}
