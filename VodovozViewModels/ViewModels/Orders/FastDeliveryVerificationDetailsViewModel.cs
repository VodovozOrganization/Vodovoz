using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Orders
{
	public class FastDeliveryVerificationDetailsViewModel : WindowDialogViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly IDeliveryRepository _deliveryRepository;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly FastDeliveryVerification _fastDeliveryVerification;
		private string _message;
		
		public FastDeliveryVerificationDetailsViewModel(
			IUnitOfWork uow,
			IDeliveryRepository deliveryRepository,
			INavigationManager navigationManager,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			FastDeliveryVerification fastDeliveryVerification,
			Order fastDeliveryOrder) : base(navigationManager)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_deliveryRulesParametersProvider =
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_fastDeliveryVerification = fastDeliveryVerification ?? throw new ArgumentNullException(nameof(fastDeliveryVerification));
			WindowPosition = WindowGravity.None;
			DetailsTitle = $"Детализация по заказу№{fastDeliveryOrder.Id}, адрес: {fastDeliveryOrder.DeliveryPoint.ShortAddress}";
			UpdateNodes();
		}

		public GenericObservableList<FastDeliveryVerificationDetailsNode> Nodes { get; } =
			new GenericObservableList<FastDeliveryVerificationDetailsNode>();
		public string Message
		{
			get => _message;
			set => SetField(ref _message, value);
		}
		
		public string DetailsTitle { get; }

		private void UpdateNodes()
		{
			foreach(var node in _fastDeliveryVerification.FastDeliveryVerificationDetailsNodes)
			{
				Nodes.Add(node);
			}

			if(_fastDeliveryVerification.AdditionalInformation != null)
			{
				Message = string.Join("\n", _fastDeliveryVerification.AdditionalInformation);
			}

			Message = Nodes.Any(x => x.IsValidRLToFastDelivery)
				? "Есть доступные водители для быстрой доставки"
				: "Нет доступных водителей для быстрой доставки";
		}
	}
}
