using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamma.GtkWidgets;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Dialog.GtkUI;
using QS.ErrorReporting;
using QS.Utilities;
using QS.Utilities.Debug;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Logistic
{
	public partial class DriversInfoExportView : TabViewBase<DriversInfoExportViewModel>
	{
		private bool _isDestroyed;

		public DriversInfoExportView(DriversInfoExportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
			Destroyed += (sender, args) => _isDestroyed = true;
		}

		private void Configure()
		{
			enumcheckCarTypeOfUse.ExpandCheckButtons = false;
			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Truck);
			enumcheckCarTypeOfUse.AddEnumToHideList(CarTypeOfUse.Loader);
			enumcheckCarTypeOfUse.Binding.AddBinding(ViewModel, vm => vm.RestrictedCarTypesOfUse, w => w.SelectedValuesList,
				new EnumsListConverter<CarTypeOfUse>()).InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding.AddBinding(ViewModel, vm => vm.RestrictedCarOwnTypes, w => w.SelectedValuesList,
				new EnumsListConverter<CarOwnType>()).InitializeFromSource();

			datepickerPeriod.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull);
			datepickerPeriod.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull);
			datepickerPeriod.StartDate = datepickerPeriod.EndDate = DateTime.Today;

			comboEmployeeStatus.ItemsEnum = typeof(EmployeeStatus);
			comboEmployeeStatus.Binding.AddBinding(ViewModel, vm => vm.EmployeeStatus, w => w.SelectedItemOrNull);

			comboTypeOfDriversInfoExport.ItemsEnum = typeof(DriversInfoExportType);
			comboTypeOfDriversInfoExport.Binding.AddBinding(ViewModel, vm => vm.DriversInfoExportType, w => w.SelectedItem);

			comboPlanFact.ItemsEnum = typeof(DriversInfoExportPlanFactType);
			comboPlanFact.Binding.AddBinding(ViewModel, vm => vm.DriversInfoExportPlanFactType, w => w.SelectedItem);

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

				if(!_isDestroyed)
				{
					Gtk.Application.Invoke((s, eventArgs) =>
					{
						if(ViewModel.DriversInfoExportType == DriversInfoExportType.RouteListGrouping)
						{
							ConfigureTreeViewForRouteListGrouping(ViewModel.IsPlan, ViewModel.IsFact);
						}
						else
						{
							ConfigureTreeViewForDriverCarGrouping(ViewModel.IsPlan, ViewModel.IsFact);
						}

						ViewModel.Items = new GenericObservableList<DriverInfoNode>(items.ToList());
					});
				}
				
			}
			catch(Exception ex)
			{
				if(ex.FindExceptionTypeInInner<TimeoutException>() != null)
				{
					Gtk.Application.Invoke((s, eventArgs) =>
						MessageDialogHelper.RunWarningDialog("Превышено время ожидания выполнения запроса.\nПопробуйте уменьшить период",
							"Таймаут"));
				}
				else
				{
					Gtk.Application.Invoke((s, eventArgs) => throw ex);
				}
			}
			finally
			{
				if(!_isDestroyed)
				{
					Gtk.Application.Invoke((s, eventArgs) =>
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

		private void ConfigureTreeViewForRouteListGrouping(bool isPlan, bool isFact)
		{
			var columnsConfig = ColumnsConfigFactory.Create<DriverInfoNode>()
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
				.AddColumn("Планируемое\nрасстояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.RouteListPlanedDistance ?? 0)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Рассчитанное\nрасстояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.RouteListRecalculatedDistance ?? 0)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Подтвержденное\nрасстояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.RouteListConfirmedDistance)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя за МЛ\n(план+факт)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverRouteListWageForRouteListGroupingString)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя\n  за период")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverPeriodWageString)
					.XAlign(0.5f)
				.AddColumn("Гос. номер авто")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CarRegNumber)
					.XAlign(0.5f)
				.AddColumn("Принадлежность авто")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.CarOwnType)
					.XAlign(0.5f)
				.AddColumn("Тип авто")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.CarTypeOfUse)
					.XAlign(0.5f)
				.AddColumn(" Кол-во отраб.\nдней за период")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.DriverDaysWorkedCount)
					.XAlign(0.5f);
			if(isPlan)
			{
				columnsConfig
					.AddColumn("Адреса\n  план")
						.AddNumericRenderer(x => x.RouteListItemCountPlanned.ToString())
						.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("Адреса\n  факт")
					.AddTextRenderer(x => x.RouteListItemCountFact)
					.XAlign(0.5f);
			}

			columnsConfig.AddColumn("       19л\nот клиента")
				.AddNumericRenderer(x => x.RouteListReturnedBottlesCount)
				.XAlign(0.5f);

			if(isPlan)
			{
				columnsConfig.AddColumn(" 19л\nплан")
					.AddNumericRenderer(x => x.Vol19LBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn(" 19л\nфакт")
					.AddTextRenderer(x => x.Vol19LBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("  6л\nплан")
					.AddNumericRenderer(x => x.Vol6LBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("  6л\nфакт")
					.AddTextRenderer(x => x.Vol6LBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("1.5л\nплан")
					.AddNumericRenderer(x => x.Vol1500MlBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("1.5л\nфакт")
					.AddTextRenderer(x => x.Vol1500MlBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("0.6л\nплан")
					.AddNumericRenderer(x => x.Vol600MlBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("0.6л\nфакт")
					.AddTextRenderer(x => x.Vol600MlBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("обор.\nплан")
					.AddNumericRenderer(x => x.EquipmentCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("обор.\nфакт")
					.AddTextRenderer(x => x.EquipmentActualCount)
					.XAlign(0.5f);
			}

			columnsConfig
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
				.AddColumn("");

			ytreeDriversInfo.ColumnsConfig = columnsConfig.Finish();
		}

		private void ConfigureTreeViewForDriverCarGrouping(bool isPlan, bool isFact)
		{
			var columnsConfig = ColumnsConfigFactory.Create<DriverInfoNode>()
				.AddColumn("Дата МЛ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.RouteListDateString)
					.XAlign(0.5f)
				.AddColumn("Принадлежность авто")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.CarOwnType)
					.XAlign(0.5f)
				.AddColumn("Тип авто")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.CarTypeOfUse)
					.XAlign(0.5f)
				.AddColumn("Гос. номер авто")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CarRegNumber)
					.XAlign(0.5f)
				.AddColumn("Водитель")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverFullName)
					.XAlign(0.5f);

			if(isPlan)
			{
				columnsConfig.AddColumn("Адреса\n  план")
					.AddNumericRenderer(x => x.RouteListItemCountPlanned.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("Адреса\n  факт")
					.AddTextRenderer(x => x.RouteListItemCountFact)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn(" 19л\nплан")
					.AddNumericRenderer(x => x.Vol19LBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn(" 19л\nфакт")
					.AddTextRenderer(x => x.Vol19LBottlesActualCount)
					.XAlign(0.5f);
			}

			columnsConfig.AddColumn("       19л\nот клиента")
				.AddNumericRenderer(x => x.RouteListReturnedBottlesCount)
				.XAlign(0.5f);

			if(isPlan)
			{
				columnsConfig.AddColumn("  6л\nплан")
					.AddNumericRenderer(x => x.Vol6LBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("  6л\nфакт")
					.AddTextRenderer(x => x.Vol6LBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("1.5л\nплан")
					.AddNumericRenderer(x => x.Vol1500MlBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("1.5л\nфакт")
					.AddTextRenderer(x => x.Vol1500MlBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("0.6л\nплан")
					.AddNumericRenderer(x => x.Vol600MlBottlesCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("0.6л\nфакт")
					.AddTextRenderer(x => x.Vol600MlBottlesActualCount)
					.XAlign(0.5f);
			}

			if(isPlan)
			{
				columnsConfig.AddColumn("обор.\nплан")
					.AddNumericRenderer(x => x.EquipmentCount.ToString())
					.XAlign(0.5f);
			}

			if(isFact)
			{
				columnsConfig.AddColumn("обор.\nфакт")
					.AddTextRenderer(x => x.EquipmentActualCount)
					.XAlign(0.5f);
			}

			columnsConfig
				.AddColumn("     19л\nнедовозы")
					.AddTextRenderer(x => x.Vol19LUndelivered)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя за МЛ\n (факт+план)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverRouteListWageForDriverCarGroupingString)
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
				.AddColumn("Код водителя")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.DriverId)
				.AddColumn("Статус водителя")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(x => x.DriverStatus)
					.XAlign(0.5f)
				.AddColumn("ЗП водителя\n  за период")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DriverPeriodWageString)
					.XAlign(0.5f)
				.AddColumn(" Кол-во отраб.\nдней за период")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.DriverDaysWorkedCount)
					.XAlign(0.5f)
				.AddColumn("Дата первого МЛ")
					.AddTextRenderer(x => x.DriverFirstRouteListDateString)
					.XAlign(0.5f)
				.AddColumn("Дата последнего МЛ")
					.AddTextRenderer(x => x.DriverLastRouteListDateString)
					.XAlign(0.5f);

			ytreeDriversInfo.ColumnsConfig = columnsConfig.Finish();
		}
	}
}
