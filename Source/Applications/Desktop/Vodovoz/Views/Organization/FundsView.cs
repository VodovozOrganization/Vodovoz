﻿using System;
using QS.Views.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations;

namespace Vodovoz.Views.Organization
{
	public partial class FundsView : DialogViewBase<FundsViewModel>
	{
		public FundsView(FundsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, e => e.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblId.Binding
				.AddBinding(ViewModel, e => e.IdString, w => w.LabelProp)
				.AddBinding(ViewModel, e => e.CanShowId, w => w.Visible)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			enumСmbDefaultAccountFillType.ItemsEnum = typeof(AccountFillType);
			enumСmbDefaultAccountFillType.Binding
				.AddBinding(ViewModel.Entity, e => e.DefaultAccountFillType, w => w.SelectedItem)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
		}
	}
}
