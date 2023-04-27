using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Client
{
	public class ExternalCounterpartyAssignNotification : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual ExternalCounterparty ExternalCounterparty { get; set; }
		public virtual int? HttpCode { get; set; }
		public virtual DateTime? SentDate { get; set; }
	}
}
