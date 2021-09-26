using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class ProductionWarehouseMovementReportProvider : IProductionWarehouseMovementReportProvider
	{
        private readonly IParametersProvider parametersProvider;

        public ProductionWarehouseMovementReportProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int DefaultProductionWarehouseId => parametersProvider.GetIntValue("production_warehouse_movement_report_warehouse_id");
	}
}
