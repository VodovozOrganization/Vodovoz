using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using NHibernate.Criterion;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Tdi;
using QSOrmProject;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeographicGroupsToStringWidget : Gtk.Bin
	{
		public event EventHandler<EventArgs> ListContentChanged;
		public BindingControler<GeographicGroupsToStringWidget> Binding { get; private set; }

		public IUnitOfWork UoW { get; set; }

		public string Label {
			get => lblName.LabelProp;
			set => lblName.LabelProp = value;
		}

		GenericObservableList<GeographicGroup> items;
		public GenericObservableList<GeographicGroup> Items {
			get => items;
			set {
				items = value;
				Binding.FireChange(x => x.Items);
				Items.ElementAdded += (sender, e) => UpdateText();
				Items.ElementRemoved += (aList, aIdx, aObject) => UpdateText();
				UpdateText();
			}
		}

		public GeographicGroupsToStringWidget()
		{
			this.Build();

			Binding = new BindingControler<GeographicGroupsToStringWidget>(
				this,
				new Expression<Func<GeographicGroupsToStringWidget, object>>[] {
					w => w.Items
				}
			);
			UpdateText();
		}

		void UpdateText()
		{
			string text = string.Format(
				"<b>{0}</b>",
				Items != null && Items.Any()
					? string.Join(", ", Items.Select(g => g.Name))
					: "Нет"
			);
			lblElements.Markup = text;
			ListContentChanged?.Invoke(this, new EventArgs());
		}

		protected void OnBtnChangeListClicked(object sender, EventArgs e)
		{
			var selectedGeographicGroups = new OrmReference(typeof(GeographicGroup), UoW) {
				Mode = OrmReferenceMode.MultiSelect,
				ButtonMode = ReferenceButtonMode.None
			};
			selectedGeographicGroups.ObjectSelected += SelectedGeographicGroups_ObjectSelected;

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
				return;

			mytab.TabParent.AddSlaveTab(mytab, selectedGeographicGroups);
		}

		void SelectedGeographicGroups_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var item in e.Subjects) {
				if(item is GeographicGroup group && !Items.Any(x => x.Id == group.Id))
					Items.Add(group);
			}
			UpdateText();
		}

		protected void OnBtnRemoveClicked(object sender, EventArgs e)
		{
			var ids = Items.Select(x => x.Id).ToArray();
			var selectGeographicGroups = new OrmReference(QueryOver.Of<GeographicGroup>().Where(x => x.Id.IsIn(ids))) {
				Mode = OrmReferenceMode.MultiSelect,
				ButtonMode = ReferenceButtonMode.None
			};
			selectGeographicGroups.ObjectSelected += RemovingGeographicGroups_ObjectSelected;

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
				return;

			mytab.TabParent.AddSlaveTab(mytab, selectGeographicGroups);
		}

		void RemovingGeographicGroups_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var item in e.Subjects) {
				if(item is GeographicGroup removingGroup && Items.Any(x => x.Id == removingGroup.Id))
					Items.Remove(Items.FirstOrDefault(x => x.Id == removingGroup.Id));
			}
			UpdateText();
		}
	}
}
