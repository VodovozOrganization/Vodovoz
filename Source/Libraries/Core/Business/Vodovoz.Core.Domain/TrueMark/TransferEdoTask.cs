using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark
{
	public class TransferEdoTask : EdoTask
	{
		private int _orderEdoTaskId;
		private int _fromOrganizationId;
		private int _toOrganizationId;

		[Display(Name = "Код ЭДО задачи по заказу")]
		public virtual int OrderEdoTaskId
		{
			get => _orderEdoTaskId;
			set => SetField(ref _orderEdoTaskId, value);
		}

		[Display(Name = "Код организации отправителя")]
		public virtual int FromOrganizationId
		{
			get => _fromOrganizationId;
			set => SetField(ref _fromOrganizationId, value);
		}

		[Display(Name = "Код организации получателя")]
		public virtual int ToOrganizationId
		{
			get => _toOrganizationId;
			set => SetField(ref _toOrganizationId, value);
		}
	}
}
