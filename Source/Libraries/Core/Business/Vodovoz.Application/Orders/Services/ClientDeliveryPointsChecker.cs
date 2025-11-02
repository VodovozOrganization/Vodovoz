using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Counterparties;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	internal sealed class ClientDeliveryPointsChecker : IClientDeliveryPointsChecker
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDeliveryPointRepository _deliveryPointRepository;

		public ClientDeliveryPointsChecker(
			IUnitOfWorkFactory unitOfWorkFactory,
			IDeliveryPointRepository deliveryPointRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_deliveryPointRepository = deliveryPointRepository;
		}
		
		public bool ClientDeliveryPointExists(int counterpartyId, int deliveryPointId)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Проверка соответствия точки доставки контрагенту"))
			{
				return _deliveryPointRepository.ClientDeliveryPointExists(uow, counterpartyId, deliveryPointId);
			}
		}
	}
}
