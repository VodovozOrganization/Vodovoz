using System;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s.Requests;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;

namespace FastPaymentsAPI.Library.Managers
{
	public class OrderRequestManager : IOrderRequestManager
	{
		private readonly IDTOManager _dtoManager;
		private readonly ISignatureManager _signatureManager;
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IFastPaymentAPIFactory _fastPaymentApiFactory;
		private readonly IOrderService _orderService;

		public OrderRequestManager(
			IDTOManager dtoManager,
			ISignatureManager signatureManager,
			IFastPaymentParametersProvider fastPaymentParametersProvider,
			IFastPaymentAPIFactory fastPaymentApiFactory,
			IOrderService orderService)
		{
			_dtoManager = dtoManager ?? throw new ArgumentNullException(nameof(dtoManager));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
		}

		public Task<OrderRegistrationResponseDTO> RegisterOrder(Order order, string phoneNumber = null)
		{
			var orderDTO = GetOrderRegistrationRequestDTO(order, phoneNumber);
			var xmlStringFromOrderDTO = _dtoManager.GetXmlStringFromDTO(orderDTO);

			return _orderService.RegisterOrderAsync(xmlStringFromOrderDTO);
		}

		public Task<OrderInfoResponseDTO> GetOrderInfo(string ticket)
		{
			var orderInfoDTO = _fastPaymentApiFactory.GetOrderInfoRequestDTO(ticket);
			var xmlStringFromOrderInfoDTO = _dtoManager.GetXmlStringFromDTO(orderInfoDTO);

			return _orderService.GetOrderInfoAsync(xmlStringFromOrderInfoDTO);
		}

		public Task<CancelPaymentResponseDTO> CancelPayment(string ticket)
		{
			var cancelPaymentRequestDto = _fastPaymentApiFactory.GetCancelPaymentRequestDTO(ticket);
			var xmlStringFromCancelPaymentRequestDTO = _dtoManager.GetXmlStringFromDTO(cancelPaymentRequestDto);

			return _orderService.CancelPaymentAsync(xmlStringFromCancelPaymentRequestDTO);
		}

		private OrderRegistrationRequestDTO GetOrderRegistrationRequestDTO(Order order, string phoneNumber = null)
		{
			var signatureParameters = _fastPaymentApiFactory.GetSignatureParamsForRegisterOrder(order.Id, order.OrderSum);
			var signature = _signatureManager.GenerateSignature(signatureParameters);
			var orderRegistrationRequestDTO = _fastPaymentApiFactory.GetOrderRegistrationRequestDTO(
				order.Id, signature, order.OrderSum, _fastPaymentParametersProvider.GetFastPaymentBackUrl);

			if(phoneNumber == null)
			{
				orderRegistrationRequestDTO.IsQR = 1;
				orderRegistrationRequestDTO.ReturnQRImage = 1;
				orderRegistrationRequestDTO.QRTtl = _fastPaymentParametersProvider.GetQRLifetime;
			}

			return orderRegistrationRequestDTO;
		}
	}
}
