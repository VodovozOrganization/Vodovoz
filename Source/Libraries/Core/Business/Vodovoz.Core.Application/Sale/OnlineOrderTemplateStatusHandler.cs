using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Errors.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderTemplateStatusHandler : IOnlineOrderTemplateStatusHandler
	{
		private readonly ILogger<OnlineOrderTemplateStatusHandler> _logger;
		private readonly IGenericRepository<OnlineOrderTemplateStatusChanged> _templateStatusChangedRepository;

		public OnlineOrderTemplateStatusHandler(
			ILogger<OnlineOrderTemplateStatusHandler> logger,
			IGenericRepository<OnlineOrderTemplateStatusChanged> templateStatusChangedRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_templateStatusChangedRepository =
				templateStatusChangedRepository ?? throw new ArgumentNullException(nameof(templateStatusChangedRepository));
		}
		
		public async Task<Result> PauseTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			)
		{
			if(template.Status == OnlineOrderTemplateStatus.Paused)
			{
				return Result.Success();
			}

			if(template.Status != OnlineOrderTemplateStatus.Active
				&& template.Status != OnlineOrderTemplateStatus.PaymentProblem)
			{
				var initiator = GetInitiatorString(customerId, employeeId);
				_logger.LogWarning("Попытка остановки шаблона {TemplateId} не в том статусе {TemplateStatus} инициатор {Initiator}",
					template.Id,
					template.Status.GetEnumDisplayName(),
					initiator
					);
				
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatePauseUnavailable);
			}
			
			template.Status = OnlineOrderTemplateStatus.Paused;
			await uow.SaveAsync(template, cancellationToken: cancellationToken);

			var changed = await TryUpdateTemplateStatusChanged(
				uow,
				template.Id,
				customerId,
				employeeId,
				OnlineOrderTemplateStatusChangedType.ToPause,
				cancellationToken
				);

			await uow.SaveAsync(changed, cancellationToken: cancellationToken);

			return Result.Success();
		}

		public async Task<Result> ResumeTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			DateTime nextDeliveryDate,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			)
		{
			if(template.Status == OnlineOrderTemplateStatus.Active)
			{
				return Result.Success();
			}

			if(template.Status != OnlineOrderTemplateStatus.Paused)
			{
				var initiator = GetInitiatorString(customerId, employeeId);
				_logger.LogWarning("Попытка возобновления шаблона {TemplateId} не в том статусе {TemplateStatus} инициатор {Initiator}",
					template.Id,
					template.Status.GetEnumDisplayName(),
					initiator
					);
				
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatePauseUnavailable);
			}
			
			template.Resume(nextDeliveryDate);
			await uow.SaveAsync(template.Schedule, cancellationToken: cancellationToken);
			await uow.SaveAsync(template, cancellationToken: cancellationToken);
			
			var changed = await TryUpdateTemplateStatusChanged(
				uow,
				template.Id,
				customerId,
				employeeId,
				OnlineOrderTemplateStatusChangedType.ToResume,
				cancellationToken
				);

			await uow.SaveAsync(changed, cancellationToken: cancellationToken);

			return Result.Success();
		}

		public async Task<Result> ArchiveTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			)
		{
			if(template.Status == OnlineOrderTemplateStatus.Archived)
			{
				return Result.Success();
			}
			
			if(!OnlineOrderTemplate.CanArchive.Contains(template.Status))
			{
				var initiator = GetInitiatorString(customerId, employeeId);
				_logger.LogWarning("Попытка архивации шаблона {TemplateId} не в том статусе {TemplateStatus} инициатор {Initiator}",
					template.Id,
					template.Status.GetEnumDisplayName(),
					initiator
				);
				
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplateArchiveUnavailable);
			}
			
			// Удаление токена рекуррентного платежа
			if(template.RecurrentPayment != null)
			{
				//TODO-6121 удалить его из сервиса, чтобы не было списаний
				var payment = uow.GetById<OnlineRecurrentPayment>(template.RecurrentPayment.Id);
				
				if(payment != null)
				{
					await uow.DeleteAsync(payment, cancellationToken: cancellationToken);
				}
				
				template.RecurrentPayment = null;
			}
			
			template.Status = OnlineOrderTemplateStatus.Archived;
			await uow.SaveAsync(template, cancellationToken: cancellationToken);
			
			var changed = await TryUpdateTemplateStatusChanged(
				uow,
				template.Id,
				customerId,
				employeeId,
				OnlineOrderTemplateStatusChangedType.ToArchive,
				cancellationToken);

			await uow.SaveAsync(changed, cancellationToken: cancellationToken);
			
			return Result.Success();
		}

		private static string GetInitiatorString(int? customerId, int? employeeId) =>
			customerId.HasValue
				? $"клиент id {customerId.Value}"
				: employeeId.HasValue
					? $"сотрудник id {employeeId.Value}"
					: "неизвестный";
		
		private async Task<OnlineOrderTemplateStatusChanged> TryUpdateTemplateStatusChanged(
			IUnitOfWork uow,
			int templateId,
			int? customerId,
			int? employeeId,
			OnlineOrderTemplateStatusChangedType statusChangedType,
			CancellationToken cancellationToken
		)
		{
			var changed = await GetStatusChangedEventAsync(uow, templateId, statusChangedType, cancellationToken);

			if(changed is null)
			{
				changed = OnlineOrderTemplateStatusChanged.Create(
					templateId, DateTime.Now, employeeId, customerId, statusChangedType
				);
			}
			else
			{
				changed.Update(customerId, employeeId);
			}

			return changed;
		}
		
		private async Task<OnlineOrderTemplateStatusChanged> GetStatusChangedEventAsync(
			IUnitOfWork uow,
			int templateId,
			OnlineOrderTemplateStatusChangedType statusChangedType,
			CancellationToken cancellationToken
		)
		{
			var changed = await _templateStatusChangedRepository.GetAsync(
					uow,
					x => x.TemplateId == templateId
						&& x.Type == statusChangedType,
					cancellationToken: cancellationToken)
				;
			
			return changed.Value.FirstOrDefault();
		}
	}
}
