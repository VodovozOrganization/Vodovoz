using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Базовый валидатор отклонения по результату обработки кодов в ГИС МТ
	/// </summary>
	public abstract class GisMtTaskDeviationValidatorBase : EdoTaskDeviationValidatorBase
	{
		private readonly EdoDeviationMonitoringOptions _options;

		protected GisMtTaskDeviationValidatorBase(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
		{
			if(options is null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_options = options.Value;
		}

		/// <summary>
		/// Результат ГИС МТ приходит после того, как документооборот уже завершил задачу
		/// </summary>
		public override bool IsAppliesToFinishedTask => true;

		/// <summary>
		/// Проверяет, отслеживается ли результат ГИС МТ по задаче
		/// </summary>
		/// <param name="task">Состояние задачи ЭДО</param>
		protected bool IsGisMtTracked(EdoTaskMonitoringNode task) =>
			EdoDocflowDeviationRules.IsGisMtTracked(
				task.OrderDeliveryDate ?? task.TaskCreationTime,
				_options.GisMtTrackingStartDate);
	}
}
