using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using NHibernate.Criterion;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Tdi;
using QSOrmProject;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GeographicGroupsToStringWidget : Gtk.Bin
	{
		private readonly int eastGeographicGroupId = new GeographicGroupParametersProvider(new ParametersProvider()).EastGeographicGroupId;
		public event EventHandler<EventArgs> ListContentChanged;
		public BindingControler<GeographicGroupsToStringWidget> Binding { get; private set; }

		public IUnitOfWork UoW { get; set; }

		public string Label {
			get => lblName.LabelProp;
			set => lblName.LabelProp = value;
		}

		GenericObservableList<GeoGroup> items;
		public GenericObservableList<GeoGroup> Items {
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
			var uowFactory = UnitOfWorkFactory.GetDefaultFactory;
			var commonServices = ServicesConfig.CommonServices;
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var warehouseJournalFactory = new WarehouseJournalFactory();
			var employeeService = new EmployeeService();
			var geoGroupVersionsModel = new GeoGroupVersionsModel(commonServices.UserService, employeeService);
			var journal = new GeoGroupJournalViewModel(uowFactory, commonServices, subdivisionJournalFactory, warehouseJournalFactory, geoGroupVersionsModel);
			journal.SelectionMode = JournalSelectionMode.Multiple;
			journal.DisableChangeEntityActions();
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
			{
				return;
			}

			mytab.TabParent.AddSlaveTab(mytab, journal);
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selected = e.SelectedNodes.Cast<GeoGroupJournalNode>();
			if(!selected.Any())
			{
				return;
			}
			foreach(var item in selected)
			{
				if(!Items.Any(x => x.Id == item.Id))
				{
					var group = UoW.GetById<GeoGroup>(item.Id);
					Items.Add(group);
				}
			}
			UpdateText();
		}

		void SelectedGeographicGroups_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			foreach(var item in e.Subjects) {
				if(item is GeoGroup group && !Items.Any(x => x.Id == group.Id))
					Items.Add(group);
			}
			UpdateText();
		}

		protected void OnBtnRemoveClicked(object sender, EventArgs e)
		{
			var uowFactory = UnitOfWorkFactory.GetDefaultFactory;
			var commonServices = ServicesConfig.CommonServices;
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var warehouseJournalFactory = new WarehouseJournalFactory();
			var employeeService = new EmployeeService();
			var geoGroupVersionsModel = new GeoGroupVersionsModel(commonServices.UserService, employeeService);
			var journal = new GeoGroupJournalViewModel(uowFactory, commonServices, subdivisionJournalFactory, warehouseJournalFactory, geoGroupVersionsModel);
			journal.SelectionMode = JournalSelectionMode.Multiple;
			journal.DisableChangeEntityActions();
			journal.OnEntitySelectedResult += Journal_OnRemoveEntitySelectedResult; ;

			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
			{
				return;
			}

			mytab.TabParent.AddSlaveTab(mytab, journal);
		}

		private void Journal_OnRemoveEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var selected = e.SelectedNodes.Cast<GeoGroupJournalNode>();
			if(!selected.Any())
			{
				return;
			}
			foreach(var item in selected)
			{
				var group = Items.FirstOrDefault(x => x.Id == item.Id);
				if(group != null)
				{
					Items.Remove(group);
				}
			}
			UpdateText();
		}
	}
}
