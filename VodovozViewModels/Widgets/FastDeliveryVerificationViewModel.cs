using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Delivery;

namespace Vodovoz.ViewModels.Widgets
{
	public class FastDeliveryVerificationViewModel : WidgetViewModelBase
	{
		private readonly FastDeliveryVerificationDTO _fastDeliveryVerificationDTO;

		public FastDeliveryVerificationViewModel(FastDeliveryVerificationDTO fastDeliveryVerificationDTO)
		{
			_fastDeliveryVerificationDTO = fastDeliveryVerificationDTO ?? throw new ArgumentNullException(nameof(fastDeliveryVerificationDTO));

			var order = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory.Order;

			if(order != null)
			{
				DetailsTitle = $"Детализация по заказу №{order.Id}, адрес: {order.DeliveryPoint.ShortAddress}";
			}

			UpdateNodes();
		}

		private void UpdateNodes()
		{
			foreach(var node in _fastDeliveryVerificationDTO.FastDeliveryVerificationDetailsNodes)
			{
				Nodes.Add(node);
			}

			Message = Nodes.Any(x => x.IsValidRLToFastDelivery)
				? "Есть доступные водители для быстрой доставки"
				: "Нет доступных водителей для быстрой доставки";

			if(_fastDeliveryVerificationDTO.AdditionalInformation != null)
			{
				Message += string.Join("\n", _fastDeliveryVerificationDTO.AdditionalInformation);
			}
		}

		public string Message { get; set; }

		public string DetailsTitle { get; }

		public GenericObservableList<FastDeliveryVerificationDetailsNode> Nodes { get; } =
			new GenericObservableList<FastDeliveryVerificationDetailsNode>();

	}
}
