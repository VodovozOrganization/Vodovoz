using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
				.InitializeFromSource();
			ybuttonSaveXls.Clicked += (s, e) => ViewModel.SaveReportCommand.Execute();

			ybuttonQuite.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanQuite, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonQuite.Clicked += (s, e) => ViewModel.QuiteCommand.Execute();

			ViewModel.CalculationCompleted += OnCalculationCompleted;

			yprogressbarCalculationProgress.Adjustment = new Adjustment(0, 0, 100, 1, 1, 1);

			yprogressbarCalculationProgress.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsCalculationInProcess, w => w.Visible)
				.InitializeFromSource();

			ylabelCalculationinfo.UseMarkup = true;

			ylabelCalculationinfo.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ProgressInfoLabelValue, w => w.Text)
				.AddFuncBinding(vm => vm.IsCalculationInProcess ||  vm.IsCalculationCompleted, w => w.Visible)
				.InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsCommandToStartCalculationReceived))
			{
				var task = Task.Run(() =>
				{
					ViewModel.IsCalculationInProcess = true;
					ViewModel.CalculationProgress = 0;

					try
					{
						ViewModel.StartClassificationCalculationCommand.Execute();
					}
					catch(OperationCanceledException)
					{
						Gtk.Application.Invoke((s, eventArgs) => {
							//if(ViewModel.LastGenerationErrors.Any())
							//{
							//	ViewModel.ShowWarning(string.Join("\n", ViewModel.LastGenerationErrors));
							//	ViewModel.LastGenerationErrors = Enumerable.Empty<string>();
							//}
							//else
							//{
							//	ViewModel.ShowWarning("Формирование отчета было прервано");
							//}
						});
					}
					catch(Exception ex)
					{
						Gtk.Application.Invoke((s, eventArgs) => { throw ex; });
					}
					finally
					{
						Gtk.Application.Invoke((s, eventArgs) => 
						{
							ViewModel.IsCalculationInProcess = false;
							ViewModel.IsCalculationCompleted = true;
						});
					}
				});
			}
			if(e.PropertyName == nameof(ViewModel.CalculationProgress))
			{
				Gtk.Application.Invoke((s, arg) =>
				{
					yprogressbarCalculationProgress.Adjustment.Value = ViewModel.CalculationProgress;
				});
			}
		}

		private void OnCalculationCompleted(object sender, System.EventArgs e)
		{
			if(e is CalculationCompletedEventArgs args)
			{
				var isCalculationSuccessful = args.IsCalculationSuccessful;

				Gtk.Application.Invoke((s, arg) =>
				{
					if(isCalculationSuccessful)
					{
						MessageDialogHelper.RunInfoDialog($"Пересчёт классификации контрагентов завершен");

						return;
					}

					MessageDialogHelper.RunErrorDialog(
						$"Ошибка при выполнении пересчёта классификации контрагентов. Закройте окно диалога и попробуйте снова.");
				});
			}
		}

		public override void Destroy()
		{
			ViewModel.CalculationCompleted -= OnCalculationCompleted;

			base.Destroy();
		}
	}
}
