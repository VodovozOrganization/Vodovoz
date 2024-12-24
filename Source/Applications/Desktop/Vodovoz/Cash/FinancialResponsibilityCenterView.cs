using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Store.Reports;
using Selection = Gdk.Selection;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class FinancialResponsibilityCenterView : TabViewBase<FinancialResponsibilityCenterViewModel>
	{
		public FinancialResponsibilityCenterView(FinancialResponsibilityCenterViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			labelId.Text = ViewModel.Entity.Id.ToString();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			entryResponsibleEmployee.ViewModel = ViewModel.ResponsibleEmployeeViewModel;
			
			entryViceResponsibleEmployee.ViewModel = ViewModel.ViceResponsibleEmployeeViewModel;

			ychckbtnRequestApprovalDenied.Binding
				.AddBinding(ViewModel.Entity, e => e.RequestApprovalDenied, w => w.Active)
				.InitializeFromSource();

			ychckbtnArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			ResponsibleOfSubdivisions.CreateFluentColumnsConfig<EntityIdToNameNode>()
				.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.Finish();

			ResponsibleOfSubdivisions.RowActivated += OnResponsibleSubdivisionsRowActivated;

			ResponsibleOfSubdivisions.ItemsDataSource = ViewModel.ResponsibleOfSubdivisions;

			ResponsibleOfSubdivisions.Binding
				.AddBinding(ViewModel, vm => vm.SelectedSubdivisionNodeObject, w => w.SelectedRow)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);

			btnCopyEntityId.Sensitive = ViewModel.Entity.Id > 0;
			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		private void OnResponsibleSubdivisionsRowActivated(object o, RowActivatedArgs args)
		{
			ViewModel.OpenSubdivisionCommand.Execute();
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}
	}
}
