using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForOnlinePaymentService : IOrganizationForOnlinePaymentService
	{
		private readonly IOrderSettings _orderSettings;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IOrganizationForOrderFromSet _organizationForOrderFromSet;

		public OrganizationForOnlinePaymentService(
			IOrderSettings orderSettings,
			IOrganizationSettings organizationSettings,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_organizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
		}
		
		public Result<Organization> GetOrganizationForFastPayment(
			IUnitOfWork uow,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType)
		{
			if(requestFromType == FastPaymentRequestFromType.FromMobileAppByQr
				|| requestFromType == FastPaymentRequestFromType.FromSiteByQr)
			{
				return GetOrganizationByPaymentType(uow, requestTime, requestFromType);
			}

			throw new InvalidOperationException(
				$"Нельзя подбирать организацию не для ИПЗ. Используйте другую перегрузку метода {nameof(GetOrganizationForFastPayment)}");
		}
		
		public Result<Organization> GetOrganizationForFastPayment(
			IUnitOfWork uow,
			Order order,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType)
		{
			var orderOrganization = order.Contract?.Organization;
			
			if(orderOrganization is null)
			{
				return Result.Failure<Organization>(Vodovoz.Errors.Orders.FastPaymentErrors.OrderContractNotFound);
			}
			
			var orderOurOrganizationResult = CheckOrderOrganization(uow, order.OurOrganization, requestTime, requestFromType);

			if(orderOurOrganizationResult.IsSuccess && orderOurOrganizationResult.Value != null)
			{
				return orderOurOrganizationResult.Value;
			}

			var clientWorksThroughOrganization =
				CheckOrderOrganization(uow, order.Client.WorksThroughOrganization, requestTime, requestFromType);

			if(clientWorksThroughOrganization.IsSuccess && clientWorksThroughOrganization.Value != null)
			{
				return clientWorksThroughOrganization.Value;
			}

			if(!string.IsNullOrWhiteSpace(order.OrderPartsIds) || orderOrganization.Id == _organizationSettings.KulerServiceOrganizationId)
			{
				return GetOrganizationByRequestType(uow, orderOrganization, requestTime, requestFromType);
			}

			return GetOrganizationByPaymentType(uow, requestTime, requestFromType);
		}

		private Result<Organization> GetOrganizationByPaymentType(
			IUnitOfWork uow,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType)
		{
			var organizationsSettings = GetOrganizationsSettings(uow, requestFromType);
			var organization = _organizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, organizationsSettings);
			return organization;
		}

		private Result<Organization> CheckOrderOrganization(
			IUnitOfWork uow,
			Organization orderOrganization,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType)
		{
			return orderOrganization is null
				? Result.Success<Organization>(null)
				: GetOrganizationByRequestType(uow, orderOrganization, requestTime, requestFromType);
		}
		
		private Result<Organization> GetOrganizationByRequestType(
			IUnitOfWork uow,
			Organization orderOrganization,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType)
		{
			switch(requestFromType)
			{
				case FastPaymentRequestFromType.FromDesktopByCard:
				case FastPaymentRequestFromType.FromDesktopByQr:
					return !orderOrganization.AvangardShopId.HasValue
						? Result.Failure<Organization>(Vodovoz.Errors.Orders.FastPaymentErrors.OrganizationNotRegisteredInAvangard)
						: Result.Success(orderOrganization);
				case FastPaymentRequestFromType.FromDriverAppByQr:
				case FastPaymentRequestFromType.FromMobileAppByQr:
				case FastPaymentRequestFromType.FromSiteByQr:
					return !orderOrganization.AvangardShopId.HasValue
						? GetOrganizationByPaymentType(uow, requestTime, requestFromType)
						: Result.Success(orderOrganization);
				default:
					throw new InvalidOperationException("Запрос из неизвестного источника");
			}
		}
		
		private IOrganizations GetOrganizationsSettings(IUnitOfWork uow, FastPaymentRequestFromType requestFromType)
		{
			switch(requestFromType)
			{
				case FastPaymentRequestFromType.FromDesktopByQr:
					return uow.GetAll<SmsQrPaymentTypeOrganizationSettings>().SingleOrDefault();
				case FastPaymentRequestFromType.FromDriverAppByQr:
					return uow.GetAll<DriverAppQrPaymentTypeOrganizationSettings>().SingleOrDefault();
				case FastPaymentRequestFromType.FromSiteByQr:
					return uow.GetAll<OnlinePaymentTypeOrganizationSettings>()
						.SingleOrDefault(x => x.PaymentFrom.Id == _orderSettings.GetPaymentByCardFromSiteByQrCodeId);
				case FastPaymentRequestFromType.FromDesktopByCard:
					return uow.GetAll<OnlinePaymentTypeOrganizationSettings>()
						.SingleOrDefault(x => x.PaymentFrom.Id == _orderSettings.GetPaymentByCardFromAvangardId);
				case FastPaymentRequestFromType.FromMobileAppByQr:
					return uow.GetAll<OnlinePaymentTypeOrganizationSettings>()
						.SingleOrDefault(x => x.PaymentFrom.Id == _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId);
				default:
					throw new ArgumentOutOfRangeException(nameof(requestFromType), requestFromType, null);
			}
		}
	}
}
