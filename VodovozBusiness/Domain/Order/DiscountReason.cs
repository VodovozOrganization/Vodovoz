using System;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace Vodovoz.Domain.Orders
{
	public class DiscountReason : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }
	}
}
