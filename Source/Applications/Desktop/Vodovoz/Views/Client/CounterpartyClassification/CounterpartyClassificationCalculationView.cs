﻿using Gtk;
using QS.Dialog;
using QS.Views.GtkUI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Counterparties.ClientClassification;
using static Vodovoz.ViewModels.Counterparties.ClientClassification.CounterpartyClassificationCalculationViewModel;

namespace Vodovoz.Views.Client.CounterpartyClassification
{
	public partial class CounterpartyClassificationCalculationView : TabViewBase<CounterpartyClassificationCalculationViewModel>
	{
		public CounterpartyClassificationCalculationView(CounterpartyClassificationCalculationViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yvboxMain.Visible = ViewModel.CanCalculateCounterpartyClassifications;

			var dangerTextColor = GdkColors.DangerText.ToHtmlColor();
			var successTextColor = GdkColors.SuccessText.ToHtmlColor();

			yspinbuttonPeriod.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.PeriodInMonths, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonABottlesCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.BottlesCountAClassificationFrom, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonCBottlesCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.BottlesCountCClassificationTo, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonXOrdersCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.OrdersCountXClassificationFrom, w => w.ValueAsInt)
				.InitializeFromSource();

			yspinbuttonZOrdersCount.Binding
				.AddBinding(ViewModel.CalculationSettings, s => s.OrdersCountZClassificationTo, w => w.ValueAsInt)
				.InitializeFromSource();

			ybuttonCalculate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanOpenEmailSettingsDialog, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonCalculate.Clicked += (s, e) => ViewModel.OpenEmailSettingsDialogCommand.Execute();

			ybuttonCancel.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCancel, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonCancel.Clicked += (s, e) => ViewModel.CancelCommand.Execute();

			ybuttonSaveXls.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSaveReport, w => w.Sensitive)
				.AddBinding(vm => vm.CanSaveReport, w => w.Visible)
				.InitializeFromSource();
			ybuttonSaveXls.Clicked += (s, e) => ViewModel.SaveReportCommand.Execute();

			ybuttonQuite.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanQuit, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonQuite.Clicked += (s, e) => ViewModel.QuitCommand.Execute();

			yprogressbarCalculationProgress.Adjustment = new Adjustment(0, 0, 100, 1, 1, 1);

			yprogressbarCalculationProgress.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CalculationProgressValue, w => w.Adjustment.Value)
				.AddBinding(vm => vm.IsCalculationInProcess, w => w.Visible)
				.InitializeFromSource();

			ylabelCalculationinfo.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ProgressInfoLabelValue, w => w.Text)
				.AddFuncBinding(vm => vm.IsCalculationInProcess || vm.IsCalculationCompleted, w => w.Visible)
				.AddFuncBinding(vm => vm.IsCalculationInProcess ? dangerTextColor : successTextColor, w => w.ForegroundColor)
				.InitializeFromSource();

			ylabelBottleClassificationInfo.Binding
				.AddFuncBinding(ViewModel.CalculationSettings, vm => vm.BottlesCountClassificationSettingsSummary, w => w.Text)
				.InitializeFromSource();

			ylabelOrdersClassificationInfo.Binding
				.AddFuncBinding(ViewModel.CalculationSettings, vm => vm.OrdersCountClassificationSettingsSummary, w => w.Text)
				.InitializeFromSource();

			ViewModel.CommandToStartCalculationReceived += OnCommandToStartCalculationReceived;
			ViewModel.CalculationMessageReceived += OnCalculationMessageReceived;
		}

		private void OnCalculationMessageReceived(object sender, CalculationMessageEventArgs e)
		{
			Gtk.Application.Invoke((s, eventArgs) =>
			{
				ViewModel.InteractiveService.ShowMessage(
					e.ImportanceLevel,
					e.ErrorMessage);
			});
		}

		private async void OnCommandToStartCalculationReceived(object sender, EventArgs e)
		{
			await StartCalculation();
		}

		private async Task StartCalculation()
		{
			ViewModel.ReportCancelationTokenSource = new CancellationTokenSource();

			var task = Task.Run(async () =>
			{
				try
				{
					await ViewModel.StartClassificationCalculation(ViewModel.ReportCancelationTokenSource.Token);

					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.InteractiveService.ShowMessage(
							ImportanceLevel.Info,
							  $"Пересчёт классификации контрагентов завершен");
					});
				}
				catch(OperationCanceledException)
				{
					ViewModel.UpdatePropertiesAfterCancellationCommand.Execute();

					Gtk.Application.Invoke((s, eventArgs) =>
					{
						ViewModel.InteractiveService.ShowMessage(
							ImportanceLevel.Error,
							   $"Операция отменена!");
					});
				}
				catch(Exception ex)
				{
					ViewModel.UpdatePropertiesAfterExceptionCommand.Execute();

					Gtk.Application.Invoke((s, eventArgs) =>
					{
						throw ex;
					});
				}
			}, ViewModel.ReportCancelationTokenSource.Token);

			await task;
		}
	}
}
