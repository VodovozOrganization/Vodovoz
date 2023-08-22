using Gamma.Binding;
using Gamma.TreeModels;
using System;
using System.Collections;
using NewIObservableList = QS.Extensions.Observable.Collections.List.IObservableList;
using NewObservableListReorderableTreeModel = Gamma.Binding.ObservableListReorderableTreeModel;
using NewObservableListTreeModel = Gamma.Binding.ObservableListTreeModel;
using OldIObservableList = System.Data.Bindings.IObservableList;
using OldObservableListReorderableTreeModel = Vodovoz.TempAdapters.TreeModels.ObservableListReorderableTreeModel;
using OldObservableListTreeModel = Vodovoz.TempAdapters.TreeModels.ObservableListTreeModel;

namespace Vodovoz.TempAdapters.TreeModels
{
	public class CombinedTreeModelProvider : ITreeModelProvider
	{
		public IyTreeModel GetTreeModel(object datasource, bool reordable = false)
		{
			if(!(datasource is IList list))
			{
				throw new NotSupportedException($"Type '{datasource.GetType()}' is not supported. Data source must implement IList.");
			}

			if(datasource is NewIObservableList observableList)
			{
				if(reordable)
				{
					return new NewObservableListReorderableTreeModel(observableList);
				}
				else
				{
					return new NewObservableListTreeModel(observableList);
				}
			}

			if(datasource is OldIObservableList oldObservableList)
			{
				if(reordable)
				{
					return new OldObservableListReorderableTreeModel(oldObservableList);
				}
				else
				{
					return new OldObservableListTreeModel(oldObservableList);
				}
			}

			return new ListTreeModel(list);
		}
	}


}
