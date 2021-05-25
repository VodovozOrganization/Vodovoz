namespace Vodovoz.Services
{
    public interface IDriverApiParametersProvider
    {
        string CompanyPhoneNumber { get; }
        int ComplaintSourceId { get; }
    }
}