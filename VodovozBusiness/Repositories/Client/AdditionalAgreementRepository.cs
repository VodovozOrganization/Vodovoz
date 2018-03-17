using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository.Client
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

		/// <summary>
		/// Получаем все фиксированные цены по для точки доставки
		/// </summary>
		/// <returns>Фиксированные цены</returns>
		public static IList<WaterSalesAgreementFixedPrice> GetFixedPricesForDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			WaterSalesAgreementFixedPrice fixedPriceAlias = null;
			WaterSalesAgreement salesAgreementAlias = null;

			var queryResults = uow.Session.QueryOver<WaterSalesAgreementFixedPrice>(() => fixedPriceAlias)
			                      .JoinQueryOver(fix => fix.AdditionalAgreement, () => salesAgreementAlias)
			                      .Where(wsa => wsa.DeliveryPoint == deliveryPoint)
			                      .Where(wsa => !wsa.IsCancelled)
								  .List();

			return queryResults;
		}
	}
}

