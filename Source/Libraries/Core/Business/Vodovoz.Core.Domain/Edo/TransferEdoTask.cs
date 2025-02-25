using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class TransferEdoTask : EdoTask
	{
		private int _fromOrganizationId;
		private int _toOrganizationId;
		private IObservableList<TransferEdoRequest> _transferEdoRequests = new ObservableList<TransferEdoRequest>();
		private TransferEdoTaskStatus _transferStatus;
		private DateTime? _transferStartTime;
		private int? _transferOrderId;

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

		[Display(Name = "Заявки на перенос")]
		public virtual IObservableList<TransferEdoRequest> TransferEdoRequests
		{
			get => _transferEdoRequests;
			set => SetField(ref _transferEdoRequests, value);
		}

		[Display(Name = "Статус переноса")]
		public virtual TransferEdoTaskStatus TransferStatus
		{
			get => _transferStatus;
			set => SetField(ref _transferStatus, value);
		}

		[Display(Name = "Время начала переноса")]
		public virtual DateTime? TransferStartTime
		{
			get => _transferStartTime;
			set => SetField(ref _transferStartTime, value);
		}

		[Display(Name = "Заказ")]
		public virtual int? TransferOrderId
		{
			get => _transferOrderId;
			set => SetField(ref _transferOrderId, value);
		}

		public override EdoTaskType TaskType => EdoTaskType.Transfer;
	}
}
