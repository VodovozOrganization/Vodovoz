using Gdk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Cash.FinancialCategoriesGroups
{
	[ToolboxItem(true)]
	public partial class FinancialIncomeCategoryView : TabViewBase<FinancialIncomeCategoryViewModel>
	{
		public FinancialIncomeCategoryView(FinancialIncomeCategoryViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			labelIdValue.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(ec => ec.Id != 0 ? ec.Id.ToString() : string.Empty, label => label.Text)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Title, w => w.Text)
				.InitializeFromSource();

			yentryNumbering.Binding
				.AddBinding(ViewModel.Entity, e => e.Numbering, w => w.Text)
				.InitializeFromSource();

			entryParentGroup.ViewModel = ViewModel.ParentFinancialCategoriesGroupViewModel;

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			ycheckArchived.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(TargetDocument);
			yenumTypeDocument.Binding.AddBinding(ViewModel.Entity, e => e.TargetDocument, w => w.SelectedItem).InitializeFromSource();

			ycheckExcludeFromCashFlowDds.Binding.AddBinding(ViewModel.Entity, e => e.ExcludeFromCashFlowDds, w => w.Active).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			btnCopyEntityId1.Sensitive = ViewModel.Entity.Id > 0;
			btnCopyEntityId1.Clicked += OnBtnCopyEntityIdClicked;
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
