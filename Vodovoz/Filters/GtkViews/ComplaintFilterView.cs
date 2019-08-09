using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using Gtk;
using QS.Views.GtkUI;
using QS.Widgets.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Representations;
using Vodovoz.ViewModel;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintFilterView : FilterViewBase<ComplaintFilterViewModel>
	{
		public ComplaintFilterView(ComplaintFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			ybuttonAddSubdivision.Clicked += (sender, e) => AddSubdivision();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryreferencevmEmployee.RepresentationModel = new EmployeesVM(new EmployeeFilterViewModel(ServicesConfig.CommonServices));
			yentryreferencevmEmployee.Binding.AddBinding(ViewModel, x => x.Employee, v => v.Subject).InitializeFromSource();

			yenumcomboboxType.ItemsEnum = typeof(ComplaintType);
			yenumcomboboxType.Binding.AddBinding(ViewModel, x => x.ComplaintType, v => v.SelectedItemOrNull).InitializeFromSource();

			yenumcomboboxStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboboxStatus.Binding.AddBinding(ViewModel, x => x.ComplaintStatus, v => v.SelectedItemOrNull).InitializeFromSource();

			if(ViewModel.Subdivisions == null)
				ViewModel.Subdivisions = new GenericObservableList<ComplaintFilterNode>() { new ComplaintFilterNode()};

			ViewModel.Subdivisions.ElementAdded += (aList, aIdx) => Redraw();
			ViewModel.Subdivisions.ElementRemoved += (aList, aIdx, aObject) => Redraw();
			Redraw();
		}

		private void AddSubdivision()
		{
			if(ViewModel.Subdivisions.Last().IsEmpty)
				return;
			var node = new ComplaintFilterNode();
			ViewModel.Subdivisions.Add(node);
		}

		private void Redraw()
		{
			foreach(var item in vboxSubdivisions.Children)
				item.Destroy();
			foreach(ComplaintFilterNode item in ViewModel.Subdivisions)
				AddSubdivision(item);
		}

		private void AddSubdivision(ComplaintFilterNode filterNode)
		{
			HBox hBox = new HBox();

			yEntryReference subdivisionReference = new yEntryReference();
			subdivisionReference.SubjectType = typeof(Subdivision);
			subdivisionReference.Binding.AddBinding(filterNode, x => x.Subdivision, w => w.Subject).InitializeFromSource();
			hBox.Add(subdivisionReference);

			DateRangePicker datePicker = new DateRangePicker();
			datePicker.Binding.AddBinding(filterNode, x => x.StartDate, w => w.StartDate).InitializeFromSource();
			datePicker.Binding.AddBinding(filterNode, x => x.EndDate, w => w.EndDate).InitializeFromSource();
			hBox.Add(datePicker);

			yButton deleteButton = new yButton();
			deleteButton.Data.Add("node", filterNode);
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += (sender, e) => ViewModel.Subdivisions.Remove((sender as yButton).Data["node"] as ComplaintFilterNode);
			hBox.Add(deleteButton);
			hBox.SetChildPacking(deleteButton, false, false, 0, PackType.Start);

			hBox.ShowAll();
			vboxSubdivisions.Add(hBox);
		}
	}
}
