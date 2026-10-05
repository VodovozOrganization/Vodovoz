using System;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
using CustomerOrdersApi.Library.V8.Services.Validators;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <inheritdoc/>
	public class OnlineOrderTemplateConditionsCreator : IOnlineOrderTemplateConditionsCreator
	{
		private readonly ILogger<OnlineOrderTemplateConditionsCreator> _logger;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IOnlineOrderTemplateFromOnlineOrderValidator _onlineOrderTemplateValidator;

		public OnlineOrderTemplateConditionsCreator(
			ILogger<OnlineOrderTemplateConditionsCreator> logger,
			IDiscountReasonSettings discountReasonSettings,
			IOnlineOrderTemplateFromOnlineOrderValidator onlineOrderTemplateValidator
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_onlineOrderTemplateValidator = onlineOrderTemplateValidator ?? throw new ArgumentNullException(nameof(onlineOrderTemplateValidator));
		}
		
		public DiscountDto DiscountData { get; private set; }

		/// <inheritdoc/>
		public OnlineAutoOrderConditions Create(IUnitOfWork uow, OrderConditionsRequest request)
		{
			var discountReason = uow.GetById<DiscountReasonBase>(_discountReasonSettings.AutoOrderDiscountReasonId);

			if(discountReason is null)
			{
				throw new InvalidOperationException("В базе не установлен параметр, отвечающий за идентификатор скидки по автозаказу");
			}
			
			DiscountData = DiscountDto.Create(discountReason.ValueType == DiscountUnits.money, discountReason.Value);
			return GetAutoOrderConditions(uow, request);
		}

		private OnlineAutoOrderConditions GetAutoOrderConditions(
			IUnitOfWork uow,
			OrderConditionsRequest request)
		{
			return CanCreateTemplate(uow, request);
		}

		private OnlineAutoOrderConditions CanCreateTemplate(IUnitOfWork uow, OrderConditionsRequest request)
		{
			var result = _onlineOrderTemplateValidator.Validate(uow, request);
				
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
				Discount = DiscountData
			};
		}
	}
}
