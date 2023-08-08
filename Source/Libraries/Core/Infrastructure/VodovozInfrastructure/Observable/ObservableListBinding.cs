using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace VodovozInfrastructure.Observable
{
	public class ObservableListBinding<TSourceElement> : IReadyObservableListBinding
	{
		private IObservableList<TSourceElement> _source;

		private Action<TSourceElement, int> _insertAction;
		private Action<int> _removeAction;
		private Action<TSourceElement, int> _setAction;
		private Action _resetAction;

		internal ObservableListBinding<TSourceElement> Bind(IObservableList<TSourceElement> sourceElements)
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
			_resetAction = () => {
				destElements.Clear();
				foreach(var sourceElement in _source.Reverse())
				{
					_insertAction.Invoke(sourceElement, 0);
				}
			};

			_source.CollectionChanged += Source_CollectionChanged;

			return this;
		}

		void IReadyObservableListBinding.Clear()
		{
			Clear();
		}

		private void Clear()
		{
			_source.CollectionChanged -= Source_CollectionChanged;
			_source = null;
			_insertAction = null;
			_removeAction = null;
			_setAction = null;
		}

		private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					AddElement(e.NewItems, e.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException("Перемещение элемента внутри коллекции не поддерживается");
				case NotifyCollectionChangedAction.Remove:
					RemoveElement(e.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Replace:
					ReplaceElement(e.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
			}
		}

		private void AddElement(IList addedElements, int index)
		{
			var elements = addedElements as IEnumerable<TSourceElement>;
			if(elements == null)
			{
				return;
			}

			foreach(var element in elements.Reverse())
			{
				_insertAction.Invoke(element, index);
			}
		}

		private void RemoveElement(int index)
		{
			_removeAction.Invoke(index);
		}

		private void ReplaceElement(int index)
		{
			_setAction.Invoke(_source[index], index);
		}

		private void ResetCollection()
		{

			_resetAction.Invoke();
		}
	}
}
