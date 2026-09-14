using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Edo;

namespace Edo.DeviationMonitoring.Options
{
	/// <summary>
	/// Заполняет настройки сервиса мониторинга отклонений из настроек базы данных
	/// </summary>
	public class ConfigureEdoDeviationMonitoringOptions : IConfigureOptions<EdoDeviationMonitoringOptions>
	{
		private readonly IEdoDeviationSettings _edoDeviationSettings;

		public ConfigureEdoDeviationMonitoringOptions(IEdoDeviationSettings edoDeviationSettings)
		{
			_edoDeviationSettings = edoDeviationSettings
				?? throw new ArgumentNullException(nameof(edoDeviationSettings));
		}

		/// <inheritdoc/>
		public void Configure(EdoDeviationMonitoringOptions options)
		{
			options.RegistrationWorkerInterval = _edoDeviationSettings.RegistrationWorkerInterval;
			options.ResolvingWorkerInterval = _edoDeviationSettings.ResolvingWorkerInterval;
			options.BatchSize = _edoDeviationSettings.BatchSize;
			options.GisMtTrackingStartDate = _edoDeviationSettings.GisMtTrackingStartDate;

			if(options.RegistrationWorkerInterval <= TimeSpan.Zero)
			{
				throw new InvalidOperationException(
					"Интервал работы воркера регистрации отклонений ЭДО должен быть больше нуля");
			}

			if(options.ResolvingWorkerInterval <= TimeSpan.Zero)
			{
				throw new InvalidOperationException(
					"Интервал работы воркера снятия отклонений ЭДО должен быть больше нуля");
			}

			if(options.BatchSize < 1)
			{
				throw new InvalidOperationException(
					"Размер пачки сервиса мониторинга отклонений ЭДО должен быть не меньше одного");
			}

			if(options.GisMtTrackingStartDate == default)
			{
				throw new InvalidOperationException(
					"Дата начала отслеживания результата обработки кодов в ГИС МТ должна быть заполнена");
			}
		}
	}
}
