using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Errors.Sale.PromoSets;
using VodovozBusiness.Rules;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Validators.Rules.PromoSets
{
	/// <summary>
	/// Правило добавления более одного промонабора для новых клиентов
	/// </summary>
	public class AddPromoSetForNewClientsWithPreviousShipmentRule : IAddPromoSetRule
	{
		private readonly IFreeLoaderChecker _freeLoaderChecker;

		public AddPromoSetForNewClientsWithPreviousShipmentRule(IFreeLoaderChecker freeLoaderChecker)
		{
			_freeLoaderChecker = freeLoaderChecker ?? throw new ArgumentNullException(nameof(freeLoaderChecker));
		}
		
		public Result<string> Apply(IUnitOfWork uow, ISaleSource source, PromotionalSet proSet)
		{
			if(proSet.PromotionalSetForNewClients
				&& _freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
					uow,
					source.IsSelfDelivery,
					source.Counterparty,
					source.DeliveryPoint)
			)
			{
				return Result.Failure<string>(PromoSetErrors.HasPreviousShipmentToAnotherIndividualClient);
			}

			return null;
		}
	}
}
