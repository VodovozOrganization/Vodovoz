using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class OrderEdoTask : EdoTask
	{
		private OrderEdoRequest _orderEdoRequest;

		private IObservableList<EdoTaskItem> _items =
			new ObservableList<EdoTaskItem>();

		//private IObservableList<TransferEdoRequest> _transferEdoRequests = 
		//	new ObservableList<TransferEdoRequest>();

		private IObservableList<TransferEdoRequestIteration> _transferIterations = 
			new ObservableList<TransferEdoRequestIteration>();


		[Display(Name = "Заявка ЭДО отправки клиенту")]
		public virtual OrderEdoRequest OrderEdoRequest
		{
			get => _orderEdoRequest;
			set => SetField(ref _orderEdoRequest, value);
		}

		[Display(Name = "Строки с кодами")]
		public virtual IObservableList<EdoTaskItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		//[Display(Name = "Заявки на перенос")]
		//public virtual IObservableList<TransferEdoRequest> TransferEdoRequests
		//{
		//	get => _transferEdoRequests;
		//	set => SetField(ref _transferEdoRequests, value);
		//}

		[Display(Name = "Итерации переноса")]
		public virtual IObservableList<TransferEdoRequestIteration> TransferIterations
		{
			get => _transferIterations;
			set => SetField(ref _transferIterations, value);
		}

	}
}
