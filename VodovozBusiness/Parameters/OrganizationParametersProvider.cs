using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrganizationParametersProvider : IOrganizationParametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public OrganizationParametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int BeveragesWorldOrganizationId => parametersProvider.GetIntValue("beverages_world_organization_id");

        public int SosnovcevOrganizationId => parametersProvider.GetIntValue("sosnovcev_organization_id");
        
        public int VodovozOrganizationId => parametersProvider.GetIntValue("vodovoz_organization_id");
        
        public int VodovozSouthOrganizationId => parametersProvider.GetIntValue("vodovoz_south_organization_id");
        
        public int VodovozNorthOrganizationId => parametersProvider.GetIntValue("vodovoz_north_organization_id");
    }
}