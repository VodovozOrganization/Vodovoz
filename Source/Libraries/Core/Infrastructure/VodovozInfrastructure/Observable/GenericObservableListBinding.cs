using System;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;

namespace VodovozInfrastructure.Observable
{
	public class GenericObservableListBinding<TSourceElement> : IReadyObservableListBinding
	{
		private IListEvents _source;

		private Action<TSourceElement, int> _insertAction;
		private Action<int> _removeAction;
		private Action<TSourceElement, int> _setAction;

		internal GenericObservableListBinding<TSourceElement> Bind(GenericObservableList<TSourceElement> sourceElements)
		{
			_source = sourceElements;
			return this;
		}

		public IReadyObservableListBinding To<TDestElement>(IList<TDestElement> destElements, Func<TSourceElement, TDestElement> destElementFactory)
		{
			if(destElements is null)
			{
				throw new ArgumentNullException(nameof(destElements));
			}

			if(destElementFactory is null)
			{
				throw new ArgumentNullException(nameof(destElementFactory));
			}

			foreach(var sourceElement in (IEnumerable<TSourceElement>)_source)
			{
				var destElement = destElementFactory(sourceElement);
				destElements.Add(destElement);
			}

			_insertAction = (sourceElement, index) => destElements.Insert(index, destElementFactory(sourceElement));
			_setAction = (sourceElement, index) => destElements[index] = destElementFactory(sourceElement);
			_removeAction = (index) => destElements.RemoveAt(index);

			_source.ElementAdded += ObservableVersions_ElementAdded;
			_source.ElementRemoved += ObservableVersions_ElementRemoved;
			_source.ElementChanged += ObservableVersions_ElementChanged;

			return this;
		}

		void IReadyObservableListBinding.Clear()
		{
			_source.ElementAdded -= ObservableVersions_ElementAdded;
			_source.ElementRemoved -= ObservableVersions_ElementRemoved;
			_source.ElementChanged -= ObservableVersions_ElementChanged;
			_source = null;
			_insertAction = null;
			_removeAction = null;
			_setAction = null;
		}

		private void ObservableVersions_ElementAdded(object aList, int[] aIdx)
		{
			var eventSource = aList as GenericObservableList<TSourceElement>;
			if(eventSource != _source)
			{
				eventSource.ElementAdded -= ObservableVersions_ElementAdded;
			}

			foreach(var index in aIdx)
			{
				_insertAction.Invoke(eventSource[index], index);
			}
		}

		private void ObservableVersions_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			var eventSource = aList as GenericObservableList<TSourceElement>;
			if(eventSource != _source)
			{
				eventSource.ElementRemoved -= ObservableVersions_ElementRemoved;
			}

			foreach(var index in aIdx)
			{
				_removeAction.Invoke(index);
			}
		}

		private void ObservableVersions_ElementChanged(object aList, int[] aIdx)
		{
			var eventSource = aList as GenericObservableList<TSourceElement>;
			if(eventSource != null && eventSource != _source)
			{
				eventSource.ElementChanged -= ObservableVersions_ElementChanged;
			}

			foreach(var index in aIdx)
			{
				_setAction.Invoke(eventSource[index], index);
			}
		}
	}
}
