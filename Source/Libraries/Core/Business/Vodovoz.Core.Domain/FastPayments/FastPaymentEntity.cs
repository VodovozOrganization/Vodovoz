using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.FastPayments
{
	public class FastPaymentEntity : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual int OrderId { get; set; }
		public virtual string PaymentStatus { get; set; }
	}
}
