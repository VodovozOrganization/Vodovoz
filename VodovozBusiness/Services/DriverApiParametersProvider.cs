using Vodovoz.Parameters;

namespace Vodovoz.Services
{
    public class DriverApiParametersProvider : IDriverApiParametersProvider
    {
        private readonly IParametersProvider parametersProvider;

        public DriverApiParametersProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new System.ArgumentNullException(nameof(parametersProvider));
        }

        public int ComplaintSourceId => parametersProvider.GetIntValue("web_api_complaint_source_id");

        public string CompanyPhoneNumber => parametersProvider.GetStringValue("web_api_company_phone_number");

    }
}
