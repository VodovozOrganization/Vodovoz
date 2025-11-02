using System;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Settings.Database.Organizations
{
	public class OrganizationSettings : IOrganizationSettings
	{
		private readonly ISettingsController _settingsController;

		public OrganizationSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int GetCashlessOrganisationId => _settingsController.GetIntValue("cashless_organization_id");
		public int GetCashOrganisationId => _settingsController.GetIntValue("cash_organization_id");
		public int BeveragesWorldOrganizationId => _settingsController.GetIntValue("beverages_world_organization_id");
		public int SosnovcevOrganizationId => _settingsController.GetIntValue("sosnovcev_organization_id");
		public int VodovozOrganizationId => _settingsController.GetIntValue("vodovoz_organization_id");
		public int VodovozSouthOrganizationId => _settingsController.GetIntValue("vodovoz_south_organization_id");
		public int VodovozNorthOrganizationId => _settingsController.GetIntValue("vodovoz_north_organization_id");
		public int VodovozEastOrganizationId => _settingsController.GetIntValue("vodovoz_east_organization_id");
		public int VodovozDeshitsOrganizationId => _settingsController.GetIntValue("vodovoz_Deshits_organization_id");
		public int VodovozMbnOrganizationId => _settingsController.GetIntValue("vodovoz_mbn_organization_id");
		public int KulerServiceOrganizationId => _settingsController.GetIntValue("kuler_service_organization_id");
		public int CommonCashDistributionOrganisationId =>
			_settingsController.GetIntValue("common_cash_distribution_organisation_id");
		public TimeSpan LatestCreateTimeForSouthOrganizationInByCardOrder =>
			_settingsController.GetValue<TimeSpan>("latest_create_time_for_south_organization_in_by_card_order");
	}
}
