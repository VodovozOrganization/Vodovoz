using System;
using System.Collections.Generic;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Responses;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Cryptography;

namespace FastPaymentsAPI.Library.Models
{
	public class FastPaymentService : IFastPaymentService
	{
		private readonly ILogger<FastPaymentService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IOrderSettings _orderSettings;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ISignatureManager _signatureManager;
		private readonly FastPaymentFileCache _fastPaymentFileCache;
		private readonly IFastPaymentFactory _fastPaymentApiFactory;
		private readonly IFastPaymentManager _fastPaymentManager;
		private readonly IRequestFromConverter _requestFromConverter;
		private readonly IOrganizationForOnlinePaymentService _organizationForOnlinePaymentService;

		public FastPaymentService(
			ILogger<FastPaymentService> logger,
			IUnitOfWork uow,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			IOrderRepository orderRepository,
			ISignatureManager signatureManager,
			FastPaymentFileCache fastPaymentFileCache,
			IFastPaymentFactory fastPaymentApiFactory,
			IFastPaymentManager fastPaymentManager,
			IRequestFromConverter requestFromConverter,
			IOrganizationForOnlinePaymentService organizationForOnlinePaymentService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_fastPaymentFileCache = fastPaymentFileCache ?? throw new ArgumentNullException(nameof(fastPaymentFileCache));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_fastPaymentManager = fastPaymentManager ?? throw new ArgumentNullException(nameof(fastPaymentManager));
			_requestFromConverter = requestFromConverter ?? throw new ArgumentNullException(nameof(requestFromConverter));
			_organizationForOnlinePaymentService =
				organizationForOnlinePaymentService ?? throw new ArgumentNullException(nameof(organizationForOnlinePaymentService));
		}

		public Result<Organization> GetOrganization(
			TimeSpan requestTime,
			FastPaymentRequestFromType fastPaymentRequestFromType,
			Order order = null)
		{
			return order is null 
				? _organizationForOnlinePaymentService.GetOrganizationForFastPayment(_uow, requestTime, fastPaymentRequestFromType)
				: _organizationForOnlinePaymentService.GetOrganizationForFastPayment(_uow, order, requestTime, fastPaymentRequestFromType);
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
			FastPaymentRequestFromType fastPaymentRequestFromType,
			PaymentType paymentType,
			string phoneNumber = null,
			bool isDryRun = false)
		{
			Order order;
			var creationDate = DateTime.Now;
			var paymentByCardFrom = _requestFromConverter.ConvertRequestFromTypeToPaymentFrom(_uow, fastPaymentRequestFromType);

			try
			{
				order = _orderRepository.GetOrder(_uow, orderId);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При загрузке заказа№ {OrderId} произошла ошибка, записываю в файл...", orderId);
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
				fastPaymentRequestFromType,
				order,
				phoneNumber);
			fastPayment.SetProcessingStatus();
			
			try
			{
				if(!isDryRun)
				{
					Save(fastPayment);
				}
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
			FastPaymentRequestFromType fastPaymentRequestFromType,
			string callbackUrl)
		{
			var creationDate = DateTime.Now;
			var paymentByCardFrom = _requestFromConverter.ConvertRequestFromTypeToPaymentFrom(_uow, fastPaymentRequestFromType);
			
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
				fastPaymentRequestFromType,
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

			_uow.Save(fastPayment);

			var @event = FastPaymentStatusUpdatedEvent.Create(fastPayment, fastPayment.FastPaymentStatus);
			_uow.Save(@event);

			_uow.Commit();
			
			_logger.LogInformation(
				"Статус платежа с externalId: {ExternalId} изменён c {OldStatus} на {NewStatus}",
				fastPayment.ExternalId,
				oldStatus,
				newStatus);
		}

		/// <inheritdoc/>
		public void CreateWrongFastPaymentEvent(string orderNumber, string bankSignature, int shopId, string paymentSignature)
		{
			var @event = WrongSignatureFromReceivedFastPaymentEvent.Create(orderNumber, bankSignature, shopId, paymentSignature);
			Save(@event);
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
				FastPaymentPayType = payType.ToString(),
				OrganizationId = organization.Id,
				PhoneNumber = phoneNumber,
				PaymentByCardFromId = paymentByCardFrom.Id
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
