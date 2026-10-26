using System;
using System.Linq;
using Vodovoz.Settings.Sale;

namespace Vodovoz.Settings.Database.Sale
{
	public class OnlineOrderTemplateSettings : IOnlineOrderTemplateSettings
	{
		private readonly ISettingsController _settingsController;

		public OnlineOrderTemplateSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		
		/// <inheritdoc/>
		public int GetMaximumNumberActiveOnlineOrderTemplates =>
			_settingsController.GetValue<int>("OnlineOrderTemplate.MaximumNumberActiveOnlineOrderTemplates");

		/// <inheritdoc/>
		public int GetNumberNextDeliveriesToDisplay =>
			_settingsController.GetValue<int>("OnlineOrderTemplate.NumberNextDeliveriesToDisplay");

		/// <inheritdoc/>
		public int RepeatEveryWeeksMin =>
			_settingsController.GetValue<int>("OnlineOrderTemplate.RepeatEveryWeeksMin");

		/// <inheritdoc/>
		public int RepeatEveryWeeksMax =>
			_settingsController.GetValue<int>("OnlineOrderTemplate.RepeatEveryWeeksMax");

		/// <inheritdoc/>
		public int MonthDayMin => _settingsController.GetValue<int>("OnlineOrderTemplate.MonthDayMin");
		
		/// <inheritdoc/>
		public int MonthDayMax => _settingsController.GetValue<int>("OnlineOrderTemplate.MonthDayMax");

		/// <inheritdoc/>
		public int[] RepeatEveryWeeksDefault()
		{
			var defaultValue = _settingsController.GetValue<string>("OnlineOrderTemplate.RepeatEveryWeeksDefault");

			if(string.IsNullOrWhiteSpace(defaultValue))
			{
				throw new InvalidOperationException("В базе не настроены параметры повторения автозаказа");
			}
			
			return defaultValue
				.Split(',')
				.Select(x => Convert.ToInt32(x))
				.ToArray();
		}
	}
}
