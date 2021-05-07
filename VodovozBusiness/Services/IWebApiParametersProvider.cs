namespace Vodovoz.Services
{
    public interface IWebApiParametersProvider
    {
        string CompanyPhoneNumber { get; }
        int ComplaintSourceId { get; }
    }
}