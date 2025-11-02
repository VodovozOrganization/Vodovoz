using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Tools.Orders;

namespace Vodovoz.ViewModels.Widgets
{
	public class FastDeliveryVerificationViewModel : WidgetViewModelBase
	{
		private DelegateCommand _saveLogisticiaCommentCommand;
		private readonly Employee _logistician;
		private readonly IUnitOfWork _uow;

		public FastDeliveryVerificationViewModel(FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory)
		{
			var order = fastDeliveryAvailabilityHistory.Order;
			var deliveryPoint = fastDeliveryAvailabilityHistory.DeliveryPoint;
			DetailsTitle = $"Детализация по заказу №{order?.Id ?? 0}, адрес: {deliveryPoint?.ShortAddress ?? fastDeliveryAvailabilityHistory.AddressWithoutDeliveryPoint}";

			FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistory)); ;

			var fastDeliveryHistoryConverter = new FastDeliveryHistoryConverter();

			Nodes = fastDeliveryHistoryConverter.ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(fastDeliveryAvailabilityHistory.Items);

			Message = Nodes.Any(x => x.IsValidRLToFastDelivery)
				? "Есть доступные водители для быстрой доставки"
				: "Нет доступных водителей для быстрой доставки";

			if(fastDeliveryAvailabilityHistory.AdditionalInformation != null)
			{
				Message += string.Join("\n", fastDeliveryAvailabilityHistory.AdditionalInformation);
			}
		}

		public FastDeliveryVerificationViewModel(FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory, IUnitOfWork uow, Employee logistician)
			: this(fastDeliveryAvailabilityHistory)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_logistician = logistician ?? throw new ArgumentNullException(nameof(logistician));
		}

		public DelegateCommand SaveLogisticianCommentCommand =>
			_saveLogisticiaCommentCommand ?? (_saveLogisticiaCommentCommand = new DelegateCommand(() =>
				{
					if(_uow == null || _logistician == null)
					{
						return;
					}

					if(FastDeliveryAvailabilityHistory.Logistician == null)
					{
						FastDeliveryAvailabilityHistory.Logistician = _logistician;
					}

					FastDeliveryAvailabilityHistory.LogisticianCommentVersion = DateTime.Now;

					_uow.Save(FastDeliveryAvailabilityHistory);
					_uow.Commit();
				},
				() => true
			));

		public string Message { get; set; }

		public string DetailsTitle { get; }

		public IList<FastDeliveryVerificationDetailsNode> Nodes { get; }
		
		public IEnumerable<FastDeliveryOrderItemHistory> OrderItemsHistory => FastDeliveryAvailabilityHistory.OrderItemsHistory;
		
		public FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory { get; }
		
		public bool ShowLogisticianComment => _logistician != null;
	}
}
