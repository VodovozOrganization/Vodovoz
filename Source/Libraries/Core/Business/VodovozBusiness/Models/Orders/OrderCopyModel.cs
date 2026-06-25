using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Handlers;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Models.Orders
{
	public class OrderCopyModel
	{
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly IOrderProductHandler _productHandler;

		public OrderCopyModel(
			INomenclatureSettings nomenclatureSettings,
			IFlyerRepository flyerRepository,
			IOrderContractUpdater contractUpdater,
			IOrderProductHandler productHandler)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_productHandler = productHandler ?? throw new ArgumentNullException(nameof(productHandler));
		}

		public CopyingOrder StartCopyOrder(IUnitOfWork uow, int copiedOrderId, Order toOrder = null)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var copiedOrder = uow.GetById<Order>(copiedOrderId);
			if(copiedOrder is null)
			{
				throw new ArgumentException($"Не удалось получить копируемый заказ с id ({copiedOrderId})");
			}

			var resultOrder = toOrder ?? new Order();
			_productHandler.Initialize(uow, resultOrder);

			var copyingOrder = new CopyingOrder(
				uow,
				copiedOrder,
				resultOrder,
				_nomenclatureSettings,
				_flyerRepository,
				_contractUpdater,
				_productHandler
				);

			return copyingOrder;
		}
	}
}
