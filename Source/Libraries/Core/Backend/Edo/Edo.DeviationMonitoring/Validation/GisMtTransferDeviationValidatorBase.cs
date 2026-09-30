using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation.Docflow;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Data.Repositories;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Базовый валидатор отклонения по результату обработки кодов в ГИС МТ
	/// для задач трансфера
	/// </summary>
	public abstract class GisMtTransferDeviationValidatorBase : EdoTransferDeviationValidatorBase
	{
		private readonly EdoDeviationMonitoringOptions _options;

		protected GisMtTransferDeviationValidatorBase(IOptionsSnapshot<EdoDeviationMonitoringOptions> options)
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
		/// Проверяет, отслеживается ли результат ГИС МТ по задаче трансфера
		/// </summary>
		/// <param name="transferTask">Состояние задачи трансфера</param>
		protected bool IsGisMtTracked(EdoTransferTaskMonitoringNode transferTask) =>
			EdoDocflowDeviationRules.IsGisMtTracked(
				transferTask.TaskCreationTime,
				_options.GisMtTrackingStartDate);
	}
}
