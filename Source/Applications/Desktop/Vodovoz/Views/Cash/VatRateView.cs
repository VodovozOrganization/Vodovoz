using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class VatRateView : EntityTabViewBase<VatRateViewModel, VatRate>
	{
		public VatRateView(VatRateViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryVatRate.Binding
				.AddBinding(ViewModel.Entity, e => e.VatRateValue, w => w.Text, new DecimalToStringConverter())
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumVat1cType.ItemsEnum = typeof(Vat1cType);
			enumVat1cType.Binding
				.AddBinding(ViewModel.Entity, p => p.Vat1cTypeValue, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			ycheckbuttonIsArchieve.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsNew, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			buttonSave.BindCommand(ViewModel.SaveCommand);
			
			buttonCancel.BindCommand(ViewModel.CancelCommand);

			buttonCopy.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			buttonCopy.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}
	}
}
