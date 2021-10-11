using System;

namespace Vodovoz.Parameters
{
	public class GMapPararmetersProviders : IGMapParametersProviders
	{
		private readonly IParametersProvider parametersProvider;

		public GMapPararmetersProviders(IParametersProvider parametersProvider)
		{
			parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}
		
		public string SquidServer => parametersProvider.GetParameterValue("squidServer");
	}
}
