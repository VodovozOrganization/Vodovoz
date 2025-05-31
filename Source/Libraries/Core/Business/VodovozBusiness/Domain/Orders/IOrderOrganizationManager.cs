using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IOrderOrganizationManager : ISplitOrderByOrganizations
	{
		PartitionedOrderByOrganizations GetOrderPartsByOrganizations(
			IUnitOfWork uow, TimeSpan requestTime, OrderOrganizationChoice organizationChoice);
		bool OrderHasGoodsFromSeveralOrganizations(
			TimeSpan requestTime, IList<int> nomenclatureIds, bool isSelfDelivery, PaymentType paymentType, PaymentFrom paymentFrom);
	}
}
