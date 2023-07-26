using Gdk;
using QS.Navigation;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Cash.FinancialCategoriesGroups
{
	[ToolboxItem(true)]
	public partial class FinancialCategoriesGroupView : TabViewBase<FinancialCategoriesGroupViewModel>
	{
		public FinancialCategoriesGroupView(FinancialCategoriesGroupViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			labelIdValue.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(fcg => fcg.Id != 0 ? fcg.Id.ToString() : string.Empty, label => label.Text)
				.InitializeFromSource();

			entryParentGroup.ViewModel = ViewModel.ParentFinancialCategoriesGroupViewModel;

			yentryName.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(fcg => fcg.Title, entry => entry.Text)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(fcg => fcg.IsArchive, checkbox => checkbox.Active)
				.InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, CloseSource.Cancel); };

			btnCopyEntityId.Sensitive = ViewModel.Entity.Id > 0;
			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;

			chkIsHideFromPublicAccess.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(fcg => fcg.IsHiddenFromPublicAccess, checkbox => checkbox.Active)
				.InitializeFromSource();
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
