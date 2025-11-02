using System;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.Settings.Database.Delivery
{
	public class DeliveryPointSettings : IDeliveryPointSettings
	{
		private readonly ISettingsController _settingsController;

		public DeliveryPointSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int EducationalInstitutionDeliveryPointCategoryId =>
			_settingsController.GetIntValue("educational_institution_delivery_point_category_id");
	}
}
