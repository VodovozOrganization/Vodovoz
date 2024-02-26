using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Models.Orders
{
	public class OrderCopyModel
	{
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IFlyerRepository _flyerRepository;

		public OrderCopyModel(INomenclatureSettings nomenclatureSettings, IFlyerRepository flyerRepository)
		{
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
		}

		public CopyingOrder StartCopyOrder(IUnitOfWork uow, int copiedOrderId, Order toOrder = null)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			Order copiedOrder = uow.GetById<Order>(copiedOrderId);
			if(copiedOrder == null)
			{
				throw new ArgumentException($"Не удалось получить копируемый заказ с id ({copiedOrderId})");
			}

			Order resultOrder = toOrder;
			if(resultOrder == null)
			{
				resultOrder = new Order();
			}	
		
			CopyingOrder copyingOrder = new CopyingOrder(uow, copiedOrder, resultOrder, _nomenclatureSettings, _flyerRepository);

			return copyingOrder;
		}
	}
}
