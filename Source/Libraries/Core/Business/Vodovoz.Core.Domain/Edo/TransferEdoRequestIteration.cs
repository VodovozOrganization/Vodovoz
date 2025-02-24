using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class TransferEdoRequestIteration : PropertyChangedBase
    {
        private int _id;
        private DateTime _time;
        private OrderEdoTask _orderEdoTask;
		private TransferInitiator _initiator;
		private TransferEdoRequestIterationStatus _status;
        private IObservableList<TransferEdoRequest> _transferEdoRequests = new ObservableList<TransferEdoRequest>();

        [Display(Name = "Код")]
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [Display(Name = "Время")]
        public DateTime Time
        {
            get => _time;
            set => SetField(ref _time, value);
        }

        [Display(Name = "ЭДО задача")]
        public OrderEdoTask OrderEdoTask
        {
            get => _orderEdoTask;
            set => SetField(ref _orderEdoTask, value);
        }

		[Display(Name = "Инициатор трансфера")]
		public virtual TransferInitiator Initiator
		{
			get => _initiator;
			set => SetField(ref _initiator, value);
		}

		[Display(Name = "Статус")]
        public TransferEdoRequestIterationStatus Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        [Display(Name = "Заявки на трансфер")]
        public IObservableList<TransferEdoRequest> TransferEdoRequests
        {
            get => _transferEdoRequests;
            set => SetField(ref _transferEdoRequests, value);
        }
    }
}
