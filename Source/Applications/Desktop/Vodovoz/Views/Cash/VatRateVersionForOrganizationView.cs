using System;
using Gamma.ColumnConfig;
using QS.Utilities;
using QS.Views.GtkUI;
using Gtk;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Cash;
using Color = Gdk.Color;

namespace Vodovoz.Views.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VatRateVersionForOrganizationView : WidgetViewBase<VatRateOrganizationVersionViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;
		
		public VatRateVersionForOrganizationView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			yhbox4.Sensitive = ViewModel.IsButtonsAvailable;
			
			yvbox7.Binding.AddBinding(ViewModel, vm => vm.IsEditVisible, w => w.Visible).InitializeFromSource();

			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.AddFuncBinding(vm => !vm.IsNewOrganization, w => w.Sensitive)
				.InitializeFromSource();

			ytreeVersions.ColumnsConfig = FluentColumnsConfig<VatRateVersion>.Create()
				.AddColumn("Код")
					.MinWidth(50)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString())
					.XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Ставка НДС, %")
					.AddTextRenderer(x => x.VatRate == null ?  "Ставка не выбрана" : x.VatRate.VatRateValue.ToString())
					.XAlign(0.5f)
				.AddColumn("Начало действия")
					.AddTextRenderer(x => x.StartDate.ToString("g"))
					.XAlign(0.5f)
				.AddColumn("Окончание действия")
					.AddTextRenderer(x => x.EndDate != null ? x.EndDate.Value.ToString("g") : "")
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeVersions.ItemsDataSource = ViewModel.Entity.ObservableVatRateVersions;
			ytreeVersions.Binding.AddBinding(ViewModel, vm => vm.SelectedVatRateVersion, w => w.SelectedRow).InitializeFromSource();
			ytreeVersions.RowActivated += OnYtreeVersionsOnRowActivated;

			entryVatRate.ViewModel = ViewModel.VatRateEntryViewModel;
			
			buttonNewVersion.Binding
				.AddBinding(ViewModel, vm => vm.CanAddNewVersion, w => w.Sensitive)
				.InitializeFromSource();

			buttonNewVersion.Clicked += OnButtonNewVersionOnClicked;

			buttonChangeVersionDate.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeVersionDate, w => w.Sensitive)
				.InitializeFromSource();
			buttonChangeVersionDate.BindCommand(ViewModel.ChangeVersionStartDateCommand); 

			buttonCancel.BindCommand(ViewModel.CancelEditingVersionCommand);

			buttonEditVersion.BindCommand(ViewModel.EditVersionCommand);
			buttonEditVersion.Binding
				.AddBinding(ViewModel, vm => vm.IsEditAvailable, w => w.Sensitive)
				.InitializeFromSource();
			
			buttonSave.BindCommand(ViewModel.SaveEditingVersionCommand);
		}

		private void OnButtonNewVersionOnClicked(object sender, EventArgs args)
		{
			ViewModel.AddNewVersionCommand.Execute();
			GtkHelper.WaitRedraw();
			ytreeVersions.Vadjustment.Value = 0;
		}

		private void OnYtreeVersionsOnRowActivated(object sender, RowActivatedArgs args)
		{
			ViewModel.EditVersionCommand.Execute();
		}

		public override void Dispose()
		{
			ytreeVersions.RowActivated -= OnYtreeVersionsOnRowActivated;
			buttonNewVersion.Clicked -= OnButtonNewVersionOnClicked;
		}
	}
}
