using BitrixApi.Contracts.Dto;
using BitrixApi.Contracts.Dto.Requests;
using Mailganer.Api.Client;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Common;

namespace BitrixApi.Library.Services
{
	public class EmalSendService
	{
		private readonly ILogger<EmalSendService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IEmailSettings _emailSettings;
		private readonly IGenericRepository<CounterpartyEntity> _counterpartyRepository;
		private readonly IGenericRepository<OrganizationEntity> _organizationRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly EmailDirectSender _emailDirectSender;

		public EmalSendService(
			ILogger<EmalSendService> logger,
			IUnitOfWork uow,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IEmailSettings emailSettings,
			IGenericRepository<CounterpartyEntity> counterpartyRepository,
			IGenericRepository<OrganizationEntity> organizationRepository,
			IOrderRepository orderRepository,
			EmailDirectSender emailDirectSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_emailAttachmentsCreateService = emailAttachmentsCreateService ?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_emailDirectSender = emailDirectSender ?? throw new ArgumentNullException(nameof(emailDirectSender));
		}

		public async Task SendDocumentByEmail(SendReportRequest request, CancellationToken cancellationToken)
		{
			if(!IsValidEmail(request.EmailAdress))
			{
				_logger.LogError("Некорректный email адрес: {Email}", request.EmailAdress);
				throw new InvalidOperationException($"Некорректный email адрес: {request.EmailAdress}");
			}

			if(!IsValidInn(request.CounterpartyInn))
			{
				_logger.LogError("Некорректный ИНН контрагента: {CounterpartyInn}", request.CounterpartyInn);
				throw new InvalidOperationException($"Некорректный ИНН контрагента: {request.CounterpartyInn}");
			}

			var counterparty = GetCounterparty(request.CounterpartyInn.ToString());

			if(counterparty == null)
			{
				_logger.LogError("Контрагент с ИНН {CounterpartyInn} не найден", request.CounterpartyInn);
				throw new KeyNotFoundException($"Контрагент с ИНН {request.CounterpartyInn} не найден");
			}

			var organization = GetOrganizationById(request.OrganizationId);

			if(organization == null)
			{
				_logger.LogError("Организация с Id={OrganizationId} не найдена", request.OrganizationId);
				throw new KeyNotFoundException($"Организация с Id={request.OrganizationId} не найдена");
			}

			IEnumerable<EmailAttachment> attachments = Enumerable.Empty<EmailAttachment>();
			var notpaidOrderIds = Enumerable.Empty<int>();
			var messageText = string.Empty;

			switch(request.ReportType)
			{
				case ReportTypeDto.ReconciliationStatement:
					attachments =
						_emailAttachmentsCreateService.CreateRevisionAttachments(counterparty.Id, organization.Id);
					messageText = "Акт сверки";
					break;
				case ReportTypeDto.UnpaidOrdersAccount:
					notpaidOrderIds = GetNotPaidOrderIds(counterparty.Id, organization.Id);
					attachments =
						_emailAttachmentsCreateService.CreateNotPaidOrdersBillAttachments(counterparty.Id, organization.Id, notpaidOrderIds);
					messageText = "Счета по неоплаченным заказам";
					break;
				case ReportTypeDto.TotalAccount:
					notpaidOrderIds = GetNotPaidOrderIds(counterparty.Id, organization.Id);
					attachments =
						_emailAttachmentsCreateService.CreateGeneralBillAttachments(counterparty.Id, organization.Id, notpaidOrderIds);
					messageText = "Общий счет";
					break;
				default:
					throw new InvalidOperationException($"Неизвестный тип отчета {request.ReportType}");
			}

			if(attachments.Count() == 0)
			{
				_logger.LogError("Не удалось создать вложения для письма");
				throw new InvalidOperationException("Не удалось создать вложения для письма");
			}

			var emailMessage =
				CreateEmailMessage(counterparty, request.EmailAdress, messageText, attachments);

			await SendEmail(emailMessage);
		}

		private SendEmailMessage CreateEmailMessage(
			CounterpartyEntity counterparty,
			string email,
			string messageText,
			IEnumerable<EmailAttachment> attachments)
		{
			var instanceId = GetCurrentDatabaseId();

			var emailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = _emailSettings.DefaultEmailSenderName,
					Email = _emailSettings.DefaultEmailSenderAddress
				},
				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = string.IsNullOrWhiteSpace(counterparty.FullName) ? "Уважаемый пользователь" : counterparty.FullName,
						Email = email
					}
				},
				Subject = messageText,
				TextPart = messageText,
				HTMLPart = messageText,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = instanceId
				},
				Attachments = attachments.ToArray()
			};

			return emailMessage;
		}

		private async Task SendEmail(SendEmailMessage emailMessage)
		{
			await _emailDirectSender.SendAsync(emailMessage);
		}

		private int GetCurrentDatabaseId()
		{
			var instanceId = Convert.ToInt32(
				_uow.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			return instanceId;
		}

		private CounterpartyEntity GetCounterparty(string counterpartyInn) =>
			_counterpartyRepository
			.GetFirstOrDefault(
				_uow,
				x => x.INN == counterpartyInn);

		private OrganizationEntity GetOrganizationById(int organizationId) =>
			_organizationRepository
			.GetFirstOrDefault(
				_uow,
				x => x.Id == organizationId);

		private IEnumerable<int> GetNotPaidOrderIds(int counterpartyId, int organizationId) =>
			_orderRepository.GetUnpaidOrdersIds(_uow, counterpartyId, DateTime.MinValue, DateTime.MaxValue, organizationId);

		private bool IsValidEmail(string email)
		{
			try
			{
				if(Regex.IsMatch(email, EmailEntity.EmailRegEx, RegexOptions.None, TimeSpan.FromSeconds(1)))
				{
					return true;
				}
			}
			catch(RegexMatchTimeoutException ex)
			{
				return false;
			}
			catch(Exception ex)
			{
				throw ex;
			}

			return false;
		}

		private bool IsValidInn(string inn)
		{
			var innPattern = @"^\d{10}$|^\d{12}$";
			var isValidInn = Regex.IsMatch(inn, innPattern);
			return isValidInn;
		}
	}
}
