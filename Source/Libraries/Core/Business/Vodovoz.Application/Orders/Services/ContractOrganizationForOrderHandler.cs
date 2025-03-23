using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации из текущего контракта
	/// </summary>
	public class ContractOrganizationForOrderHandler : OrganizationForOrderHandler
	{
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ContractOrganizationForOrderHandler(ICashReceiptRepository cashReceiptRepository)
		{
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="order">Обрабатываемый заказ</param>
		/// <param name="uow">UnitOfWork</param>
		/// <returns></returns>
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			if(order.Id != 0 && order.Contract != null && _cashReceiptRepository.HasNeededReceipt(order.Id))
			{
				return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
				{
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(order.Contract.Organization, null)
				};
			}

			return base.GetOrganizationsWithOrderItems(requestTime, order, uow);
		}
	}
}
