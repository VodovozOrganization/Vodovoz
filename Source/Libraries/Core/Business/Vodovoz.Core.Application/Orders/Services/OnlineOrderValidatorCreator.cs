using System;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Domain.Orders;
using Vodovoz.Validation;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <inheritdoc/>
	public class OnlineOrderValidatorCreator : IOnlineOrderValidatorCreator
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public OnlineOrderValidatorCreator(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}
		
		/// <inheritdoc/>
		public IOnlineOrderValidator Create(OnlineOrder onlineOrder)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			OrderFromOnlineOrderValidator onlineOrderValidator;
			
			if(onlineOrder is OnlineOrderV1)
			{
				onlineOrderValidator = scope.ServiceProvider.GetRequiredService<OldOnlineOrderValidator>();
			}
			else
			{
				onlineOrderValidator = scope.ServiceProvider.GetRequiredService<NewOnlineOrderValidator>();
			}
			
			onlineOrderValidator.SetOnlineOrder(onlineOrder);

			return onlineOrderValidator;
		}
	}
}
