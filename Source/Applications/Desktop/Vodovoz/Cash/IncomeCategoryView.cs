﻿using Gdk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Cash
{
	[ToolboxItem(true)]
	public partial class IncomeCategoryView : TabViewBase<IncomeCategoryViewModel>
	{
		public IncomeCategoryView(IncomeCategoryViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			labelIdValue.Binding
				.AddSource(ViewModel.Entity)
				.AddFuncBinding(ic => ic.Id != 0 ? ic.Id.ToString() : string.Empty, label => label.Text)
				.InitializeFromSource();

			yentryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, (widget) => widget.Text)
				.InitializeFromSource();

			yentryNumbering.Binding
				.AddBinding(ViewModel.Entity, e => e.Numbering, (widget) => widget.Text)
				.InitializeFromSource();

			entryParentGroup.ViewModel = ViewModel.ParentFinancialCategoriesGroupViewModel;

			#region ParentEntityviewmodelentry
			ParentEntityviewmodelentry.SetEntityAutocompleteSelectorFactory(ViewModel.IncomeCategoryAutocompleteSelectorFactory);
			ParentEntityviewmodelentry.Binding.AddBinding(ViewModel.Entity, s => s.Parent, w => w.Subject).InitializeFromSource();
			ParentEntityviewmodelentry.CanEditReference = true;
			#endregion

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			ycheckArchived.Binding.AddBinding(ViewModel, e => e.IsArchive, w => w.Active).InitializeFromSource();

			yenumTypeDocument.ItemsEnum = typeof(IncomeInvoiceDocumentType);
			yenumTypeDocument.Binding.AddBinding(ViewModel.Entity, e => e.IncomeDocumentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };

			tableMain.Sensitive = false;

			btnCopyEntityId.Sensitive = ViewModel.Entity.Id > 0;
			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
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
