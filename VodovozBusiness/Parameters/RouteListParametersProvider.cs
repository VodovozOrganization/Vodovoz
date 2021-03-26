using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class RouteListParametersProvider : IRouteListParametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public RouteListParametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
        
        public int CashSubdivisionSofiiskayaId => parametersProvider.GetIntValue("cashsubdivision_sofiiskaya_id");
        public int CashSubdivisionParnasId => parametersProvider.GetIntValue("cashsubdivision_parnas_id");
        public int WarehouseSofiiskayaId => parametersProvider.GetIntValue("warehouse_sofiiskaya_id");
        public int WarehouseParnasId => parametersProvider.GetIntValue("warehouse_parnas_id");
    }
}