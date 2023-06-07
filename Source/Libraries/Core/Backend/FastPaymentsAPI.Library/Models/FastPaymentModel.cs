using System;
using System.Collections.Generic;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Services;

namespace FastPaymentsAPI.Library.Models
{
	public class FastPaymentModel : IFastPaymentModel
	{
		private readonly ILogger<FastPaymentModel> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ISignatureManager _signatureManager;
		private readonly FastPaymentFileCache _fastPaymentFileCache;
		private readonly IFastPaymentAPIFactory _fastPaymentApiFactory;
		private readonly IFastPaymentManager _fastPaymentManager;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private readonly IRequestFromConverter _requestFromConverter;

		public FastPaymentModel(
			ILogger<FastPaymentModel> logger,
			IUnitOfWork uow,
			IFastPaymentRepository fastPaymentRepository,
			IOrderRepository orderRepository,
			ISignatureManager signatureManager,
			FastPaymentFileCache fastPaymentFileCache,
			IFastPaymentAPIFactory fastPaymentApiFactory,
			IFastPaymentManager fastPaymentManager,
			IOrganizationRepository organizationRepository,
			IOrganizationParametersProvider organizationParametersProvider,
			IRequestFromConverter requestFromConverter)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_fastPaymentFileCache = fastPaymentFileCache ?? throw new ArgumentNullException(nameof(fastPaymentFileCache));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_fastPaymentManager = fastPaymentManager ?? throw new ArgumentNullException(nameof(fastPaymentManager));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_organizationParametersProvider =
				organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			_requestFromConverter = requestFromConverter ?? throw new ArgumentNullException(nameof(requestFromConverter));
		}

		public Organization GetOrganization(RequestFromType requestFromType)
		{
			var organization = _organizationRepository.GetPaymentFromOrganizationById(_uow, (int)requestFromType);
			return organization ?? _organizationRepository.GetOrganizationById(_uow, _organizationParametersProvider.VodovozSouthOrganizationId);
		}

		public FastPayment GetFastPaymentByTicket(string ticket)
		{
			return _fastPaymentRepository.GetFastPaymentByTicket(_uow, ticket);
		}
		
		public IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOrder(int orderId)
		{
			return _fastPaymentRepository.GetAllPerformedOrProcessingFastPaymentsByOrder(_uow, orderId);
		}
		
		public IList<FastPayment> GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(int onlineOrderId, decimal onlineOrderSum)
		{
			return _fastPaymentRepository.GetAllPerformedOrProcessingFastPaymentsByOnlineOrder(_uow, onlineOrderId, onlineOrderSum);
		}

		public void SaveNewTicketForOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			int orderId,
			Guid fastPaymentGuid,
			FastPaymentPayType payType,
			Organization organization,
			RequestFromType requestFromType,
			PaymentType paymentType,
			string phoneNumber = null)
		{
			Order order;
			var creationDate = DateTime.Now;
			var paymentByCardFrom = _requestFromConverter.ConvertRequestFromTypeToPaymentFrom(_uow, requestFromType);

			try
			{
				order = _orderRepository.GetOrder(_uow, orderId);
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"При загрузке заказа№ {orderId} произошла ошибка, записываю в файл...");
				CacheData(orderRegistrationResponseDto, orderId, creationDate, fastPaymentGuid, payType, organization, phoneNumber,
					paymentByCardFrom);
				return;
			}

			var fastPayment = _fastPaymentApiFactory.GetFastPayment(
				orderRegistrationResponseDto,
				creationDate,
				fastPaymentGuid,
				order.OrderSum,
				orderId,
				payType,
				organization,
				paymentByCardFrom,
				paymentType,
				order,
				phoneNumber);
			fastPayment.SetProcessingStatus();
			
			try
			{
				Save(fastPayment);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При сохранении платежа произошла ошибка, записываю в файл...");
				CacheData(orderRegistrationResponseDto, orderId, creationDate, fastPaymentGuid, payType, organization, phoneNumber,
					paymentByCardFrom);
			}
		}
		
		public void SaveNewTicketForOnlineOrder(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			Guid fastPaymentGuid,
			int onlineOrderId,
			decimal onlineOrderSum,
			FastPaymentPayType payType,
			Organization organization,
			RequestFromType requestFromType,
			string callbackUrl)
		{
			var creationDate = DateTime.Now;
			var paymentByCardFrom = _requestFromConverter.ConvertRequestFromTypeToPaymentFrom(_uow, requestFromType);
			
			var fastPayment = _fastPaymentApiFactory.GetFastPayment(
				orderRegistrationResponseDto,
				creationDate,
				fastPaymentGuid,
				onlineOrderSum,
				onlineOrderId,
				payType,
				organization,
				paymentByCardFrom,
				PaymentType.PaidOnline,
				null,
				null,
				onlineOrderId,
				callbackUrl);
			fastPayment.SetProcessingStatus();
			
			Save(fastPayment);
		}

		public bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto, out string paymentSignature)
		{
			var signatureParameters = _fastPaymentApiFactory.GetSignatureParamsForValidate(paidOrderInfoDto);
			return _signatureManager.Validate(paidOrderInfoDto.Signature, signatureParameters, out paymentSignature);
		}
		
		public bool UpdateFastPaymentStatus(PaidOrderInfoDTO paidOrderInfoDto, FastPayment fastPayment)
		{
			_uow.Session.Refresh(fastPayment);
			
			if((int)paidOrderInfoDto.Status != (int)fastPayment.FastPaymentStatus)
			{
				UpdateFastPaymentStatus(fastPayment, paidOrderInfoDto.Status, paidOrderInfoDto.StatusDate);
				return true;
			}

			return false;
		}
		
		public void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate)
		{
			var oldStatus = fastPayment.FastPaymentStatus;
			_fastPaymentManager.UpdateFastPaymentStatus(_uow, fastPayment, newStatus, statusDate);
			Save(fastPayment);
			_logger.LogInformation($"Статус платежа с externalId: {fastPayment.ExternalId} изменён c {oldStatus} на {newStatus}");
		}

		private void CacheData(
			OrderRegistrationResponseDTO orderRegistrationResponseDto,
			int orderId,
			DateTime creationDate,
			Guid fastPaymentGuid,
			FastPaymentPayType payType,
			Organization organization,
			string phoneNumber,
			PaymentFrom paymentByCardFrom)
		{
			var fastPaymentDTO = new FastPaymentDTO
			{
				OrderId = orderId,
				CreationDate = creationDate,
				Ticket = orderRegistrationResponseDto.Ticket,
				QRPngBase64 = orderRegistrationResponseDto.QRPngBase64,
				ExternalId = orderId,
				FastPaymentGuid = fastPaymentGuid,
				FastPaymentPayType = payType,
				Organization = organization,
				PhoneNumber = phoneNumber,
				PaymentByCardFrom = paymentByCardFrom
			};

			_fastPaymentFileCache.WritePaymentCache(fastPaymentDTO);
		}
		
		private void Save<TEntity>(TEntity entity)
			where TEntity : class, IDomainObject
		{
			_uow.Save(entity);
			_uow.Commit();
		}
	}
}
