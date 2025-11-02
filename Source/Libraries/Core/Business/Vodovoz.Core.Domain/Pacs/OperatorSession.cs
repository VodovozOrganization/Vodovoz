using System;

namespace Vodovoz.Core.Domain.Pacs
{
	public class OperatorSession
	{
		public virtual Guid Id { get; set; }
		public virtual DateTime Started { get; set; }
		public virtual DateTime? Ended { get; set; }
		public virtual int OperatorId { get; set; }
	}
}
