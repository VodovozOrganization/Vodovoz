using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public abstract class SelectableParameter : PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set {
				if(SetField(ref selected, value)) {
					if(selected) {
						SelectAllChilds();
					} else {
						UnselectAllChilds();
					}
					RaiseAnySelectedChanged(this);
					Parent?.ActualizeSelected();
				}
			}
		}

		public event EventHandler<SelectableParameterSelectionChangedEventArgs> AnySelectedChanged;

		public abstract string Title { get; }

		public abstract Func<object> ValueFunc { get; }

		public object Value => ValueFunc();

		public virtual SelectableParameter Parent { get; set; }

		private GenericObservableList<SelectableParameter> sourceChildren = new GenericObservableList<SelectableParameter>();

		private GenericObservableList<SelectableParameter> children = new GenericObservableList<SelectableParameter>();
		public virtual GenericObservableList<SelectableParameter> Children {
			get => children;
			private set => SetField(ref children, value);
		}

		protected SelectableParameter()
		{
		}

		private void RaiseAnySelectedChanged(SelectableParameter selectableParameter)
		{
			AnySelectedChanged?.Invoke(this, new SelectableParameterSelectionChangedEventArgs(Value, Title, Selected));
		}

		private void ActualizeSelected()
		{
			if(Children == null || !Children.Any()) {
				return;
			}
			if(Children.All(x => x.Selected)) {
				SelectOnlyThis();
			} else {
				UnselectOnlyThis();
			}
		}

		private void SelectOnlyThis()
		{
			selected = true;
			OnPropertyChanged(nameof(Selected));
			Parent?.ActualizeSelected();
		}

		private void UnselectOnlyThis()
		{
			selected = false;
			OnPropertyChanged(nameof(Selected));
			Parent?.ActualizeSelected();
		}

		private void UnselectAllChilds()
		{
			foreach(SelectableParameter child in Children) {
				child.Selected = false;
			}
		}

		private void SelectAllChilds()
		{
			foreach(SelectableParameter child in Children) {
				child.Selected = true;
			}
		}

		private string searchValue;

		public void FilterChilds(string searchValue)
		{
			this.searchValue = searchValue;
			foreach(SelectableParameter child in Children) {
				child.FilterChilds(searchValue);
			}
			UpdateChilds();
		}

		private void UpdateChilds()
		{
			if(string.IsNullOrWhiteSpace(searchValue)) {
				Children = sourceChildren;
			} else {
				//выбираем все дочерние элементы
				var filtered = sourceChildren.Where(x =>
					//коротые удовлетворяют поисковому критерию
					x.Title.ToLower().Contains(searchValue.ToLower())
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
			foreach(SelectableParameter oldChild in sourceChildren) {
				oldChild.AnySelectedChanged -= OnChildAnySelectedChanged;
			}

			foreach(SelectableParameter child in childs) {
				child.AnySelectedChanged += OnChildAnySelectedChanged;
			}

			sourceChildren = new GenericObservableList<SelectableParameter>(childs.ToList());
			UpdateChilds();
		}

		void OnChildAnySelectedChanged(object sender, EventArgs e)
		{
			RaiseAnySelectedChanged(this);
		}

		public IEnumerable<SelectableParameter> GetAllSelected()
		{
			List<SelectableParameter> result = new List<SelectableParameter>();
			if(Selected) {
				result.Add(this);
			}
			result.AddRange(Children.SelectMany(x => x.GetAllSelected()));
			return result;
		}
	}
}
