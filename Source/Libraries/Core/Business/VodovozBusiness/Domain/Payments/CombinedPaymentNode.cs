using QS.DomainModel.Entity;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Payments
{
	public class CombinedPaymentNode : IDomainObject
	{
		public bool IsTopLevel { get; set; }
		public PaymentType? PaymentType { get; set; }
		public string PaymentSubType { get; set; }
		public int Id { get; set; }
		public string Title { get; set; }
		public int? TopLevelId { get; set; }
		public IList<CombinedPaymentNode> Childs { get; set; }
	}
}
