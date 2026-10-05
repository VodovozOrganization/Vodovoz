using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <inheritdoc/>
	public class CustomerCartService : ICustomerCartService
	{
		private readonly ILogger<CustomerCartService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IInfoMessageFactory _infoMessageFactory;
		private readonly IPaymentMethodsCreator _paymentMethodsCreator;
		private readonly IDeliveryRulesConditionsCreator _deliveryRulesConditionsCreator;
		private readonly IOnlineOrderTemplateConditionsCreator _templateConditionsCreator;

		public CustomerCartService(
			ILogger<CustomerCartService> logger,
			IUnitOfWork uow,
			IInfoMessageFactory infoMessageFactory,
			IPaymentMethodsCreator paymentMethodsCreator,
			IDeliveryRulesConditionsCreator deliveryRulesConditionsCreator,
			IOnlineOrderTemplateConditionsCreator templateConditionsCreator
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_infoMessageFactory = infoMessageFactory ?? throw new ArgumentNullException(nameof(infoMessageFactory));
			_paymentMethodsCreator = paymentMethodsCreator ?? throw new ArgumentNullException(nameof(paymentMethodsCreator));
			_deliveryRulesConditionsCreator =
				deliveryRulesConditionsCreator ?? throw new ArgumentNullException(nameof(deliveryRulesConditionsCreator));
			_templateConditionsCreator = templateConditionsCreator ?? throw new ArgumentNullException(nameof(templateConditionsCreator));
		}

		/// <inheritdoc/>
		public async Task<OrderConditionsResponse> GetOrderConditionsAsync(OrderConditionsRequest request)
		{
			var paymentMethods = _paymentMethodsCreator.GetPaymentMethods(request.Source);
			var conditions = _deliveryRulesConditionsCreator.Create(_uow, request);
			var templateConditions = _templateConditionsCreator.Create(_uow, request);

			var response = OrderConditionsResponse.Create(
				paymentMethods,
				templateConditions,
				conditions,
				new List<InfoMessage>());

			return response;
		}
	}
}
