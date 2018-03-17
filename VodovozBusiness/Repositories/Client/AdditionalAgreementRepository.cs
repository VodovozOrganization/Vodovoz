using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public static class AdditionalAgreementRepository
	{
		public static IList<AdditionalAgreement> GetActiveAgreementsForDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint)			
		{
			AdditionalAgreement agreementAlias = null;
			var queryResult = uow.Session.QueryOver<AdditionalAgreement>(() => agreementAlias)
				.Where(() => agreementAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Where(() => !agreementAlias.IsCancelled)
				.List();
			return queryResult;
		}
	}
}

