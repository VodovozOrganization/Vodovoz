using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public abstract class SelectableParameter : PropertyChangedBase
	{
		private bool _selected;
		private string _searchValue;

		protected SelectableParameter()
		{
		}

		public virtual bool Selected
		{
			get => _selected;
			set
			{
				if(SetField(ref _selected, value))
				{
					if(_selected)
					{
						SelectAllChilds();
					}
					else
					{
						UnselectAllChilds();
					}

					Parent?.ActualizeSelected();
					RaiseAnySelectedChanged(this);
				}
			}
		}

		public event EventHandler<SelectableParameterSelectionChangedEventArgs> AnySelectedChanged;

		public abstract string Title { get; }

		public abstract Func<object> ValueFunc { get; }

		public object Value => ValueFunc();

		public virtual SelectableParameter Parent { get; set; }


		private GenericObservableList<SelectableParameter> _sourceChildren = new GenericObservableList<SelectableParameter>();


		private GenericObservableList<SelectableParameter> _children = new GenericObservableList<SelectableParameter>();

		public virtual GenericObservableList<SelectableParameter> Children
		{
			get => _children;
			private set => SetField(ref _children, value);
		}

		private void RaiseAnySelectedChanged(SelectableParameter selectableParameter)
		{
			AnySelectedChanged?.Invoke(this, new SelectableParameterSelectionChangedEventArgs(Value, Title, Selected));
		}

		private void ActualizeSelected()
		{
			if(Children == null || !Children.Any())
			{
				return;
			}

			if(Children.All(x => x.Selected))
			{
				SelectOnlyThis();
			}
			else
			{
				UnselectOnlyThis();
			}
		}

		private void SelectOnlyThis()
		{
			_selected = true;
			OnPropertyChanged(nameof(Selected));
			Parent?.ActualizeSelected();
		}

		private void UnselectOnlyThis()
		{
			_selected = false;
			OnPropertyChanged(nameof(Selected));
			Parent?.ActualizeSelected();
		}

		private void UnselectAllChilds()
		{
			foreach(SelectableParameter child in Children)
			{
				child.Selected = false;
			}
		}

		private void SelectAllChilds()
		{
			foreach(SelectableParameter child in Children)
			{
				child.Selected = true;
			}
		}

		public void FilterChilds(string searchValue)
		{
			_searchValue = searchValue;

			if(!_sourceChildren.Any())
			{
				return;
			}

			UpdateChilds();

			foreach(SelectableParameter child in Children)
			{
				child.FilterChilds(searchValue);
			}
		}

		private void UpdateChilds()
		{
			if(string.IsNullOrWhiteSpace(_searchValue))
			{
				Children = _sourceChildren;
			}
			else
			{
				//выбираем все дочерние элементы
				//коротые удовлетворяют поисковому критерию
				var filtered = _sourceChildren
					.Where(x => x.Title.ToLower().Contains(_searchValue.ToLower())
						//или имеют любые дочерние элементы
						//(Это сделано для того чтобы, не отфильтровать родителя 
						//который не удовлетворяет поисковому критерию, но имеет 
						//гдето глубже в структуре дочерние элементы которые удовлетворяют)
						|| x.Children.Any());

				Children = new GenericObservableList<SelectableParameter>(filtered.ToList());
			}
		}

		public void SetChilds(IList<SelectableParameter> childs)
		{
			foreach(SelectableParameter oldChild in _sourceChildren)
			{
				oldChild.AnySelectedChanged -= OnChildAnySelectedChanged;
			}

			foreach(SelectableParameter child in childs)
			{
				child.AnySelectedChanged += OnChildAnySelectedChanged;
			}

			_sourceChildren = new GenericObservableList<SelectableParameter>(childs.ToList());

			UpdateChilds();
		}

		private void OnChildAnySelectedChanged(object sender, EventArgs e)
		{
			RaiseAnySelectedChanged(this);
		}

		public IEnumerable<SelectableParameter> GetAllSelected()
		{
			var result = new List<SelectableParameter>();

			if(Selected)
			{
				result.Add(this);
			}

			result.AddRange(Children.SelectMany(x => x.GetAllSelected()));

			return result;
		}
	}
}
