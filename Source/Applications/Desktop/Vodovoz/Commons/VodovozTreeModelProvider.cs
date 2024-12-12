using Gamma.Binding;
using Gamma.TreeModels;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections;

namespace Vodovoz {
	public class VodovozTreeModelProvider : ITreeModelProvider {
		public IyTreeModel GetTreeModel(object datasource, bool reordable = false) {
			if(!(datasource is IList list)) {
				throw new NotSupportedException($"Type '{datasource.GetType()}' is not supported. Data source must implement IList.");
			}

			if(datasource is IObservableList observableList) {
				if(reordable) 
				{
					return new ObservableListReorderableTreeModel(observableList);
				}
				else 
				{
					return new ObservableListTreeModel(observableList);
				}
			}
			
			if(datasource is System.Data.Bindings.IObservableList oldObservableList)
			{
				if(reordable)
				{
					return new DeprecatedObservableListReorderableTreeModel(oldObservableList);
				}
				else
				{
					return new DeprecatedObservableListTreeModel(oldObservableList);
				}
			}

			return new ListTreeModel(list);
		}
	}
}
