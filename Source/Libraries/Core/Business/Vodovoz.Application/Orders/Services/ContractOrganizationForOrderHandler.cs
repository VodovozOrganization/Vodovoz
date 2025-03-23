﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Cash;
using VodovozBusiness.Models.Orders;
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

		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			if(organizationChoice.OrderId != 0
				&& organizationChoice.Contract != null
				&& _cashReceiptRepository.HasNeededReceipt(organizationChoice.OrderId))
			{
				return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
				{
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(organizationChoice.Contract.Organization, null)
				};
			}

			return base.GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
		}
	}
}
