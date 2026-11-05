using System;
using System.Linq;
using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Errors.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderTemplateStatusHandler : IOnlineOrderTemplateStatusHandler
	{
		private readonly ILogger<OnlineOrderTemplateStatusHandler> _logger;

		public OnlineOrderTemplateStatusHandler(
			ILogger<OnlineOrderTemplateStatusHandler> logger
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		
		public Result PauseTemplate(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId)
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
			uow.Save(template);
			
			//TODO-6121 уточнить, какие данные храним. Или всегда последнюю попытку, или все
			var changed = OnlineOrderTemplateStatusChanged.Create(
				template.Id, DateTime.Now, employeeId, customerId, OnlineOrderTemplateStatusChangedType.ToPause);
			uow.Save(changed);

			return Result.Success();
		}

		public Result ResumeTemplate(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId)
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
			
			template.Status = OnlineOrderTemplateStatus.Active;
			uow.Save(template);
			
			//TODO-6121 уточнить, какие данные храним. Или всегда последнюю попытку, или все
			var changed = OnlineOrderTemplateStatusChanged.Create(
				template.Id, DateTime.Now, employeeId, customerId, OnlineOrderTemplateStatusChangedType.ToResume);
			uow.Save(changed);

			return Result.Success();
		}

		public Result ArchiveTemplate(IUnitOfWork uow, OnlineOrderTemplate template, int? customerId, int? employeeId)
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
					uow.Delete(payment);
				}
				
				template.RecurrentPayment = null;
			}
			
			template.Status = OnlineOrderTemplateStatus.Archived;
			uow.Save(template);
			
			var changed = OnlineOrderTemplateStatusChanged.Create(
				template.Id, DateTime.Now, null, customerId, OnlineOrderTemplateStatusChangedType.ToArchive);
			uow.Save(changed);
			
			return Result.Success();
		}

		private static string GetInitiatorString(int? customerId, int? employeeId) =>
			customerId.HasValue
				? $"клиент id {customerId.Value}"
				: employeeId.HasValue
					? $"сотрудник id {employeeId.Value}"
					: "неизвестный";
	}
}
