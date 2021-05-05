using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class TabsParametersProvider : ITabsParametersProvider
    {
        private readonly ParametersProvider parametersProvider;
        
        public TabsParametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
        
        public char TabsPrefix => parametersProvider.GetCharValue("tab_prefix");
    }
}