using Edo.DeviationMonitoring.Errors;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Edo.DeviationMonitoring.Validation.Sources
{
	/// <summary>
	/// По заявке ЭДО не создана задача.
	/// Валидатор проверяет заявки, а не задачи, поэтому реализует отдельный интерфейс
	/// </summary>
	public class TaskNotCreatedValidator : IEdoRequestDeviationValidator
	{
		/// <summary>
		/// Название стадии: у заявки без задачи стадии документооборота еще нет
		/// </summary>
		private const string _stageName = "Заявка";

		/// <inheritdoc/>
		public EdoDeviationType DeviationType => EdoDeviationType.TaskNotCreated;

		/// <inheritdoc/>
		public bool IsFallback => false;

		/// <summary>
		/// В выборку попадают только заявки без задачи, поэтому условие валидатора
		/// относится к любой проверяемой заявке
		/// </summary>
		/// <param name="request">Заявка ЭДО без задачи</param>
		public bool IsApplicable(EdoRequestMonitoringNode request) => request != null;

		/// <inheritdoc/>
		public Result Validate(
			EdoRequestMonitoringNode request,
			EdoDeviationSource source,
			DateTime checkTime)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(source is null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			var elapsed = checkTime - request.RequestTime;

			if(elapsed <= source.Timeout)
			{
				return Result.Success();
			}

			var deviation = new EdoDeviationValidationResult
			{
				DeviationSourceId = source.Id,
				EdoTaskId = null,
				EdoRequestId = request.RequestId,
				StageName = _stageName,
				StageStartTime = request.RequestTime,
				Threshold = source.Timeout,
				Details =
					$"Заявка создана {EdoDeviationTextFormatter.FormatTime(request.RequestTime)}, "
					+ $"задача ЭДО не создана за "
					+ $"{EdoDeviationTextFormatter.FormatElapsed(elapsed, source.Timeout)}"
			};

			return new EdoDeviationError(DeviationType, deviation);
		}
	}
}
