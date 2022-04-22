using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using QS.Navigation;
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
		private readonly FastDeliveryVerificationData _fastDeliveryVerificationData;
		private string _message;
		
		public FastDeliveryVerificationDetailsViewModel(
			IUnitOfWork uow,
			IDeliveryRepository deliveryRepository,
			INavigationManager navigationManager,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			FastDeliveryVerificationData fastDeliveryVerificationData) : base(navigationManager)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
			_deliveryRulesParametersProvider =
				deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_fastDeliveryVerificationData = fastDeliveryVerificationData ?? throw new ArgumentNullException(nameof(fastDeliveryVerificationData));
			WindowPosition = WindowGravity.None;
			DetailsTitle = $"Детализация по заказу№{fastDeliveryVerificationData.OrderId}, адрес: {fastDeliveryVerificationData.Address}";
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
			var fastOrders = _deliveryRepository.GetRouteListsForFastDelivery(
				_uow, _fastDeliveryVerificationData.Latitude, _fastDeliveryVerificationData.Longitude, _deliveryRulesParametersProvider,
				_fastDeliveryVerificationData.NomenclatureNodes);

			foreach(var node in fastOrders)
			{
				Nodes.Add(node);
			}

			Message = Nodes.Any(x => x.IsValidRLToFastDelivery)
				? "Есть доступные водители для быстрой доставки"
				: "Нет доступных водителей для быстрой доставки";
		}
	}

	public class FastDeliveryVerificationData
	{
		public FastDeliveryVerificationData(
			int orderId,
			string address,
			double latitude,
			double longitude,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes)
		{
			OrderId = orderId;
			Address = address;
			Latitude = latitude;
			Longitude = longitude;
			NomenclatureNodes = nomenclatureNodes;
		}
		
		public int OrderId { get; }
		public string Address { get; }
		public double Latitude { get; }
		public double Longitude { get; }
		public IEnumerable<NomenclatureAmountNode> NomenclatureNodes { get; }
	}
}
