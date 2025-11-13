using Autofac;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views.Dialog;
using QSReport;
using QSWidgetLib;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Counterparties;

namespace Vodovoz.Counterparties
{
	[ToolboxItem(true)]
	public partial class CallTaskView : DialogViewBase<CallTaskViewModel>
	{
		public CallTaskView(CallTaskViewModel viewModel)
			:base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);

			labelCreator.Binding
				.AddBinding(ViewModel, vm => vm.TaskCreatorString, w => w.Text)
				.InitializeFromSource();

			comboboxImpotanceType.ItemsEnum = typeof(ImportanceDegreeType);
			comboboxImpotanceType.Binding
				.AddBinding(ViewModel.Entity, e => e.ImportanceDegree, w => w.SelectedItem)
				.InitializeFromSource();

			TaskStateComboBox.ItemsEnum = typeof(CallTaskStatus);
			TaskStateComboBox.Binding
				.AddBinding(ViewModel.Entity, s => s.TaskState, w => w.SelectedItem)
				.InitializeFromSource();

			IsTaskCompleteButton.Binding
				.AddBinding(ViewModel.Entity, s => s.IsTaskComplete, w => w.Active)
				.InitializeFromSource();

			IsTaskCompleteButton.Binding
				.AddBinding(ViewModel, vm => vm.TaskCompletedAtString, w => w.Label)
				.InitializeFromSource();

			deadlineYdatepicker.Binding
				.AddBinding(ViewModel.Entity, s => s.EndActivePeriod, w => w.Date)
				.InitializeFromSource();

			ytextviewComments.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			textviewLastComment.Binding
				.AddBinding(ViewModel, vm => vm.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			yentryTareReturn.ValidationMode = ValidationType.numeric;
			yentryTareReturn.Binding
				.AddBinding(ViewModel.Entity, s => s.TareReturn, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			textViewCommentAboutClient.Binding
				.AddFuncBinding(
					ViewModel.Entity,
					e => e.Counterparty != null ? e.Counterparty.Comment : "",
					w => w.Buffer.Text)
				.InitializeFromSource();

			vboxOldComments.Visible = true;

			entityentryAttachedEmployee.ViewModel = ViewModel.AttachedEmployeeViewModel;

			entityentryDeliveryPoint.ViewModel = ViewModel.DeliveryPointViewModel;

			entityentryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<CallTask>(ViewModel, null, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			debtByClientEntry.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyDebt, w => w.Text)
				.InitializeFromSource();

			debtByAddressEntry.Binding
				.AddBinding(ViewModel, vm => vm.DeliveryPointOrSelfDeliveryDebt, w => w.Text)
				.InitializeFromSource();

			ClientPhonesView.ViewModel = ViewModel.CounterpartyPhonesViewModel;
			DeliveryPointPhonesView.ViewModel = ViewModel.DeliveryPointPhonesViewModel;

			entryReserve.Binding
				.AddBinding(ViewModel, vm => vm.BottleReserve, w => w.Text)
				.InitializeFromSource();

			ytextviewOldComments.Binding
				.AddBinding(ViewModel, vm => vm.OldComments, w => w.Buffer.Text)
				.InitializeFromSource();

			ViewModel.SetCreateReportByCounterpartyLegacyCallback(() =>
			{
				(ViewModel.NavigationManager as ITdiCompatibilityNavigation).OpenTdiTab<ReportViewDlg>(ViewModel, OpenPageOptions.AsSlave,
					vm => { },
					dependencies => dependencies.RegisterInstance(ViewModel.Entity.CreateReportInfoByClient(ViewModel.ReportInfoFactory)));
			});

			ViewModel.SetCreateReportByDeliveryPointLegacyCallback(() =>
			{
				(ViewModel.NavigationManager as ITdiCompatibilityNavigation).OpenTdiTab<ReportViewDlg>(ViewModel, OpenPageOptions.AsSlave,
					vm => { },
					dependencies => dependencies.RegisterInstance(ViewModel.Entity.CreateReportInfoByDeliveryPoint(ViewModel.ReportInfoFactory)));
			});

			buttonAddComment.BindCommand(ViewModel.AddCommentCommand);
			buttonRevertComment.BindCommand(ViewModel.CancelLastCommentCommand);

			buttonReportByClient.BindCommand(ViewModel.CreateReportByCounterpartyCommand);
			buttonReportByDP.BindCommand(ViewModel.CreateReportByDeliveryPointCommand);

			ViewModel.SetCreateNewOrderLegacyCallback(() =>
			{
				if(ViewModel.Entity.DeliveryPoint == null)
				{
					return;
				}
				
				(ViewModel.NavigationManager as ITdiCompatibilityNavigation)
					.OpenTdiTab<OrderDlg>(
						ViewModel,
						OpenPageOptions.None,
						orderDlg =>
						{
							orderDlg.Counterparty = orderDlg.UoW.GetById<Counterparty>(ViewModel.Entity.Counterparty.Id);
							orderDlg.UpdateClientDefaultParam();
							orderDlg.DeliveryPoint = orderDlg.UoW.GetById<DeliveryPoint>(ViewModel.Entity.DeliveryPoint.Id);

							orderDlg.CallTaskWorker.TaskCreationInteractive = new GtkTaskCreationInteractive();
						});
			});

			createOrderButton.BindCommand(ViewModel.CreateNewOrderCommand);
			createTaskButton.BindCommand(ViewModel.CreateNewTaskCommand);

			buttonSplit.Clicked += OnButtonSplitClicked;
		}

		protected void OnButtonSplitClicked(object sender, EventArgs e)
		{
			tablePreviousComments.Visible = !tablePreviousComments.Visible;
			buttonSplit.Label = tablePreviousComments.Visible ? ">>" : "<<";
		}
	}
}
