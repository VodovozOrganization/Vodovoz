using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class ContractOrganizationForOrderHandler : IGetOrganizationForOrder
	{
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ContractOrganizationForOrderHandler(ICashReceiptRepository cashReceiptRepository)
		{
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow = null)
		{
			if(order.Id != 0 && order.Contract != null && _cashReceiptRepository.HasNeededReceipt(order.Id))
			{
				return new List<OrganizationForOrderWithOrderItems>
				{
					new OrganizationForOrderWithOrderItems(order.Contract.Organization, null)
				};
			}

			return null;
		}
	}
}
