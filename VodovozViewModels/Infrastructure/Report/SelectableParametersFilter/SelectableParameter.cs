using System;
using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.Linq;
namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameter : PropertyChangedBase
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
					Parent.ActualizeSelected();
				}
			}
		}

		private string title;
		public virtual string Title => title;

		public int Id => Entity.Id;

		public IDomainObject Entity { get; }

		public virtual SelectableParameter Parent { get; set; }

		public virtual IList<SelectableParameter> Childs { get; set; } = new List<SelectableParameter>();

		public SelectableParameter(IDomainObject entity)
		{
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
			title = Entity.GetType().GetSubjectName();
		}

		private void ActualizeSelected()
		{
			if(Childs == null || !Childs.Any()) {
				return;
			}
			if(Childs.All(x => x.Selected)) {
				SelectOnlyThis();
			} else {
				UnselectOnlyThis();
			}
		}

		private void SelectOnlyThis()
		{
			selected = true;
			OnPropertyChanged(nameof(Selected));
			Parent.ActualizeSelected();
		}

		private void UnselectOnlyThis()
		{
			selected = false;
			OnPropertyChanged(nameof(Selected));
			Parent.ActualizeSelected();
		}

		private void UnselectAllChilds()
		{
			foreach(var child in Childs) {
				child.Selected = false;
			}
		}

		private void SelectAllChilds()
		{
			foreach(var child in Childs) {
				child.Selected = false;
			}
		}
	}

	public class SelectableParameter<TEntity> : SelectableParameter
		where TEntity : class, IDomainObject
	{
		public TEntity GenericEntity { get; }

		public SelectableParameter(TEntity entity) : base(entity)
		{
			GenericEntity = entity ?? throw new ArgumentNullException(nameof(entity));
		}
	}
}
