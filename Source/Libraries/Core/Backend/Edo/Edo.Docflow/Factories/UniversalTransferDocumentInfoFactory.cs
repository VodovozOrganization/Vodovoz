using Edo.Docflow.Dto;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Docflow.Factories
{
	public class UniversalTransferDocumentInfoFactory
	{
		private readonly IUnitOfWork _uow;

		public UniversalTransferDocumentInfoFactory(IUnitOfWork uow)
		{
			_uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
		}

		public UniversalTransferDocumentInfo CreateUniversalTransferDocumentInfo(TransferOrder transferOrder)
		{
			if(transferOrder is null)
			{
				throw new ArgumentNullException(nameof(transferOrder));
			}

			if(transferOrder.Seller is null
				|| transferOrder.Customer is null
				|| !transferOrder.TrueMarkCodes.Any())
			{
				throw new InvalidOperationException("В заказе перемещения товаров часть данных отсутствует");
			}

			if(transferOrder.Id == 0)
			{
				throw new InvalidOperationException("При заполнении данных в УПД необходимо, чтобы заказ перемещения товаров был предварительно сохранен");
			}

			return ConvertTransferOrderToUniversalTransferDocumentInfo(transferOrder);
		}

		private UniversalTransferDocumentInfo ConvertTransferOrderToUniversalTransferDocumentInfo(TransferOrder transferOrder)
		{
			var document = new UniversalTransferDocumentInfo();

			return document;
		}
	}
}
