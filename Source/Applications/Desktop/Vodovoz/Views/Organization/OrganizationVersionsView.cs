using Gamma.ColumnConfig;
using Pango;
using QS.Utilities;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Organizations;
using Color = Gdk.Color;

namespace Vodovoz.Views.Organization
{
	[ToolboxItem(true)]
	public partial class OrganizationVersionsView : WidgetViewBase<OrganizationVersionsViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public OrganizationVersionsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			hbox5.Sensitive = ViewModel.IsButtonsAvailable;

			yvboxVersionEdit.Binding.AddBinding(ViewModel, vm => vm.IsEditVisible, w => w.Visible).InitializeFromSource();

			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.AddFuncBinding(vm => !vm.IsNewOrganization, w => w.Sensitive)
				.InitializeFromSource();

			ytreeVersions.ColumnsConfig = FluentColumnsConfig<OrganizationVersion>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "").XAlign(0.5f)
				.AddColumn("Руководитель").AddTextRenderer(x => x.LeaderShortName).XAlign(0.5f)
				.AddColumn("Бухгалтер").AddTextRenderer(x => x.AccountantShortName).XAlign(0.5f)
				.AddColumn("Адрес").AddTextRenderer(x => x.Address).WrapMode(WrapMode.WordChar).WrapWidth(200).XAlign(0.5f)
				.AddColumn("Юр. адрес").AddTextRenderer(x => x.JurAddress).WrapMode(WrapMode.WordChar).WrapWidth(200).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeVersions.ItemsDataSource = ViewModel.Entity.OrganizationVersions;
			ytreeVersions.Binding.AddBinding(ViewModel, vm => vm.SelectedOrganizationVersion, w => w.SelectedRow).InitializeFromSource();
			ytreeVersions.RowActivated += (sender, args) => ViewModel.EditVersionCommand.Execute();

			evmeLeader.SetEntityAutocompleteSelectorFactory(ViewModel.LeaderSelectorFactory);
			evmeLeader.Binding.AddBinding(ViewModel, vm => vm.Leader, w => w.Subject).InitializeFromSource();

			evmeAccountant.SetEntityAutocompleteSelectorFactory(ViewModel.AccountantSelectorFactory);
			evmeAccountant.Binding.AddBinding(ViewModel, vm => vm.Accountant, w => w.Subject).InitializeFromSource();

			yCmbCurrentSignatureLeader.ItemsList = ViewModel.AllSignatures;
			yCmbCurrentSignatureLeader.Binding.AddBinding(ViewModel, s => s.SignatureLeader, w => w.SelectedItem).InitializeFromSource();
			yCmbCurrentSignatureLeader.SetSizeRequest(250, 30);

			yCmbCurrentSignatureAccountant.ItemsList = ViewModel.AllSignatures;
			yCmbCurrentSignatureAccountant.Binding.AddBinding(ViewModel, s => s.SignatureAccountant, w => w.SelectedItem).InitializeFromSource();
			yCmbCurrentSignatureAccountant.SetSizeRequest(250, 30);

			datatextviewAddress.Binding.AddBinding(ViewModel, vm => vm.Address, w => w.Buffer.Text).InitializeFromSource();
			datatextviewJurAddress.Binding.AddBinding(ViewModel, vm => vm.JurAddress, w => w.Buffer.Text).InitializeFromSource();

			buttonNewVersion.Binding.AddBinding(ViewModel, vm => vm.CanAddNewVersion, w => w.Sensitive).InitializeFromSource();
			buttonNewVersion.Clicked += (sender, args) =>
			{
				ViewModel.AddNewVersionCommand.Execute();
				GtkHelper.WaitRedraw();
				ytreeVersions.Vadjustment.Value = 0;
			};

			buttonChangeVersionDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeVersionDate, w => w.Sensitive).InitializeFromSource();
			buttonChangeVersionDate.Clicked += (sender, args) => ViewModel.ChangeVersionStartDateCommand.Execute();

			buttonCancel.Clicked += (sender, args) => ViewModel.CancelEditingVersionCommand.Execute();

			buttonEditVersion.Binding.AddBinding(ViewModel, vm => vm.IsEditAvailable, w => w.Sensitive).InitializeFromSource();
			buttonEditVersion.Clicked += (sender, args) => ViewModel.EditVersionCommand.Execute();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveEditingVersionCommand.Execute();
		}
	}
}
