using BitrixApi.Library.Services;
using DateTimeHelpers;
using Email.Infrastructure.Factories;
using Email.Infrastructure.Generators;
using EmailDebtNotificationWorker.Services.Common.Generators;
using EmailDebtNotificationWorker.Services.Common.Selectors;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Organizations;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace EmailDebtNotificationWorker.Services.ChainStoreDelayLetters
{
	/// <inheritdoc/>
	public class ChainStoreDelayLettersService : IChainStoreDelayLettersService
	{
		private const string _emailSubjectFormat = "Просрочка платежа по договору № {0} - просим оплатить.";
		private const string _storedEmailDescription = "Напоминание об оплате (сети/тендеры)";

		private readonly ILogger<ChainStoreDelayLettersService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IGenericRepository<ChainStoreDelayLetter> _letterRepository;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IChainStoreDelayLettersSettings _chainStoreDelayLettersSettings;
		private readonly IClientEmailSelector _clientEmailSelector;
		private readonly IEmailAttachmentsCreateService _emailAttachmentsCreateService;
		private readonly IEmailBodyGenerator _emailBodyGenerator;
		private readonly IEmailLinkGenerator _emailLinkGenerator;
		private readonly IEmailMessageFactory _emailMessageFactory;
		private readonly IBus _bus;

		public ChainStoreDelayLettersService(
			ILogger<ChainStoreDelayLettersService> logger,
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IEmailRepository emailRepository,
			IGenericRepository<ChainStoreDelayLetter> letterRepository,
			ICounterpartySettings counterpartySettings,
			IOrganizationSettings organizationSettings,
			IChainStoreDelayLettersSettings chainStoreDelayLettersSettings,
			IClientEmailSelector clientEmailSelector,
			IEmailAttachmentsCreateService emailAttachmentsCreateService,
			IEmailBodyGenerator emailBodyGenerator,
			IEmailLinkGenerator emailLinkGenerator,
			IEmailMessageFactory emailMessageFactory,
			IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_letterRepository = letterRepository ?? throw new ArgumentNullException(nameof(letterRepository));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_chainStoreDelayLettersSettings = chainStoreDelayLettersSettings
				?? throw new ArgumentNullException(nameof(chainStoreDelayLettersSettings));
			_clientEmailSelector = clientEmailSelector ?? throw new ArgumentNullException(nameof(clientEmailSelector));
			_emailAttachmentsCreateService = emailAttachmentsCreateService
				?? throw new ArgumentNullException(nameof(emailAttachmentsCreateService));
			_emailBodyGenerator = emailBodyGenerator ?? throw new ArgumentNullException(nameof(emailBodyGenerator));
			_emailLinkGenerator = emailLinkGenerator ?? throw new ArgumentNullException(nameof(emailLinkGenerator));
			_emailMessageFactory = emailMessageFactory ?? throw new ArgumentNullException(nameof(emailMessageFactory));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <inheritdoc/>
		public async Task<int> CollectLetters(CancellationToken cancellationToken)
		{
			var moscowNow = DateTime.UtcNow.ToMoscowDateTime();
			var today = moscowNow.Date;
			var daysBeforePaymentDate = _chainStoreDelayLettersSettings.DaysBeforePaymentDate;

			using(var uow = _uowFactory.CreateWithoutRoot($"Сервис {nameof(ChainStoreDelayLettersService)}. Отбор заказов"))
			{
				var windowOrders = await _orderRepository.GetChainStoreDelayOrdersAsync(
					uow,
					today,
					daysBeforePaymentDate,
					_counterpartySettings.CounterpartyFromTenderId,
					_organizationSettings.VodovozOrganizationId,
					cancellationToken);

				// строки, не отправленные до расчётной даты, переводятся в отдельное состояние
				// до проверки пустого окна: ранний выход её не должен пропускать
				var expiredLetters = _letterRepository
					.Get(uow, x => x.Status == ChainStoreDelayLetterStatus.SendRequired && x.EstimatedPaymentDate <= today)
					.ToList();

				foreach(var expiredLetter in expiredLetters)
				{
					expiredLetter.Status = ChainStoreDelayLetterStatus.PaymentDateReached;
					expiredLetter.LastUpdateDate = moscowNow;

					await uow.SaveAsync(expiredLetter, cancellationToken: cancellationToken);
				}

				var expiredLettersCount = expiredLetters.Count;

				if(windowOrders.Count == 0)
				{
					if(expiredLettersCount > 0)
					{
						await uow.CommitAsync(cancellationToken);
					}

					_logger.LogInformation(
						"Нет заказов в окне для писем по заказам сетей и тендеров, переведено в \"наступила расчётная дата\" {ExpiredLettersCount}",
						expiredLettersCount);
					return 0;
				}

				var windowOrderIds = windowOrders.Select(x => x.OrderId).ToArray();

				var existingLetters = _letterRepository
					.Get(uow, x => windowOrderIds.Contains(x.OrderId))
					.ToDictionary(x => x.OrderId);

				var newLettersCount = 0;
				var repeatedLettersCount = 0;

				foreach(var order in windowOrders)
				{
					if(!existingLetters.TryGetValue(order.OrderId, out var letter))
					{
						var newLetter = new ChainStoreDelayLetter
						{
							Status = ChainStoreDelayLetterStatus.SendRequired,
							CreationDate = moscowNow,
							OrderId = order.OrderId,
							CounterpartyId = order.CounterpartyId,
							EstimatedPaymentDate = order.EstimatedPaymentDate
						};

						await uow.SaveAsync(newLetter, cancellationToken: cancellationToken);
						newLettersCount++;
						continue;
					}

					if(letter.Status == ChainStoreDelayLetterStatus.NoClientEmail
						|| letter.Status == ChainStoreDelayLetterStatus.NoSenderEmail)
					{
						letter.Status = ChainStoreDelayLetterStatus.SendRequired;
						letter.LastUpdateDate = moscowNow;

						await uow.SaveAsync(letter, cancellationToken: cancellationToken);
						repeatedLettersCount++;
					}
				}

				if(newLettersCount + repeatedLettersCount + expiredLettersCount > 0)
				{
					await uow.CommitAsync(cancellationToken);
				}

				_logger.LogInformation(
					"Отбор заказов для писем сетей и тендеров: в окне {WindowOrdersCount}, новых {NewLettersCount}, "
					+ "повторных {RepeatedLettersCount}, переведено в \"наступила расчётная дата\" {ExpiredLettersCount}",
					windowOrders.Count,
					newLettersCount,
					repeatedLettersCount,
					expiredLettersCount);

				return newLettersCount + repeatedLettersCount;
			}
		}

		/// <inheritdoc/>
		public async Task SendLetters(CancellationToken cancellationToken)
		{
			var today = DateTime.UtcNow.ToMoscowDateTime().Date;

			int[] letterIds;

			using(var uow = _uowFactory.CreateWithoutRoot($"Сервис {nameof(ChainStoreDelayLettersService)}. Поиск писем для отправки"))
			{
				// просроченные заказы писем не получают
				letterIds = _letterRepository
					.Get(uow, x => x.Status == ChainStoreDelayLetterStatus.SendRequired && x.EstimatedPaymentDate > today)
					.Select(x => x.Id)
					.ToArray();
			}

			var errorsCount = 0;

			foreach(var letterId in letterIds)
			{
				cancellationToken.ThrowIfCancellationRequested();

				try
				{
					await ProcessLetter(letterId, today, cancellationToken);
				}
				catch(OperationCanceledException)
				{
					throw;
				}
				catch(Exception ex)
				{
					errorsCount++;

					_logger.LogError(ex, "Ошибка при формировании письма по записи истории {LetterId}", letterId);

					await SaveLetterError(letterId, ex.Message, cancellationToken);
				}
			}

			if(errorsCount > 0)
			{
				_logger.LogWarning(
					"Письма по заказам сетей и тендеров: строк к отправке {LettersCount}, с технической ошибкой {ErrorsCount}",
					letterIds.Length,
					errorsCount);
			}
		}

		private async Task SaveLetterError(int letterId, string error, CancellationToken cancellationToken)
		{
			try
			{
				using(var uow = _uowFactory.CreateWithoutRoot($"Сервис {nameof(ChainStoreDelayLettersService)}. Сохранение ошибки письма"))
				{
					var letter = await uow.Session.GetAsync<ChainStoreDelayLetter>(letterId, cancellationToken);

					if(letter == null)
					{
						return;
					}

					letter.LastError = error;
					letter.LastUpdateDate = DateTime.UtcNow.ToMoscowDateTime();

					await uow.SaveAsync(letter, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось сохранить ошибку формирования письма по записи истории {LetterId}", letterId);
			}
		}

		private async Task ProcessLetter(int letterId, DateTime today, CancellationToken cancellationToken)
		{
			SendEmailMessage message;

			using(var uow = _uowFactory.CreateWithoutRoot($"Сервис {nameof(ChainStoreDelayLettersService)}. Формирование письма"))
			{
				var letter = await uow.Session.GetAsync<ChainStoreDelayLetter>(letterId, cancellationToken);

				if(letter == null || letter.Status != ChainStoreDelayLetterStatus.SendRequired)
				{
					return;
				}

				var order = await uow.Session.GetAsync<VodovozOrder>(letter.OrderId, cancellationToken)
					?? throw new InvalidOperationException($"Заказ {letter.OrderId} не найден");

				if(order.OrderPaymentStatus == OrderPaymentStatus.Paid)
				{
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.OrderPaidBeforeSending, cancellationToken);
					return;
				}

				var contract = order.Contract
					?? throw new InvalidOperationException($"У заказа {order.Id} нет договора");
				var organization = contract.Organization
					?? throw new InvalidOperationException($"У договора заказа {order.Id} нет организации");
				var client = order.Client;

				if(organization.DisableDebtMailing)
				{
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.OrganizationDebtMailingDisabled, cancellationToken);
					return;
				}

				if(client.DisableDebtMailing)
				{
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.CounterpartyDebtMailingDisabled, cancellationToken);
					return;
				}

				if(_emailRepository.GetLastBulkEmailEvent(uow, client.Id)?.EventType == BulkEmailEventType.Unsubscribing)
				{
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.ClientUnsubscribed, cancellationToken);
					return;
				}

				if(string.IsNullOrWhiteSpace(organization.EmailForChainStoreNotifications))
				{
					_logger.LogWarning(
						"Организация {OrganizationId} не имеет e-mail для уведомлений сетям. Письмо по заказу {OrderId} не отправлено",
						organization.Id,
						order.Id);
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.NoSenderEmail, cancellationToken);
					return;
				}

				var clientEmail = _clientEmailSelector.SelectEmailForDebtNotification(client);

				if(string.IsNullOrWhiteSpace(clientEmail))
				{
					_logger.LogWarning(
						"Клиент {ClientId} не имеет подходящего e-mail. Письмо по заказу {OrderId} не отправлено",
						client.Id,
						order.Id);
					await SaveLetterStatus(uow, letter, ChainStoreDelayLetterStatus.NoClientEmail, cancellationToken);
					return;
				}

				// ошибки формирования вложений не перехватываются: это техническая ошибка, формирование повторится
				var attachments = _emailAttachmentsCreateService
					.CreateOrdersBillsAttachments(client.Id, organization.Id, new[] { order.Id })
					.ToList();

				var deliveryYear = (order.DeliveryDate ?? today).Year;

				attachments.AddRange(_emailAttachmentsCreateService.CreateRevisionAttachments(
					client.Id,
					organization.Id,
					new DateTime(deliveryYear, 1, 1),
					today));

				var subject = string.Format(_emailSubjectFormat, order.Id);

				var storedEmail = _emailMessageFactory.CreateStoredEmail(subject, clientEmail, _storedEmailDescription);
				await uow.SaveAsync(storedEmail, cancellationToken: cancellationToken);

				if(!storedEmail.Guid.HasValue)
				{
					_logger.LogError("Не удалось получить Guid письма по заказу {OrderId} для ссылки отписки", order.Id);
					throw new InvalidOperationException($"Не удалось получить Guid письма по заказу {order.Id} для ссылки отписки");
				}

				var letterEmail = new ChainStoreDelayLetterEmail
				{
					StoredEmail = storedEmail,
					Counterparty = client,
					OrganizationId = organization.Id
				};
				await uow.SaveAsync(letterEmail, cancellationToken: cancellationToken);

				var body = _emailBodyGenerator.GenerateChainStoreDelayLetterBody(
					contract.Number,
					order.Id,
					letter.EstimatedPaymentDate,
					_chainStoreDelayLettersSettings.ContactPhone,
					organization.EmailForChainStoreNotifications,
					_emailLinkGenerator.GetUnsubscribeLink(storedEmail.Guid.Value));

				message = _emailMessageFactory.CreateSendEmailMessage(
					uow,
					storedEmail,
					client.FullName,
					organization.FullName,
					organization.EmailForChainStoreNotifications,
					attachments,
					clientEmail,
					subject,
					body);

				letter.Status = ChainStoreDelayLetterStatus.Sent;
				letter.CounterpartyEmailId = letterEmail.Id;
				letter.LastError = null;
				letter.LastUpdateDate = DateTime.UtcNow.ToMoscowDateTime();

				await uow.SaveAsync(letter, cancellationToken: cancellationToken);

				// письмо, запись в истории писем клиента и состояние сохраняются одной транзакцией
				await uow.CommitAsync(cancellationToken);
			}

			await PublishEmailMessage(letterId, message, cancellationToken);
		}

		private async Task SaveLetterStatus(
			IUnitOfWork uow,
			ChainStoreDelayLetter letter,
			ChainStoreDelayLetterStatus status,
			CancellationToken cancellationToken)
		{
			letter.Status = status;
			letter.LastError = null;
			letter.LastUpdateDate = DateTime.UtcNow.ToMoscowDateTime();

			await uow.SaveAsync(letter, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Письмо по заказу {OrderId} не отправлено, состояние: {Status}",
				letter.OrderId,
				status);
		}

		private async Task PublishEmailMessage(int letterId, SendEmailMessage message, CancellationToken cancellationToken)
		{
			try
			{
				await _bus.Publish(message, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при публикации сообщения на отправку письма по заказу сети или тендера. StoredEmailId = {StoredEmailId}",
					message.Payload.Id);

				await SaveLetterError(letterId, $"Ошибка передачи письма в отправку почты: {ex.Message}", cancellationToken);
			}
		}
	}
}
