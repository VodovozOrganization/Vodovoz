using System;

namespace Vodovoz.Parameters
{
    public class OrganisationParametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public OrganisationParametersProvider()
        {
            parametersProvider = ParametersProvider.Instance;
        }

        private int GetIntValue(string parameterId)
        {
            if(!parametersProvider.ContainsParameter(parameterId)) {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})" );
            }
                
            string value = parametersProvider.GetParameterValue(parameterId);

            if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return result;
        }
        
        public int CommonCashOrganisationDistributionId {
            get {
                string parameterId = "common_cash_organisation_distribution_id";
                return GetIntValue(parameterId);
            }
        }
    }
}