using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Models
{
	public interface IAdditionalLoadingModel
	{
		AdditionalLoadingDocument CreateAdditionLoadingDocument(IUnitOfWork uow, RouteList routeList);
		IList<AdditionalLoadingDocumentItem> CreateAdditionalLoadingItems(IUnitOfWork uow, decimal availableWeight, decimal availableVolume, DateTime routelistDate);
	}
}
