using QS.Extensions.Observable.Collections.List;
using System.Data.Bindings.Collections.Generic;

namespace VodovozInfrastructure.Observable
{
	public static class ObservableListBinder
	{
		public static GenericObservableListBinding<TSourceElement> Bind<TSourceElement>(GenericObservableList<TSourceElement> sourceElements)
		{
			var binding = new GenericObservableListBinding<TSourceElement>();
			return binding.Bind(sourceElements);
		}

		public static ObservableListBinding<TSourceElement> Bind<TSourceElement>(IObservableList<TSourceElement> sourceElements)
		{
			var binding = new ObservableListBinding<TSourceElement>();
			return binding.Bind(sourceElements);
		}
	}
}
