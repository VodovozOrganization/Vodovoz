using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.ErrorReporting;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Logistic
{
	public partial class DriversInfoExportView : TabViewBase<DriversInfoExportViewModel>
	{
		public DriversInfoExportView(DriversInfoExportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureTreeView();
			Configure();
			Destroyed += (sender, args) => isDestroyed = true;
		}

		private bool isDestroyed;

		private void Configure()
		{
			checkRaskat.RenderMode = QS.Widgets.RenderMode.Icon;
			checkRaskat.Binding.AddBinding(ViewModel, vm => vm.IsRaskat, w => w.Active);

			datepickerPeriod.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull);
			datepickerPeriod.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull);
			datepickerPeriod.StartDate = datepickerPeriod.EndDate = DateTime.Today;

			comboTypeOfUse.ItemsEnum = typeof(CarTypeOfUse);
			comboTypeOfUse.Binding.AddBinding(ViewModel, vm => vm.CarTypeOfUse, w => w.SelectedItemOrNull);

			comboEmployeeStatus.ItemsEnum = typeof(EmployeeStatus);
			comboEmployeeStatus.Binding.AddBinding(ViewModel, vm => vm.EmployeeStatus, w => w.SelectedItemOrNull);

			ylabelStatus.Binding.AddBinding(ViewModel, vm => vm.StatusMessage, w => w.Text).InitializeFromSource();

			ybuttonExport.Binding.AddBinding(ViewModel, vm => vm.CanExport, w => w.Sensitive).InitializeFromSource();
			ybuttonForm.Binding.AddBinding(ViewModel, vm => vm.CanForm, w => w.Sensitive).InitializeFromSource();

			ybuttonForm.Clicked += OnButtonFormClicked;
			ybuttonExport.Clicked += OnButtonExportClicked;

			ybuttonInfo.Clicked += (sender, args) => ViewModel.HelpCommand.Execute();

			ViewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(ViewModel.Items))
				{
					ytreeDriversInfo.ItemsDataSource = ViewModel.Items;
				}
			};
		}

		private async void OnButtonFormClicked(object sender, EventArgs args)
		{
			bool loadedSuccessfully = false;
			try
			{
				ViewModel.DataIsLoading = true;
				ViewModel.StatusMessage = "Загрузка данных водителей...";
				var items = await Task.Run(() => ViewModel.GetDriverInfoNodes());
				loadedSuccessfully = true;
				if(!isDestroyed)
				{
					Application.Invoke((s, eventArgs) => ViewModel.Items = new GenericObservableList<DriverInfoNode>(items.ToList()));
				}
			}
			catch(Exception ex)
			{
				if(ex.FindExceptionTypeInInner<TimeoutException>() != null)
				{
					Application.Invoke((s, eventArgs) =>
						MessageDialogHelper.RunWarningDialog("Превышено время ожидания выполнения запроса.\nПопробуйте уменьшить период",
							"Таймаут"));
				}
				else
				{
					Application.Invoke((s, eventArgs) => throw ex);
				}
			}
			finally
			{
				if(!isDestroyed)
				{
					Application.Invoke((s, eventArgs) =>
					{
						ViewModel.DataIsLoading = false;
						ViewModel.StatusMessage = loadedSuccessfully ? "Данные загружены" : "Ошибка при загрузке данных";
					});
				}
			}
		}

		private void OnButtonExportClicked(object sender, EventArgs e)
		{
			var parentWindow = GtkHelper.GetParentWindow(this);

			var csvFilter = new FileFilter();
			csvFilter.AddPattern("*.csv");
			csvFilter.Name = "Comma Separated Values File (*.csv)";

			var fileChooserDialog = new FileChooserDialog(
				"Сохранение выгрузки",
				parentWindow,
				FileChooserAction.Save,
				Stock.Cancel, ResponseType.Cancel, Stock.Save, ResponseType.Accept)
			{
				DoOverwriteConfirmation = true, CurrentName = $"Выгрузка по водителям {DateTime.Today:d}.csv"
			};

			fileChooserDialog.AddFilter(csvFilter);
			fileChooserDialog.ShowAll();
			if((ResponseType)fileChooserDialog.Run() == ResponseType.Accept)
			{
				if(String.IsNullOrWhiteSpace(fileChooserDialog.Filename))
				{
					fileChooserDialog.Destroy();
					return;
				}
				var fileName = fileChooserDialog.Filename;
				ViewModel.ExportPath = fileName.EndsWith(".csv") ? fileName : fileName + ".csv";
				fileChooserDialog.Destroy();
				ViewModel.ExportCommand.Execute();
			}
			else
			{
				fileChooserDialog.Destroy();
			}
		}

		private void ConfigureTreeView()
		{
			ytreeDriversInfo.ColumnsConfig = ColumnsConfigFactory.Create<DriverInfoNode>()
				.AddColumn("Код МЛ")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.RouteListId)
					.XAlign(0.5f)
				.AddColumn("Дата МЛ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.RouteListDateString)
					.XAlign(0.5f)
				.AddColumn("Статус МЛ")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.RouteListStatus)
					.XAlign(0.5f)
				.AddColumn("Код водителя")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.DriverId)
					.XAlign(0.5f)
				.AddColumn("Водитель")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverFullName)
					.XAlign(0.5f)
				.AddColumn("Статус водителя")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.DriverStatus)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя\n за МЛ план")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverRouteListWagePlannedString)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя\n за МЛ факт")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverRouteListWageFactString)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя\n  за период")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverPeriodWageString)
					.XAlign(0.5f)
				.AddColumn("Гос. номер авто")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CarRegNumber)
					.XAlign(0.5f)
				.AddColumn("Раскат")
					.HeaderAlignment(0.5f)
					.AddToggleRenderer(x => x.CarIsRaskat)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Принадлежность авто")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.CarTypeOfUse)
					.XAlign(0.5f)
				.AddColumn(" Кол-во отраб.\nдней за период")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.DriverDaysWorkedCount)
					.XAlign(0.5f)
				.AddColumn("Адреса\n  план")
					.AddNumericRenderer(x => x.RouteListItemCountPlanned.ToString())
					.XAlign(0.5f)
				.AddColumn("Адреса\n  факт")
					.AddTextRenderer(x => x.RouteListItemCountFact)
					.XAlign(0.5f)
				.AddColumn("       19л\nот клиента")
					.AddNumericRenderer(x => x.RouteListReturnedBottlesCount)
					.XAlign(0.5f)
				.AddColumn(" 19л\nплан")
					.AddNumericRenderer(x => x.Vol19LBottlesCount.ToString())
					.XAlign(0.5f)
				.AddColumn(" 19л\nфакт")
					.AddTextRenderer(x => x.Vol19LBottlesActualCount)
					.XAlign(0.5f)
				.AddColumn("  6л\nплан")
					.AddNumericRenderer(x => x.Vol6LBottlesCount.ToString())
					.XAlign(0.5f)
				.AddColumn("  6л\nфакт")
					.AddTextRenderer(x => x.Vol6LBottlesActualCount)
					.XAlign(0.5f)
				.AddColumn("1.5л\nплан")
					.AddNumericRenderer(x => x.Vol1500MlBottlesCount.ToString())
					.XAlign(0.5f)
				.AddColumn("1.5л\nфакт")
					.AddTextRenderer(x => x.Vol1500MlBottlesActualCount)
					.XAlign(0.5f)
				.AddColumn("0.6л\nплан")
					.AddNumericRenderer(x => x.Vol600MlBottlesCount.ToString())
					.XAlign(0.5f)
				.AddColumn("0.6л\nфакт")
					.AddTextRenderer(x => x.Vol600MlBottlesActualCount)
					.XAlign(0.5f)
				.AddColumn("обор.\nплан")
					.AddNumericRenderer(x => x.EquipmentCount.ToString())
					.XAlign(0.5f)
				.AddColumn("обор.\nфакт")
					.AddTextRenderer(x => x.EquipmentActualCount)
					.XAlign(0.5f)
				.AddColumn("     19л\nнедовозы")
					.AddTextRenderer(x => x.Vol19LUndelivered)
					.XAlign(0.5f)
				.AddColumn("Дата первого МЛ")
					.AddTextRenderer(x => x.DriverFirstRouteListDateString)
					.XAlign(0.5f)
				.AddColumn("Дата последнего МЛ")
					.AddTextRenderer(x => x.DriverLastRouteListDateString)
					.XAlign(0.5f)
				.AddColumn("Закреплённые районы")
					.AddTextRenderer(x => x.DriverAssignedDistricts)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(500)
				.AddColumn("Планируемые районы")
					.AddTextRenderer(x => x.DriverPlannedDistricts)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(500)
				.AddColumn("Фактические районы")
					.AddTextRenderer(x => x.DriverFactDistricts)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();
		}
	}
}
