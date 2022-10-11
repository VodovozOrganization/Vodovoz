using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class DeliveryScheduleParametersProvider : IDeliveryScheduleParametersProvider
    {
        private readonly IParametersProvider parametersProvider;

        public DeliveryScheduleParametersProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
        
        public int ClosingDocumentDeliveryScheduleId => parametersProvider.GetIntValue("closing_document_delivery_schedule_id");
    }
}