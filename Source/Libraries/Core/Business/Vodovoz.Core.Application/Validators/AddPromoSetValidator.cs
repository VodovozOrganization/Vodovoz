using System;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators.Rules;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Validators
{
	public class DesktopAddPromoSetValidator
	{
		private readonly IAddNomenclatureToSaleValidator _addNomenclatureToSaleValidator;
		private readonly IAddPromoSetValidator _addPromoSetValidator;

		public DesktopAddPromoSetValidator(
			IAddNomenclatureToSaleValidator addNomenclatureToSaleValidator,
			IAddPromoSetValidator addPromoSetValidator
			)
		{
			_addNomenclatureToSaleValidator =
				addNomenclatureToSaleValidator ?? throw new ArgumentNullException(nameof(addNomenclatureToSaleValidator));
			_addPromoSetValidator = addPromoSetValidator ?? throw new ArgumentNullException(nameof(addPromoSetValidator));
		}

		public Result<string> CanAddPromotionalSet(IUnitOfWork uow, IAddSaleItemSource saleItemSource, PromotionalSet proSet)
		{
			var canAddNomenclatureResult = _addNomenclatureToSaleValidator.CanAddNomenclatures(saleItemSource);

			if(canAddNomenclatureResult.IsFailure)
			{
				return Result.Failure<string>(canAddNomenclatureResult.Errors);
			}
			
			return _addPromoSetValidator.CanAddPromotionalSet(uow, saleItemSource, proSet);
		}
	}
	
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
		/// <param name="addSaleItemSource">Источник, куда добавляется промонабор</param>
		/// <param name="proSet">Промонабор</param>
		public virtual Result<string> CanAddPromotionalSet(
			IUnitOfWork uow,
			IAddSaleItemSource addSaleItemSource,
			PromotionalSet proSet
			)
		{
			foreach(var rule in _rules)
			{
				var result = rule.Validate(uow, addSaleItemSource, proSet);

				if(result != null)
				{
					return result;
				}
			}
			
			return Result.Success(string.Empty);
		}
	}

	public interface IAddPromoSetValidator
	{
		Result<string> CanAddPromotionalSet(IUnitOfWork uow, IAddSaleItemSource addSaleItemSource, PromotionalSet proSet);
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
