using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository.Client
{
	public static class AdditionalAgreementRepository
	{

		/// <summary>
		/// Возвращает все доп. соглашения которые применимы к точке доставки. В список так же включены доп соглашения, без точки доставки. Так как они применимы к контрагенту целиком.
		/// </summary>
		public static IList<AdditionalAgreement> GetActiveAgreementsForDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint)			
		{
			AdditionalAgreement agreementAlias = null;
			CounterpartyContract contractAlias = null;
			var queryResult = uow.Session.QueryOver<AdditionalAgreement>(() => agreementAlias)
			                     .JoinAlias(() => agreementAlias.Contract, () => contractAlias)
			                     .Where(() => !contractAlias.IsArchive)
			                     .Where(() => agreementAlias.DeliveryPoint.Id == deliveryPoint.Id 
			                            || (agreementAlias.DeliveryPoint == null && contractAlias.Counterparty == deliveryPoint.Counterparty))
								 .Where(() => !agreementAlias.IsCancelled)
								 .List();
			return queryResult;
		}

		/// <summary>
		/// Получаем все фиксированные цены по для точки доставки
		/// </summary>
		/// <returns>Фиксированные цены</returns>
		public static IList<WaterSalesAgreementFixedPrice> GetFixedPricesForDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, CounterpartyContract contract)
		{
			WaterSalesAgreementFixedPrice fixedPriceAlias = null;
			WaterSalesAgreement salesAgreementAlias = null;
			CounterpartyContract contractAlias = null;

			var queryResults = uow.Session.QueryOver<WaterSalesAgreementFixedPrice>(() => fixedPriceAlias)
			                      .JoinQueryOver(fix => fix.AdditionalAgreement, () => salesAgreementAlias)
			                      .JoinAlias(() => salesAgreementAlias.Contract, () => contractAlias)
			                      .Where(() => !contractAlias.IsArchive)
			                      .Where(wsa => wsa.DeliveryPoint == deliveryPoint 
			                             || (wsa.DeliveryPoint == null && contractAlias.Counterparty == deliveryPoint.Counterparty))
			                      .Where(wsa => !wsa.IsCancelled && wsa.Contract.Id == contract.Id)
								  .List();

			return queryResults;
		}
	}
}

