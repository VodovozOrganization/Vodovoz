using System;

namespace Vodovoz.Parameters
{
    public class ReportDefaultsProvider : IReportDefaultsProvider
    {
        private readonly ParametersProvider parametersProvider;

        public ReportDefaultsProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
        public int GetDefaultOrderChangesOrganizationId => parametersProvider.GetIntValue("order_changes_default_organization_id");
    }
}
