using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public class WaterSalesAgreementFixedPriceRepository
	{
		/// <summary>
		/// Получить фиксированные цены по доп.соглашению.
		/// </summary>
		/// <returns>Фиксированные цены</returns>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="addAgreement">AdditionalAgreement</param>
		public static IList<WaterSalesAgreementFixedPrice> GetFixedPricesForAgreement (IUnitOfWork uow, AdditionalAgreement addAgreement)
		{
			WaterSalesAgreementFixedPrice fixedPriceAlias = null;

			var queryResults = uow.Session.QueryOver<WaterSalesAgreementFixedPrice>(() => fixedPriceAlias)
								  .Where(() => fixedPriceAlias.AdditionalAgreement == addAgreement)
								  .List();

			return queryResults;
		}
	}
}
