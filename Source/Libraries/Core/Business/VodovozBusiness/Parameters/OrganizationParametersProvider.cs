using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrganizationParametersProvider : IOrganizationParametersProvider
    {
        private readonly IParametersProvider _parametersProvider;

        public OrganizationParametersProvider(IParametersProvider parametersProvider)
        {
            _parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int GetCashlessOrganisationId => _parametersProvider.GetIntValue("cashless_organization_id");
        public int GetCashOrganisationId => _parametersProvider.GetIntValue("cash_organization_id");
        public int BeveragesWorldOrganizationId => _parametersProvider.GetIntValue("beverages_world_organization_id");
        public int SosnovcevOrganizationId => _parametersProvider.GetIntValue("sosnovcev_organization_id");
        public int VodovozOrganizationId => _parametersProvider.GetIntValue("vodovoz_organization_id");
        public int VodovozSouthOrganizationId => _parametersProvider.GetIntValue("vodovoz_south_organization_id");
        public int VodovozNorthOrganizationId => _parametersProvider.GetIntValue("vodovoz_north_organization_id");
        public int VodovozEastOrganizationId => _parametersProvider.GetIntValue("vodovoz_east_organization_id");
        public int VodovozDeshitsOrganizationId => _parametersProvider.GetIntValue("vodovoz_Deshits_organization_id");
        public int CommonCashDistributionOrganisationId =>
            _parametersProvider.GetIntValue("common_cash_distribution_organisation_id");
        public TimeSpan LatestCreateTimeForSouthOrganizationInByCardOrder =>
	        _parametersProvider.GetValue<TimeSpan>("latest_create_time_for_south_organization_in_by_card_order");
    }
}
