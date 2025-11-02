using System;
using System.Threading.Tasks;
using Core.Infrastructure;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Services;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.FastPayments;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Managers
{
	public class OrderRequestManager : IOrderRequestManager
	{
		private readonly ISignatureManager _signatureManager;
		private readonly IFastPaymentSettings _fastPaymentSettings;
		private readonly IFastPaymentFactory _fastPaymentApiFactory;
		private readonly IOrderService _orderService;

		public OrderRequestManager(
			ISignatureManager signatureManager,
			IFastPaymentSettings fastPaymentSettings,
			IFastPaymentFactory fastPaymentApiFactory,
			IOrderService orderService)
		{
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_fastPaymentSettings =
				fastPaymentSettings ?? throw new ArgumentNullException(nameof(fastPaymentSettings));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}

		public Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, Guid fastPaymentGuid, Organization organization,
			string phoneNumber = null, bool isQr = true)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var orderDTO = GetOrderRegistrationRequestDto(order, fastPaymentGuid, shopId, phoneNumber, isQr);
			var xmlStringFromOrderDTO = orderDTO.ToXmlString();

			return _orderService.RegisterOrderAsync(xmlStringFromOrderDTO);
		}
		
		public Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var orderDTO = GetOrderRegistrationRequestDto(registerOnlineOrderDto, shopId, fastPaymentRequestFromType);
			var xmlStringFromOrderDTO = orderDTO.ToXmlString();

			return _orderService.RegisterOrderAsync(xmlStringFromOrderDTO);
		}

		public Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var orderInfoDTO = _fastPaymentApiFactory.GetOrderInfoRequestDTO(ticket, shopId);
			var xmlStringFromOrderInfoDTO = orderInfoDTO.ToXmlString();

			return _orderService.GetOrderInfoAsync(xmlStringFromOrderInfoDTO);
		}

		public Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var cancelPaymentRequestDto = _fastPaymentApiFactory.GetCancelPaymentRequestDTO(ticket, shopId);
			var xmlStringFromCancelPaymentRequestDTO = cancelPaymentRequestDto.ToXmlString();

			return _orderService.CancelPaymentAsync(xmlStringFromCancelPaymentRequestDTO);
		}
		
		public string GetVodovozFastPayUrl(Guid fastPaymentGuid)
		{
			return $"{_fastPaymentSettings.GetVodovozFastPayBaseUrl}/{fastPaymentGuid}";
		}

		private OrderRegistrationRequestDTO GetOrderRegistrationRequestDto(Order order, Guid fastPaymentGuid, int shopId, string phoneNumber = null,
			bool isQr = true)
		{
			var signatureParameters =
				_fastPaymentApiFactory.GetSignatureParamsForRegisterOrder(order.Id, order.OrderSum, shopId);
			var signature = _signatureManager.GenerateSignature(signatureParameters);
			var orderRegistrationRequestDto =
				_fastPaymentApiFactory.GetOrderRegistrationRequestDTO(order.Id, signature, order.OrderSum, shopId);

			if(phoneNumber == null)
			{
				orderRegistrationRequestDto.ReturnQRImage = 1;
				orderRegistrationRequestDto.IsQR = 1;
				orderRegistrationRequestDto.QRTtl = _fastPaymentSettings.GetQRLifetime;
				orderRegistrationRequestDto.BackUrl = _fastPaymentSettings.GetFastPaymentBackUrl;
			}
			else
			{
				if(isQr)
				{
					orderRegistrationRequestDto.IsQR = 1;
					orderRegistrationRequestDto.QRTtl = _fastPaymentSettings.GetPayUrlLifetime;
				}
				
				var backUrl = GetVodovozFastPayUrl(fastPaymentGuid);
				orderRegistrationRequestDto.BackUrl = backUrl;
				orderRegistrationRequestDto.BackUrlOk = backUrl;
				orderRegistrationRequestDto.BackUrlFail = backUrl;
			}

			return orderRegistrationRequestDto;
		}

		private OrderRegistrationRequestDTO GetOrderRegistrationRequestDto(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			int shopId,
			FastPaymentRequestFromType fastPaymentRequestFromType)
		{
			var signatureParameters =
				_fastPaymentApiFactory.GetSignatureParamsForRegisterOrder(
					registerOnlineOrderDto.OrderId, registerOnlineOrderDto.OrderSum, shopId);
			var signature = _signatureManager.GenerateSignature(signatureParameters);
			var orderRegistrationRequestDto =
				_fastPaymentApiFactory.GetOrderRegistrationRequestDTOForOnlineOrder(registerOnlineOrderDto, signature, shopId);
			
			orderRegistrationRequestDto.QRTtl = _fastPaymentSettings.GetOnlinePayByQRLifetime;

			if(fastPaymentRequestFromType == FastPaymentRequestFromType.FromMobileAppByQr)
			{
				orderRegistrationRequestDto.ReturnQRImage = 1;
			}

			return orderRegistrationRequestDto;
		}

		private int GetShopIdFromOrganization(Organization organization)
		{
			int shopId;
			if(organization == null)
			{
				shopId = _fastPaymentSettings.GetDefaultShopId;
			}
			else if(organization.AvangardShopId.HasValue)
			{
				shopId = organization.AvangardShopId.Value;
			}
			else
			{
				shopId = default(int);
			}

			return shopId;
		}
	}
}
