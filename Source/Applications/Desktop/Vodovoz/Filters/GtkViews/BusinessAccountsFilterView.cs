using System;
using System.ComponentModel;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class BusinessAccountsFilterView : FilterViewBase<BusinessAccountsFilterViewModel>
	{
		public BusinessAccountsFilterView(BusinessAccountsFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ConfigureEntries();
			ConfigureEntityEntries();
			ConfigureComboBox();

			chkShowArchived.Binding
				.AddBinding(ViewModel, vm => vm.ShowArchived, w => w.Active)
				.InitializeFromSource();
		}


		private void ConfigureEntries()
		{
			entryName.KeyReleaseEvent += OnKeyReleased;
			entryName.Binding
				.AddBinding(ViewModel, vm => vm.Name, w => w.Text)
				.InitializeFromSource();

			entryNumber.KeyReleaseEvent += OnKeyReleased;
			entryNumber.Binding
				.AddBinding(ViewModel, vm => vm.Number, w => w.Text)
				.InitializeFromSource();

			entryBank.KeyReleaseEvent += OnKeyReleased;
			entryBank.Binding
				.AddBinding(ViewModel, vm => vm.Bank, w => w.Text)
				.InitializeFromSource();
		}

		private void ConfigureEntityEntries()
		{
			entityEntryFunds.ViewModel = ViewModel.FundsViewModel;
			entityEntryBusinessActivity.ViewModel = ViewModel.BusinessActivityViewModel;
		}

		private void ConfigureComboBox()
		{
			enumCmbAccountFillType.ShowSpecialStateAll = true;
			enumCmbAccountFillType.ItemsEnum = typeof(AccountFillType);
			enumCmbAccountFillType.Binding
				.AddBinding(ViewModel, vm => vm.AccountFillType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}


		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Destroy()
		{
			entryName.KeyReleaseEvent -= OnKeyReleased;
			entryNumber.KeyReleaseEvent -= OnKeyReleased;
			entryBank.KeyReleaseEvent -= OnKeyReleased;
			base.Destroy();
		}
	}
}
