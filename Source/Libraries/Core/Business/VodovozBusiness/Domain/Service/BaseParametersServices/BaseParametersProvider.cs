using System;
using System.Globalization;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Core.DataService
{
	public class BaseParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		
		public BaseParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
	}
}
