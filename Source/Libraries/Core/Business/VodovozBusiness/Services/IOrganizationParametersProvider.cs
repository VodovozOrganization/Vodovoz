using System;

namespace Vodovoz.Services
{
    public interface IOrganizationParametersProvider
    {
        int GetCashlessOrganisationId { get; }
        int GetCashOrganisationId { get; }
        int BeveragesWorldOrganizationId { get; }
        int SosnovcevOrganizationId { get; }
        int VodovozOrganizationId { get; }
        int VodovozSouthOrganizationId { get; }
        int VodovozNorthOrganizationId { get; }
		int VodovozEastOrganizationId { get; }
        int VodovozDeshitsOrganizationId { get; }
        int CommonCashDistributionOrganisationId { get; }
        TimeSpan LatestCreateTimeForSouthOrganizationInByCardOrder { get; }
    }
}
