using BitrixApi.Library.Services;
using EmailDebtNotificationWorker.DTO;
using Mailjet.Api.Abstractions;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Builders
{
	public class ClientClosingDeliveriesEmailBuilder : IClientClosingDeliveriesEmailBuilder
	{
		private readonly ILogger<ClientClosingDeliveriesEmailBuilder> _logger;
		private readonly IEmailAttachmentsCreateService _attachmentsService;
		private readonly IClosingDeliveriesSettings _closingDeliveriesSettings;

		public ClientClosingDeliveriesEmailBuilder(
			ILogger<ClientClosingDeliveriesEmailBuilder> logger,
			IEmailAttachmentsCreateService attachmentsService,
			IClosingDeliveriesSettings closingDeliveriesSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_attachmentsService = attachmentsService ?? throw new ArgumentNullException(nameof(attachmentsService));
			_closingDeliveriesSettings = closingDeliveriesSettings ?? throw new ArgumentNullException(nameof(closingDeliveriesSettings));
		}

		public async Task<IReadOnlyList<SendEmailMessage>> Build(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			if(!notificationInfos.Any())
			{
				return Array.Empty<SendEmailMessage>();
			}

			var sendEmailMessages = new List<SendEmailMessage>(notificationInfos.Count);

			foreach(var info in notificationInfos)
			{
				var orderWithoutShipmentForDebt = info.OrderWithoutShipmentForDebt;

				try
				{
					var sendEmailMessage = await Create(uow, orderWithoutShipmentForDebt, cancellationToken);

					sendEmailMessages.Add(sendEmailMessage);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка подготовки письма клиенту для счета без отгрузки на долг {OrderWithoutShipmentForDebt}", orderWithoutShipmentForDebt.Id);
				}
			}

			return sendEmailMessages;
		}

		private async Task<SendEmailMessage> Create(
			IUnitOfWork uow,
			OrderWithoutShipmentForDebt orderWithoutShipmentForDebt,
			CancellationToken cancellationToken)
		{
			var emailSentFrom = orderWithoutShipmentForDebt.Organization.ClosingDeliveriesNotificationEmailFrom;

			if(string.IsNullOrWhiteSpace(emailSentFrom))
			{
				throw new InvalidOperationException($"У организации {orderWithoutShipmentForDebt.Organization.Id} не заполнен {nameof(emailSentFrom)}");
			}

			var orderWihoutShipmentForDebtAttachment = _attachmentsService.CreateOrderWithoutShipmentForDebtAttachments(orderWithoutShipmentForDebt);

			var revisionStartDate = new DateTime(orderWithoutShipmentForDebt.CreateDate.Value.Year, 1, 1);
			var revisionEndDate = DateTime.Today.AddDays(-1);
			var revisionAttachment = _attachmentsService.CreateRevisionAttachments(orderWithoutShipmentForDebt.Counterparty.Id, orderWithoutShipmentForDebt.Organization.Id, revisionStartDate, revisionEndDate);

			var attachments = orderWihoutShipmentForDebtAttachment.Concat(revisionAttachment).ToList();

			if(!attachments.Any())
			{
				throw new InvalidOperationException("Нет вложений");
			}

			var emailSentTo = SelectEmail(orderWithoutShipmentForDebt.Client);

			var storedEmail = new StoredEmail
			{
				State = StoredEmailStates.WaitingToSend,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				RecipientAddress = emailSentTo,
				Subject = $"{orderWithoutShipmentForDebt.Client.FullName} (ИНН: {orderWithoutShipmentForDebt.Client.INN}) от {orderWithoutShipmentForDebt.Organization.FullName}",
				Guid = Guid.NewGuid(),
				Description = "Стоп-отгрузка"
			};

			await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var closingDeliveriesEmail = new ClosingDeliveriesEmail
			{
				OrderWithoutShipmentForDebt = orderWithoutShipmentForDebt,
				StoredEmail = storedEmail,
				Counterparty = orderWithoutShipmentForDebt.Client
			};

			await uow.SaveAsync(closingDeliveriesEmail, cancellationToken: cancellationToken);

			var clientEmailBody = GenerateClientEmailBody(orderWithoutShipmentForDebt);

			var sendEmailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = orderWithoutShipmentForDebt.Organization.FullName,
					Email = emailSentFrom
				},
				To = new List<EmailContact>
				{
					new()
					{
						Name = orderWithoutShipmentForDebt.Client.FullName,
						Email = emailSentTo
					}
				},
				Subject = storedEmail.Subject,
				TextPart = clientEmailBody,
				HTMLPart = clientEmailBody,
				Attachments = attachments,
				Payload = new EmailPayload
				{
					Id = storedEmail.Id,
					Trackable = true
				}
			};

			return sendEmailMessage;
		}

		private string SelectEmail(Counterparty client)
		{
			if(client.Emails is null || !client.Emails.Any())
			{
				throw new InvalidOperationException($"У клиента {client.Id} отсутствуют любые email.");
			}

			var billEmail = client.Emails
				.LastOrDefault(x => x.EmailType?.EmailPurpose is EmailPurpose.ForBills)
				?.Address;

			if(!string.IsNullOrWhiteSpace(billEmail))
			{
				return billEmail;
			}

			var defaultEmail = client.Emails
				.LastOrDefault()
				?.Address;

			if(!string.IsNullOrWhiteSpace(defaultEmail))
			{
				return defaultEmail;
			}

			throw new InvalidOperationException($"Для клиента {client.Id} не удалось подобрать подходящий email для отправки уведомления о блокировке поставок.");
		}

		private string GenerateClientEmailBody(OrderWithoutShipmentForDebt debt)
		{
			return $@"
				<p>Уважаемый клиент!</p>

				<p>
					Сообщаем, что на текущий момент по договору имеется просроченная задолженность. 
					В связи с тем, что срок просрочки превышает {_closingDeliveriesSettings.DaysBeforeClosingDeliveries} дней, мы вынуждены временно ограничить возможность оформления заказов по безналичному расчету до полного погашения задолженности.
				</p>

				<p><strong>Общая сумма задолженности по указанным заказам составляет {debt.DebtSum} руб.</strong></p>

				<p>
					Просим вас произвести оплату в ближайшее время. Счет на оплату приложен к письму.
				</p>

				<p>
					Обращаем внимание: в случае отсутствия оплаты в течение 2 дней в ваш адрес будет направлена претензия. 
					Если оплата уже произведена, пожалуйста, направьте платежное поручение в ответ на данное письмо.
				</p>

				<p>
					После поступления оплаты поставки будут возобновлены в полном объеме.
				</p>

				<p>
					Если у вас возникнут вопросы по сумме задолженности или срокам оплаты, пожалуйста, свяжитесь с нами — мы будем рады помочь.
				</p>

				<p>Благодарим за сотрудничество и рассчитываем на скорейшее урегулирование вопроса.</p>

				<p>С уважением,</p>
				<p><strong>Отдел сопровождения клиентов</strong></p>
				<p>+7 (812) 317-00-00, доб. 700</p>
				<p>client.buh@vodovoz-spb.ru</p>";
		}
	}
}
