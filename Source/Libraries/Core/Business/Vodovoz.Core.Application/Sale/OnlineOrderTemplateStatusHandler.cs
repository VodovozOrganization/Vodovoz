using System;
using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
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
				_logger.LogWarning("Попытка остановки шаблона {TemplateId} не в том статусе {TemplateStatus} инициатор {Initiator}",
					template.Id,
					template.Status.GetEnumDisplayName()
					);
				
				return Result.Failure(OnlineOrderTemplateApiErrors.OrderTemplatePauseUnavailable);
			}
			
			template.Status = OnlineOrderTemplateStatus.Paused;
			
			//TODO-6121 уточнить, какие данные храним. Или всегда последнюю попытку, или все
			OnlineOrderTemplateStatusChanged.Create(DateTime.Now, employeeId, customerId, OnlineOrderTemplateStatusChangedType.ToPause);

			return Result.Success();
		}
	}
}
