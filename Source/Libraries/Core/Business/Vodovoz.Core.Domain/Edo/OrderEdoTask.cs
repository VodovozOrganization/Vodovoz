using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderEdoTask : EdoTask
	{
		private int _orderId;
		private int _organizationId;
		private int _counterpartyId;

		[Display(Name = "Код заказа")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		[Display(Name = "Код организации")]
		public virtual int OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}

		[Display(Name = "Код контрагента")]
		public virtual int CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}
	}
}
