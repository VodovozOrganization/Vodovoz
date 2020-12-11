namespace Vodovoz.Services
{
    public interface IOrganizationParametersProvider
    {
        int BeveragesWorldOrganizationId { get; }
        int SosnovcevOrganizationId { get; }
        int VodovozOrganizationId { get; }
        int VodovozSouthOrganizationId { get; }
        int VodovozNorthOrganizationId { get; }
    }
}