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
					RaiseAnySelectedChanged();
					if(selected) {
						SelectAllChilds();
					} else {
						UnselectAllChilds();
					}
					RaiseAnySelectedChanged();
					Parent?.ActualizeSelected();
				}
			}
		}

		public event EventHandler AnySelectedChanged;

		public abstract string Title { get; }

		public abstract Func<object> ValueFunc { get; }

		public object Value => ValueFunc();

		public virtual SelectableParameter Parent { get; set; }

		public virtual GenericObservableList<SelectableParameter> Children { get; private set; } = new GenericObservableList<SelectableParameter>();

		protected SelectableParameter()
		{
		}

		private void RaiseAnySelectedChanged()
		{
			AnySelectedChanged?.Invoke(this, EventArgs.Empty);
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

		public void SetChilds(IList<SelectableParameter> childs)
		{
			foreach(SelectableParameter oldChild in Children) {
				oldChild.AnySelectedChanged -= OnChildAnySelectedChanged;
			}

			foreach(SelectableParameter child in childs) {
				child.AnySelectedChanged += OnChildAnySelectedChanged;
			}

			Children = new GenericObservableList<SelectableParameter>(childs.ToList());
		}

		void OnChildAnySelectedChanged(object sender, EventArgs e)
		{
			RaiseAnySelectedChanged();
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
