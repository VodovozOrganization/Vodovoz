namespace Vodovoz.Services
{
    public interface IOrganisationParametersProvider
    {
        int GetCashlessOrganisationId { get; }
        int GetCashOrganisationId { get; }
    }
}