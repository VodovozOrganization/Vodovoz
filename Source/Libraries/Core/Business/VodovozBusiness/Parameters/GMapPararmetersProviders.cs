using System;

namespace Vodovoz.Parameters
{
	public class GMapPararmetersProviders : IGMapParametersProviders
	{
		private readonly IParametersProvider _parametersProvider;

		public GMapPararmetersProviders(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		
		public string SquidServer => _parametersProvider.GetParameterValue("squidServer");
	}
}
