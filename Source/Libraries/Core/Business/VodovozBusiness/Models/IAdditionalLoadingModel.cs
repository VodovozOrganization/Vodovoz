using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Models
{
	public interface IAdditionalLoadingModel
	{
		AdditionalLoadingDocument CreateAdditionLoadingDocument(IUnitOfWork uow, RouteList routeList);
		void ReloadActiveFlyers(IUnitOfWork uow, RouteList routelist, DateTime previousRoutelistDate);
		void UpdateDeliveryFreeBalanceOperations(IUnitOfWork uow, RouteList routeList);
	}
}
