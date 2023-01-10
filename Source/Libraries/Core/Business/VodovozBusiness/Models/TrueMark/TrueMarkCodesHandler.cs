using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodesHandler
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITrueMarkCodesPool _codePool;
		private readonly ITrueMarkRepository _trueMarkRepository;

		public TrueMarkCodesHandler(IUnitOfWorkFactory uowFactory, TrueMarkApiClientFactory trueMarkApiClientFactory, ITrueMarkCodesPool codePool, ITrueMarkRepository trueMarkRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codePool = codePool ?? throw new ArgumentNullException(nameof(codePool));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
		}

		public void ProcessCodes()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var newCashReceiptOrders = _trueMarkRepository.GetNewCashReceiptOrders(uow);

				foreach(TrueMarkCashReceiptOrder newCashReceiptOrder in newCashReceiptOrders)
				{
					ProcessOrder(newCashReceiptOrder);
				}
			}
		}

		private void ProcessOrder(TrueMarkCashReceiptOrder trueMarkCashReceiptOrder)
		{
			var defectiveCodes = trueMarkCashReceiptOrder.ScannedCodes.Where(x => x.IsDefectiveSourceCode);
			ProcessDefectiveCodes(defectiveCodes);

			var goodCodes = trueMarkCashReceiptOrder.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

			var order = trueMarkCashReceiptOrder.Order;
			if(order.Client.PersonType == PersonType.legal)
			{
				ProcessLegalCounterparty(order, goodCodes);
			}
			else
			{
				ProcessNaturalCounterparty(order, goodCodes);
			}
		}

		private void ProcessDefectiveCodes(IEnumerable<TrueMarkCashReceiptProductCode> defectiveCodeEntities)
		{
			foreach(var codeEntity in defectiveCodeEntities)
			{
				_codePool.PutDefectiveCode(codeEntity.CodeSource);
				var newCode = _codePool.TakeCode();
				codeEntity.CodeResult = newCode;
			}
		}

		private void ProcessGoodCode(Order order, TrueMarkCashReceiptProductCode codeEntity)
		{
			if(order.Client.PersonType == PersonType.legal)
			{
				ProcessLegalCounterparty(order, codeEntity);
			}
			else
			{
				ProcessNaturalCounterparty(order, codeEntity);
			}

			_codePool.PutDefectiveCode(codeEntity.CodeSource);

			var goodCode = _codePool.TakeCode();
			codeEntity.CodeResult = goodCode;
		}

		private void AddDefectiveCodesToPool(IEnumerable<string> defectiveCodes)
		{
			foreach(var defectiveCode in defectiveCodes)
			{
				_codePool.PutDefectiveCode(defectiveCode);
			}
		}

		private void AddCodesToPool(IEnumerable<string> codes)
		{
			foreach(var code in codes)
			{
				_codePool.PutCode(code);
			}
		}

		private void ProcessLegalCounterparty(Order order, IEnumerable<TrueMarkCashReceiptProductCode> goodCodeEntities)
		{
			if(order.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				return;
			}

			foreach(var codeEntity in goodCodeEntities)
			{
				_codePool.PutCode(codeEntity.CodeSource);
			}
		}

		private void ProcessNaturalCounterparty(Order order, IEnumerable<TrueMarkCashReceiptProductCode> goodCodeEntities)
		{
			if(order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				//проверяем в честном знаке
				//Надо создавать систему параллельной регистрации и проверки кодов, не мешающей вызовам апи
				bool vivedenIzOborota = false;
				if(vivedenIzOborota)
				{

				}


			}
			else
			{
				foreach(var codeEntity in goodCodeEntities)
				{
					codeEntity.CodeResult = codeEntity.CodeSource;
				}
			}

			foreach(var scannedItem in scannedItems)
			{
				string code = scannedItem.BottleCodes
				

				AddDefectiveCodesToPool(scannedItem.DefectiveBottleCodes);
				AddCodesToPool(scannedItem.BottleCodes);
			}

			//Выбираем 

			//Добавляем код к заказу для чека
		}
	}
}
