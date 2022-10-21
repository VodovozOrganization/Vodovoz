using System.Data.Bindings.Collections.Generic;

namespace VodovozInfrastructure.Observable
{
	public static class ObservableListBinder
	{
		public static ObservableListBinding<TSourceElement> Bind<TSourceElement>(GenericObservableList<TSourceElement> sourceElements)
		{
			var binding = new ObservableListBinding<TSourceElement>();
			return binding.Bind(sourceElements);
		}
	}
}
