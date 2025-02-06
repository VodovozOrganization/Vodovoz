using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;

namespace VodovozBusiness.Domain.Orders
{
	public class ContractOrganizationForOrderHandler : IGetOrganizationForOrder
	{
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ContractOrganizationForOrderHandler(ICashReceiptRepository cashReceiptRepository)
		{
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			if(order.Id != 0 && order.Contract != null && _cashReceiptRepository.HasNeededReceipt(order.Id))
			{
				return new Dictionary<Organization, IEnumerable<OrderItem>>
				{
					{ order.Contract.Organization, null }
				};
			}

			return null;
		}
	}
}
