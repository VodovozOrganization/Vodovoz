using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class NomenclaturePlanParametersProvider : INomenclaturePlanParametersProvider
    {
        private readonly IParametersProvider parametersProvider;

        public NomenclaturePlanParametersProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int CallCenterSubdivisionId  => parametersProvider.GetIntValue("call_center_subdivision_id");
    }
}
