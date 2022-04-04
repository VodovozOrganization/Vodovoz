using System;
using System.Collections.Generic;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Responses;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Managers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;

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

		public FastPaymentModel(
			ILogger<FastPaymentModel> logger,
			IUnitOfWork uow,
			IFastPaymentRepository fastPaymentRepository,
			IOrderRepository orderRepository,
			ISignatureManager signatureManager,
			FastPaymentFileCache fastPaymentFileCache,
			IFastPaymentAPIFactory fastPaymentApiFactory,
			IFastPaymentManager fastPaymentManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_signatureManager = signatureManager ?? throw new ArgumentNullException(nameof(signatureManager));
			_fastPaymentFileCache = fastPaymentFileCache ?? throw new ArgumentNullException(nameof(fastPaymentFileCache));
			_fastPaymentApiFactory = fastPaymentApiFactory ?? throw new ArgumentNullException(nameof(fastPaymentApiFactory));
			_fastPaymentManager = fastPaymentManager ?? throw new ArgumentNullException(nameof(fastPaymentManager));
		}

		public FastPayment GetFastPaymentByTicket(string ticket)
		{
			return _fastPaymentRepository.GetFastPaymentByTicket(_uow, ticket);
		}
		
		public IList<FastPayment> GetAllPerformedOrProcessingFastPayments(int orderId)
		{
			return _fastPaymentRepository.GetAllPerformedOrProcessingFastPaymentsByOrder(_uow, orderId);
		}

		public void SaveNewTicket(OrderRegistrationResponseDTO orderRegistrationResponseDto, int orderId, string phoneNumber = null)
		{
			Order order;
			var creationDate = DateTime.Now;
			
			try
			{
				order = _orderRepository.GetOrder(_uow, orderId);
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"При загрузке заказа№ {orderId} произошла ошибка, записываю в файл...");
				CachData(orderRegistrationResponseDto, orderId, creationDate);
				return;
			}

			var fastPayment = _fastPaymentApiFactory.GetFastPayment(orderRegistrationResponseDto, order, creationDate, phoneNumber);
			fastPayment.SetProcessingStatus();
			
			try
			{
				Save(fastPayment);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "При сохранении платежа произошла ошибка, записываю в файл...");
				CachData(orderRegistrationResponseDto, orderId, creationDate);
			}
		}

		public bool ValidateSignature(PaidOrderInfoDTO paidOrderInfoDto)
		{
			var signatureParameters = _fastPaymentApiFactory.GetSignatureParamsForValidate(paidOrderInfoDto);
			return _signatureManager.Validate(paidOrderInfoDto.Signature, signatureParameters);
		}
		
		public void UpdateFastPaymentStatus(PaidOrderInfoDTO paidOrderInfoDto)
		{
			var fastPayment = _fastPaymentRepository.GetFastPaymentByTicket(_uow, paidOrderInfoDto.Ticket);

			if(fastPayment != null && (int)paidOrderInfoDto.Status != (int)fastPayment.FastPaymentStatus)
			{
				UpdateFastPaymentStatus(fastPayment, paidOrderInfoDto.Status, paidOrderInfoDto.StatusDate);
			}
		}
		
		public void UpdateFastPaymentStatus(FastPayment fastPayment, FastPaymentDTOStatus newStatus, DateTime statusDate)
		{
			var oldStatus = fastPayment.FastPaymentStatus;
			_fastPaymentManager.UpdateFastPaymentStatus(_uow, fastPayment, newStatus, statusDate);
			Save(fastPayment);
			_logger.LogInformation($"Статус платежа с externalId: {fastPayment.ExternalId} изменён c {oldStatus} на {newStatus}");
		}

		private void CachData(OrderRegistrationResponseDTO orderRegistrationResponseDto, int orderId, DateTime creationDate)
		{
			var fastPaymentDTO = new FastPaymentDTO
			{
				OrderId = orderId,
				CreationDate = creationDate,
				Ticket = orderRegistrationResponseDto.Ticket,
				QRPngBase64 = orderRegistrationResponseDto.QRPngBase64,
				ExternalId = (int)orderRegistrationResponseDto.Id
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
