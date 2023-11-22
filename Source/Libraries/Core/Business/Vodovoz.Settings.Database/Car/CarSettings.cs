using System;
using System.Linq;
using Vodovoz.Settings.Car;

namespace Vodovoz.Settings.Database.Car
{
	public class CarSettings : ICarSettings
	{
		private readonly ISettingsController _settingsController;

		public CarSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int[] FirstInSelectionListCarModelId
		{
			get
			{
				var modelIdsString = _settingsController.GetValue<string>(nameof(FirstInSelectionListCarModelId).FromPascalCaseToSnakeCase());

				var modelIdsSplitedString = modelIdsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				
				var modelIds = modelIdsSplitedString
					.Select(x => int.Parse(x.Trim()))
					.ToArray();

				return modelIds;
			}
		}
	}
}
