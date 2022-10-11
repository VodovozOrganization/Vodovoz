using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class TabsParametersProvider : ITabsParametersProvider
    {
        private readonly IParametersProvider parametersProvider;
        
        public TabsParametersProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
        
        public char TabsPrefix => parametersProvider.GetCharValue("tab_prefix");
    }
}