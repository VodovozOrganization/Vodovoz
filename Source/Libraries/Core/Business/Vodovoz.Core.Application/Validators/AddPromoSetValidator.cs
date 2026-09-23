using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators.Rules;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Validation;

namespace Vodovoz.Core.Application.Validators
{
	public class AddPromoSetValidator : IAddPromoSetValidator
	{
		private readonly IEnumerable<IAddPromoSetRule> _rules;

		public AddPromoSetValidator(IEnumerable<IAddPromoSetRule> rules)
		{
			_rules = rules ?? throw new ArgumentNullException(nameof(rules));
		}
		
		/// <summary>
		/// Проверка на возможность добавления промонабора
		/// </summary>
		/// <returns><c>Result.Success</c>, если можно добавить промонабор,
		/// <c>Result.Failure</c> если нельзя.</returns>
		/// <param name="uow">unit of work</param>
		/// <param name="source">Источник, куда добавляется промонабор</param>
		/// <param name="proSet">Промонабор</param>
		public virtual Result<string> CanAddPromotionalSet(
			IUnitOfWork uow,
			ISaleSource source,
			PromotionalSet proSet
			)
		{
			foreach(var rule in _rules)
			{
				var result = rule.Apply(uow, source, proSet);

				if(result != null)
				{
					return result;
				}
			}
			
			return Result.Success(string.Empty);
		}
	}

	public interface IAddPromoSetValidatorFactory
	{
		IAddPromoSetValidator CreateForOrder();
		IAddPromoSetValidator Create();
	}

	public class AddPromoSetValidatorFactory : IAddPromoSetValidatorFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public AddPromoSetValidatorFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}
		
		public IAddPromoSetValidator CreateForOrder()
		{
			var rules = new List<IAddPromoSetRule>
			{
				_lifetimeScope.Resolve<AddMoreOnePromoSetForNewClientsToOrderRule>(),
				_lifetimeScope.Resolve<AddPromoSetToSelfDeliveryRule>(),
				_lifetimeScope.Resolve<AddPromoSetForNewClientsWithPreviousShipmentRule>(),
				_lifetimeScope.Resolve<AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule>()
			};

			return new AddPromoSetValidator(rules);
		}
		
		public IAddPromoSetValidator Create()
		{
			var rules = new List<IAddPromoSetRule>
			{
				_lifetimeScope.Resolve<AddMoreOnePromoSetForNewClientsRule>(),
				_lifetimeScope.Resolve<AddPromoSetToSelfDeliveryRule>(),
				_lifetimeScope.Resolve<AddPromoSetForNewClientsWithPreviousShipmentRule>(),
				_lifetimeScope.Resolve<AddPromoSetForNotNewClientsOrWithoutPreviousShipmentsRule>()
			};

			return new AddPromoSetValidator(rules);
		}
	}
}
