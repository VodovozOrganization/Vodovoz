using System;
using FastPaymentsAPI.Library.DTO_s;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentManager : IFastPaymentManager
	{
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IStandartNomenclatures _standartNomenclatures;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly ISelfDeliveryRepository _selfDeliveryRepository;
		private readonly ICashRepository _cashRepository;

		public FastPaymentManager(
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IOrderParametersProvider orderParametersProvider,
			IStandartNomenclatures standartNomenclatures,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository)
		{
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_standartNomenclatures = standartNomenclatures ?? throw new ArgumentNullException(nameof(standartNomenclatures));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_selfDeliveryRepository = selfDeliveryRepository ?? throw new ArgumentNullException(nameof(selfDeliveryRepository));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
		}

		public bool IsTimeToCancelPayment(DateTime fastPaymentCreationDate, bool fastPaymentWithQR)
		{
			var elapsedTime = (DateTime.Now - fastPaymentCreationDate).TotalMinutes;

			if(fastPaymentWithQR)
			{
				if(elapsedTime > _fastPaymentParametersProvider.GetQRLifetime)
				{
					return true;
				}
			}
			else
			{
				if(elapsedTime > _fastPaymentParametersProvider.GetPayUrlLifetime)
				{
					return true;
				}
			}

			return false;
		}

		public void UpdateFastPaymentStatus(IUnitOfWork uow, FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate)
		{
			switch(newStatus)
			{
				case FastPaymentDTOStatus.Processing:
					fastPayment.SetProcessingStatus();
					break;
				case FastPaymentDTOStatus.Performed:
					fastPayment.SetPerformedStatus(
						uow,
						statusDate,
						uow.GetById<PaymentFrom>(_orderParametersProvider.GetPaymentByCardFromFastPaymentServiceId),
						_standartNomenclatures,
						_routeListItemRepository,
						_selfDeliveryRepository,
						_cashRepository);
					break;
				case FastPaymentDTOStatus.Rejected:
					fastPayment.SetRejectedStatus();
					break;
				default:
					throw new InvalidOperationException("Неизвестный статус оплаты");
			}
		}
	}
}
