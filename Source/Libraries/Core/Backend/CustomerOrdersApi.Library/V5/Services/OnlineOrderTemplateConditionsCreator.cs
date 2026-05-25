using System;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V5.Carts;
using CustomerOrders.Contracts.V5.Orders.Discounts;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <inheritdoc/>
	public class OnlineOrderTemplateConditionsCreator : IOnlineOrderTemplateConditionsCreator
	{
		private readonly ILogger<OnlineOrderTemplateConditionsCreator> _logger;
		private readonly ICheckUserBasketCacheService _checkUserBasketCacheService;
		private readonly IOnlineOrderTemplateFromOnlineOrderValidator _onlineOrderTemplateValidator;

		public OnlineOrderTemplateConditionsCreator(
			ILogger<OnlineOrderTemplateConditionsCreator> logger,
			ICheckUserBasketCacheService checkUserBasketCacheService,
			IOnlineOrderTemplateFromOnlineOrderValidator onlineOrderTemplateValidator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_checkUserBasketCacheService = checkUserBasketCacheService ?? throw new ArgumentNullException(nameof(checkUserBasketCacheService));
			_onlineOrderTemplateValidator = onlineOrderTemplateValidator ?? throw new ArgumentNullException(nameof(onlineOrderTemplateValidator));
		}

		/// <inheritdoc/>
		public async Task<OnlineAutoOrderConditions> CreateAsync(IUnitOfWork uow, Guid? checkId, DiscountDto discount)
		{
			var templateConditions = await _checkUserBasketCacheService.GetCachedVerificationAsync(checkId);

			if(templateConditions != null)
			{
				var result = _onlineOrderTemplateValidator.Validate(uow, templateConditions.Request);
				
				if(result.IsFailure)
				{
					_logger.LogWarning("Не прошли проверку на возможность добавления автозаказа {ErrorMessage}", result.GetErrorsString());
					return new OnlineAutoOrderConditions
					{
						IsAvailable = false,
						Discount = null
					};
				}
				
				return new OnlineAutoOrderConditions
				{
					IsAvailable = true,
					Discount = discount
				};
			}
			
			return new OnlineAutoOrderConditions
			{
				IsAvailable = false,
				Discount = null
			};
		}
	}
}
