using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkDriverAppModel
	{
		private readonly TrueMarkCodesPool _codePool;

		public TrueMarkDriverAppModel(TrueMarkCodesPool codePool)
		{
			_codePool = codePool ?? throw new ArgumentNullException(nameof(codePool));
		}

		public void ProcessCodes(Order order, IEnumerable<IOrderItemScannedInfo> scannedItems)
		{
			if(order.Client.PersonType == PersonType.legal)
			{
				ProcessLegalCounterparty(order, scannedItems);
			}
			else
			{
				ProcessNaturalCounterparty(order, scannedItems);
			}
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

		private void ProcessLegalCounterparty(Order order, IEnumerable<IOrderItemScannedInfo> scannedItems)
		{
			if(order.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				return;
			}

			foreach(var scannedItem in scannedItems)
			{
				AddDefectiveCodesToPool(scannedItem.DefectiveBottleCodes);
				AddCodesToPool(scannedItem.BottleCodes);
			}
		}

		private void ProcessNaturalCounterparty(Order order, IEnumerable<IOrderItemScannedInfo> scannedItems)
		{
			foreach(var scannedItem in scannedItems)
			{
				string code = scannedItem.BottleCodes
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

				}

				AddDefectiveCodesToPool(scannedItem.DefectiveBottleCodes);
				AddCodesToPool(scannedItem.BottleCodes);
			}

			//Выбираем 

			//Добавляем код к заказу для чека
		}
	}

	public interface IOrderItemScannedInfo
	{
		IEnumerable<string> BottleCodes { get; set; }
		IEnumerable<string> DefectiveBottleCodes { get; set; }
		int OrderSaleItemId { get; set; }
	}
}
