using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class NomenclaturePlanParametersProvider : INomenclaturePlanParametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public NomenclaturePlanParametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int CallCenterSubdivisionId  => parametersProvider.GetIntValue("call_center_subdivision_id");
    }
}