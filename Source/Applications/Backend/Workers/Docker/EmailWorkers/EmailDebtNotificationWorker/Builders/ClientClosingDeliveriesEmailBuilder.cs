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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Orders;

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
			if(notificationInfos.Count == 0)
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
			var attachments = _attachmentsService
				.CreateOrderWithoutShipmentForDebtAttachments(orderWithoutShipmentForDebt)
				.ToList();

			if(!attachments.Any())
			{
				throw new InvalidOperationException("Нет вложений");
			}

			var email = SelectEmail(orderWithoutShipmentForDebt.Client);

			var storedEmail = new StoredEmail
			{
				State = StoredEmailStates.WaitingToSend,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				RecipientAddress = email,
				Subject = $"{orderWithoutShipmentForDebt.Client.FullName} от {orderWithoutShipmentForDebt.Organization.FullName}",
				Guid = Guid.NewGuid(),
				Description = "Стоп-отгрузка"
			};

			await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

			var orderWithoutShipmentForDebtEmail = new OrderWithoutShipmentForDebtEmail
			{
				OrderWithoutShipmentForDebt = orderWithoutShipmentForDebt,
				StoredEmail = storedEmail,
				Counterparty = orderWithoutShipmentForDebt.Client
			};

			await uow.SaveAsync(orderWithoutShipmentForDebtEmail, cancellationToken: cancellationToken);

			var clientEmailBody = GenerateClientEmailBody(orderWithoutShipmentForDebt);

			var sendEmailMessage = new SendEmailMessage
			{
				From = new EmailContact
				{
					Name = orderWithoutShipmentForDebt.Organization.FullName,
					Email = orderWithoutShipmentForDebt.Organization.EmailForMailing
				},
				To = new List<EmailContact>
				{
					new()
					{
						Name = orderWithoutShipmentForDebt.Client.FullName,
						Email = email
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
			return client.Emails?.LastOrDefault()?.Address
				?? throw new InvalidOperationException("Нет email");
		}

		private string GenerateClientEmailBody(OrderWithoutShipmentForDebt debt)
		{
			return $@"
				<p>Уважаемый клиент!</p>

				<p>
					Сообщаем, что на текущий момент у вас имеется неоплаченная задолженность по договору.
					Так как просроченная дебиторская задолженность составляет более {_closingDeliveriesSettings.DaysBeforeClosingDeliveries} дней мы вынуждены заблокировать возможность оформлять заказы по безналичному расчету до полного погашения.
				</p>

				<p>Общая задолженность по данным заказам составляет: {debt.DebtSum} руб.</p>

				<p>
					Просим вас произвести оплату в кратчайшие сроки - счет на оплату прикрепили в приложении.
					Если оплата уже была направлена, пожалуйста, пришлите платежное поручение.
				</p>

				<p>
					После поступления оплаты, поставки будут возобновлены.
					При возникновении вопросов по сумме или срокам оплаты вы можете связаться с нами — будем рады помочь.
					Благодарим за сотрудничество и рассчитываем на скорейшее урегулирование вопроса.
				</p>

				<p>С уважением,</p>

				<p>Отдел сопровождения клиентов</p>

				<p>+7(812) 3170000 доб. 700</p>

				<p>client.buh@vodovoz-spb.ru</p>";
		}
	}
}
