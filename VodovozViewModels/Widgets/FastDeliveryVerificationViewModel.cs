using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;

namespace Vodovoz.ViewModels.Widgets
{
	public class FastDeliveryVerificationViewModel : WidgetViewModelBase
	{
		private readonly FastDeliveryVerificationDTO _fastDeliveryVerificationDTO;
		private DelegateCommand _saveLogisticiaCommentCommand;
		private readonly Employee _logistician;
		private readonly IUnitOfWork _uow;

		public FastDeliveryVerificationViewModel(FastDeliveryVerificationDTO fastDeliveryVerificationDTO)
		{
			_fastDeliveryVerificationDTO = fastDeliveryVerificationDTO ?? throw new ArgumentNullException(nameof(fastDeliveryVerificationDTO));

			var order = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory.Order;
			var deliveryPoint = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory.DeliveryPoint;
			DetailsTitle = $"Детализация по заказу №{order?.Id ?? 0}, адрес: {deliveryPoint?.ShortAddress}";

			FastDeliveryAvailabilityHistory = fastDeliveryVerificationDTO.FastDeliveryAvailabilityHistory;

			Nodes = _fastDeliveryVerificationDTO.FastDeliveryVerificationDetailsNodes.ToList();

			Message = Nodes.Any(x => x.IsValidRLToFastDelivery)
				? "Есть доступные водители для быстрой доставки"
				: "Нет доступных водителей для быстрой доставки";

			if(_fastDeliveryVerificationDTO.AdditionalInformation != null)
			{
				Message += string.Join("\n", _fastDeliveryVerificationDTO.AdditionalInformation);
			}
		}

		public FastDeliveryVerificationViewModel(FastDeliveryVerificationDTO fastDeliveryVerificationDTO, IUnitOfWork uow, Employee logistician)
			: this(fastDeliveryVerificationDTO)
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
