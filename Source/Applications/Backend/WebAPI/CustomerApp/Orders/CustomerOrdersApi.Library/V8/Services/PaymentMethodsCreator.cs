using System;
using System.Collections.Generic;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Factories.DeliveryConditions;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <inheritdoc/>
	public class PaymentMethodsCreator : IPaymentMethodsCreator
	{
		private readonly VodovozWebSitePaymentMethodFactory _vodovozWebSitePaymentMethodFactory;
		private readonly MobileAppPaymentMethodFactory _mobileAppPaymentMethodFactory;

		public PaymentMethodsCreator(
			VodovozWebSitePaymentMethodFactory vodovozWebSitePaymentMethodFactory,
			MobileAppPaymentMethodFactory mobileAppPaymentMethodFactory
			)
		{
			_vodovozWebSitePaymentMethodFactory =
				vodovozWebSitePaymentMethodFactory ?? throw new ArgumentNullException(nameof(vodovozWebSitePaymentMethodFactory));
			_mobileAppPaymentMethodFactory =
				mobileAppPaymentMethodFactory ?? throw new ArgumentNullException(nameof(mobileAppPaymentMethodFactory));
		}
		
		/// <inheritdoc/>
		public IEnumerable<PaymentMethod> GetPaymentMethods(ExternalSource source)
		{
			return source switch
			{
				ExternalSource.VodovozWebSite => _vodovozWebSitePaymentMethodFactory.Create(),
				ExternalSource.MobileApp => _mobileAppPaymentMethodFactory.Create(),
				_ => throw new ArgumentOutOfRangeException(
					nameof(source),
					source,
					"Для пришедшего значения не реализована логика получения методов оплат")
			};
		}
	}
}
