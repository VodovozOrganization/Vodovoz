﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации через которую работает клиент
	/// </summary>
	public class OrganizationFromClientForOrderHandler : OrganizationForOrderHandler
	{
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			if(organizationChoice.ClientWorksThroughOrganization != null)
			{
				return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
				{
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(organizationChoice.ClientWorksThroughOrganization)
				};
			}
			
			return base.GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
		}
	}
}
