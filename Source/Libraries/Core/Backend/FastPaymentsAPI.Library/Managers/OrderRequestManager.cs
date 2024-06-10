using System;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.FastPayments;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Managers
{
	public class OrderRequestManager : IOrderRequestManager
	{
		private readonly IDTOManager _dtoManager;
		private readonly ISignatureManager _signatureManager;
		private readonly IFastPaymentSettings _fastPaymentSettings;
		private readonly IFastPaymentFactory _fastPaymentApiFactory;
		private readonly IOrderService _orderService;

		public OrderRequestManager(
			IDTOManager dtoManager,
			ISignatureManager signatureManager,
			IFastPaymentSettings fastPaymentSettings,
			IFastPaymentFactory fastPaymentApiFactory,
			IOrderService orderService)
		{
			_dtoManager = dtoManager ?? throw new ArgumentNullException(nameof(dtoManager));
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
			var orderDTO = GetOrderRegistrationRequestDTO(order, fastPaymentGuid, shopId, phoneNumber, isQr);
			var xmlStringFromOrderDTO = _dtoManager.GetXmlStringFromDTO(orderDTO);

			return _orderService.RegisterOrderAsync(xmlStringFromOrderDTO);
		}
		
		public Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto, Organization organization)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var orderDTO = GetOrderRegistrationRequestDTO(registerOnlineOrderDto, shopId);
			var xmlStringFromOrderDTO = _dtoManager.GetXmlStringFromDTO(orderDTO);

			return _orderService.RegisterOrderAsync(xmlStringFromOrderDTO);
		}

		public Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var orderInfoDTO = _fastPaymentApiFactory.GetOrderInfoRequestDTO(ticket, shopId);
			var xmlStringFromOrderInfoDTO = _dtoManager.GetXmlStringFromDTO(orderInfoDTO);

			return _orderService.GetOrderInfoAsync(xmlStringFromOrderInfoDTO);
		}

		public Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization)
		{
			var shopId = GetShopIdFromOrganization(organization);
			var cancelPaymentRequestDto = _fastPaymentApiFactory.GetCancelPaymentRequestDTO(ticket, shopId);
			var xmlStringFromCancelPaymentRequestDTO = _dtoManager.GetXmlStringFromDTO(cancelPaymentRequestDto);

			return _orderService.CancelPaymentAsync(xmlStringFromCancelPaymentRequestDTO);
		}
		
		public string GetVodovozFastPayUrl(Guid fastPaymentGuid)
		{
			return $"{_fastPaymentSettings.GetVodovozFastPayBaseUrl}/{fastPaymentGuid}";
		}

		private OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(Order order, Guid fastPaymentGuid, int shopId, string phoneNumber = null,
			bool isQr = true)
		{
			var signatureParameters = _fastPaymentApiFactory.GetSignatureParamsForRegisterOrder(order.Id, order.OrderSum, shopId);
			var signature = _signatureManager.GenerateSignature(signatureParameters);
			var orderRegistrationRequestDTO = _fastPaymentApiFactory.GetOrderRegistrationRequestDTO(order.Id, signature, order.OrderSum, shopId);

			if(phoneNumber == null)
			{
				orderRegistrationRequestDTO.ReturnQRImage = 1;
				orderRegistrationRequestDTO.IsQR = 1;
				orderRegistrationRequestDTO.QRTtl = _fastPaymentSettings.GetQRLifetime;
				orderRegistrationRequestDTO.BackUrl = _fastPaymentSettings.GetFastPaymentBackUrl;
			}
			else
			{
				if(isQr)
				{
					orderRegistrationRequestDTO.IsQR = 1;
					orderRegistrationRequestDTO.QRTtl = _fastPaymentSettings.GetPayUrlLifetime;
				}
				
				var backUrl = GetVodovozFastPayUrl(fastPaymentGuid);
				orderRegistrationRequestDTO.BackUrl = backUrl;
				orderRegistrationRequestDTO.BackUrlOk = backUrl;
				orderRegistrationRequestDTO.BackUrlFail = backUrl;
			}

			return orderRegistrationRequestDTO;
		}

		private OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(RequestRegisterOnlineOrderDTO registerOnlineOrderDto, int shopId)
		{
			var signatureParameters =
				_fastPaymentApiFactory.GetSignatureParamsForRegisterOrder(registerOnlineOrderDto.OrderId, registerOnlineOrderDto.OrderSum, shopId);
			var signature = _signatureManager.GenerateSignature(signatureParameters);
			var orderRegistrationRequestDTO =
				_fastPaymentApiFactory.GetOrderRegistrationRequestDTOForOnlineOrder(registerOnlineOrderDto, signature, shopId);
			
			orderRegistrationRequestDTO.QRTtl = _fastPaymentSettings.GetOnlinePayByQRLifetime;
			orderRegistrationRequestDTO.BackUrl = registerOnlineOrderDto.BackUrl;
			orderRegistrationRequestDTO.BackUrlOk = registerOnlineOrderDto.BackUrlOk;
			orderRegistrationRequestDTO.BackUrlFail = registerOnlineOrderDto.BackUrlFail;

			return orderRegistrationRequestDTO;
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
