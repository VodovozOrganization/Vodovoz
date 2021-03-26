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
        
        public int СashierSofiiskayaId => parametersProvider.GetIntValue("cashier_sofiiskaya_id");
        public int СashierParnasId => parametersProvider.GetIntValue("cashier_parnas_id");
        public int WarehouseSofiiskayaId => parametersProvider.GetIntValue("warehouse_sofiiskaya_id");
        public int WarehouseParnasId => parametersProvider.GetIntValue("warehouse_parnas_id");
    }
}