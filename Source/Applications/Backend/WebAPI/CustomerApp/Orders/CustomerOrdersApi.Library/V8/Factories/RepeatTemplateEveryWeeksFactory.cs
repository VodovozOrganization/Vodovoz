using System;
using CustomerApp.Contracts.Sale.Templates;
using Vodovoz.Settings.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	/// <inheritdoc/>
	public class RepeatTemplateEveryWeeksFactory : IRepeatTemplateEveryWeeksFactory
	{
		private readonly IOnlineOrderTemplateSettings _repeatTemplateSettings;

		public RepeatTemplateEveryWeeksFactory(IOnlineOrderTemplateSettings repeatTemplateSettings)
		{
			_repeatTemplateSettings = repeatTemplateSettings ?? throw new ArgumentNullException(nameof(repeatTemplateSettings));
		}
		
		/// <inheritdoc/>
		public RepeatTemplateEveryWeeks Create()
		{
			return new RepeatTemplateEveryWeeks
			{
				ManualSettingsMin = _repeatTemplateSettings.RepeatEveryWeeksMin,
				ManualSettingsMax = _repeatTemplateSettings.RepeatEveryWeeksMax,
				Options = _repeatTemplateSettings.RepeatEveryWeeksDefault()
			};
		}
	}
}
