using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Project.Search.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.Views.Reports
{
	public partial class NomenclaturePlanReportView : TabViewBase<NomenclaturePlanReportViewModel>
	{
		public NomenclaturePlanReportView(NomenclaturePlanReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ydateperiodReportDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodReportDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

			ycheckbuttonIncludeProceed.Binding.AddBinding(ViewModel.SelectedProceeds, p => p.InludeProceeds, w => w.Active).InitializeFromSource();

			buttonNomenclaturePlan.Clicked += (sender, args) => ViewModel.ButtonNomenclaturePlanClicked();
			buttonHelp.Clicked += (sender, args) => ViewModel.ShowInfoWindow();

			ybuttonSaveProceeds.Clicked += (sender, args) => ViewModel.ButtonSaveProceedsClicked();
			ybuttonSaveProceeds.Sensitive = ViewModel.CanSaveCallCenterMotivationReportFilter;

			ConfigureNomenclatures();
			ConfigureEmployees();
			ConfigureEquipmentKinds();
			ConfigureEquipmentTypes();

			buttonCreateReport.Clicked += (sender, args) =>
			{
				ViewModel.GenerateReport();

				ConfigureTreeView();

				ytreeViewReport.ItemsDataSource = ViewModel.Report.Rows;
				ytreeViewReport.YTreeModel.EmitModelChanged();

				ynotebookMain.NextPage();
			};

			buttonSaveReport.Clicked += (sender, args) =>
			{
				if(ViewModel.Report == null)
				{
					return;
				}

				var extension = ".xlsx";

				var filechooser = new FileChooserDialog("Сохранить отчет...",
					null,
					FileChooserAction.Save,
					"Отменить", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept)
				{
					CurrentName = $"{Tab.TabName} {ViewModel.Report.CreationDate:yyyy-MM-dd-HH-mm}{extension}"
				};

				var excelFilter = new FileFilter
				{
					Name = $"Документ Microsoft Excel ({extension})"
				};

				excelFilter.AddMimeType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
				excelFilter.AddPattern($"*{extension}");
				filechooser.AddFilter(excelFilter);

				if(filechooser.Run() == (int) ResponseType.Accept)
				{
					var path = filechooser.Filename;

					if(!path.Contains(extension))
					{
						path += extension;
					}

					filechooser.Hide();

					ViewModel.ExportReport(path);
				}

				filechooser.Destroy();
			};
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<NomenclaturePlanReportViewModel.NomenclaturePlanReportRow>.Create()
				.AddColumn("ФИО")
				.HeaderAlignment(.5f)
				.AddTextRenderer(row => row.Name);

			for(var i = 0; i < ViewModel.Report.Titles.Count; i++)
			{
				var index = i;

				columnsConfig.AddColumn($"{  ViewModel.Report.Titles[index] }")
					.MinWidth(70)
					.AddTextRenderer(row => row.Items[index].ToString())
					.WrapWidth(70)
					.WrapMode(Pango.WrapMode.WordChar);
			}

			ytreeViewReport.ColumnsConfig = columnsConfig.Finish();

			ytreeViewReport.EnableGridLines = TreeViewGridLines.Both;
		}

		private void ConfigureEmployees()
		{
			SearchView employeeSearchView = new SearchView(ViewModel.EmployeeSearchVM);
			hboxEmployeeSearch.Add(employeeSearchView);
			employeeSearchView.Show();

			yenumStatus.ShowSpecialStateAll = true;
			yenumStatus.ItemsEnum = typeof(EmployeeStatus);
			yenumStatus.Binding.AddBinding(ViewModel, vm => vm.EmployeeStatus, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumStatus.Changed += ViewModel.EmployeeSearchOnSearch;

			btnEmployeeAdd.Clicked += EmployeeAdded;
			btnEmployeeDelete.Clicked += EmployeeDeleted;

			ytreeviewEmployees.RowActivated += EmployeeAdded;
			ytreeviewEmployees.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedEmployees.RowActivated += EmployeeDeleted;
			ytreeviewSelectedEmployees.Selection.Mode = SelectionMode.Multiple;

			ycomboboxSubdivision.ShowSpecialStateNot = true;
			ycomboboxSubdivision.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Subdivisions, w => w.ItemsList)
			 .AddBinding(vm => vm.Subdivision, w => w.SelectedItem)
				.InitializeFromSource();
			ycomboboxSubdivision.Changed += ViewModel.EmployeeSearchOnSearch;

			ytreeviewEmployees.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EmployeeReportNode>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("ФИО").AddTextRenderer(x => x.FullName)
				.Finish();

			ytreeviewSelectedEmployees.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EmployeeReportNode>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("ФИО").AddTextRenderer(x => x.FullName)
				.Finish();

			ytreeviewSelectedEmployees.ItemsDataSource = ViewModel.SelectedEmployees;

			ytreeviewEmployees.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewEmployees.Vadjustment.Value + ytreeviewEmployees.Vadjustment.PageSize < ytreeviewEmployees.Vadjustment.Upper ||
				   !ViewModel.IsNomenclatureNextPage)
				{
					return;
				}

				ViewModel.LoadNextEmployees(ytreeviewEmployees.Vadjustment.Value);
			};

			ViewModel.EmployeeDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Application.Invoke((s, arg) =>
				{
					if(!ViewModel.IsDestroyed)
					{
						ytreeviewEmployees.ItemsDataSource = ViewModel.EmployeeDataLoader.Items;
						GtkHelper.WaitRedraw();
						ytreeviewEmployees.Vadjustment.Value = ViewModel.EmployeeLastScrollPosition;
						ViewModel.IsEmployeeNextPage = true;
					}
				});
			};
		}

		private void ConfigureEquipmentKinds()
		{
			SearchView EquipmentKindSearchView = new SearchView(ViewModel.EquipmentKindSearchVM);
			hboxEquipmentKindSearch.Add(EquipmentKindSearchView);
			EquipmentKindSearchView.Show();

			btnEquipmentKindAdd.Clicked += EquipmentKindAdded;
			btnEquipmentKindDelete.Clicked += EquipmentKindDeleted;

			ytreeviewEquipmentKinds.RowActivated += EquipmentKindAdded;
			ytreeviewEquipmentKinds.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedEquipmentKinds.RowActivated += EquipmentKindDeleted;
			ytreeviewSelectedEquipmentKinds.Selection.Mode = SelectionMode.Multiple;

			ytreeviewEquipmentKinds.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EquipmentKindReportNode>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Вид оборудования").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewSelectedEquipmentKinds.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EquipmentKindReportNode>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Вид оборудования").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewSelectedEquipmentKinds.ItemsDataSource = ViewModel.SelectedEquipmentKinds;

			ytreeviewEquipmentKinds.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewEquipmentKinds.Vadjustment.Value + ytreeviewEquipmentKinds.Vadjustment.PageSize < ytreeviewEquipmentKinds.Vadjustment.Upper ||
				   !ViewModel.IsNomenclatureNextPage)
				{
					return;
				}

				ViewModel.LoadNextEquipmentKinds(ytreeviewEquipmentKinds.Vadjustment.Value);
			};

			ViewModel.EquipmentKindDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Application.Invoke((s, arg) =>
				{
					if(!ViewModel.IsDestroyed)
					{
						ytreeviewEquipmentKinds.ItemsDataSource = ViewModel.EquipmentKindDataLoader.Items;
						GtkHelper.WaitRedraw();
						ytreeviewEquipmentKinds.Vadjustment.Value = ViewModel.EquipmentKindLastScrollPosition;
						ViewModel.IsEquipmentKindNextPage = true;
					}
				});
			};

			ybuttonSaveEquipmentKinds.Clicked += (sender, args) => { ViewModel.ButtonEquipmentKindsSaveClicked(); };
			ybuttonSaveEquipmentKinds.Sensitive = ViewModel.CanSaveCallCenterMotivationReportFilter;
		}

		private void ConfigureEquipmentTypes()
		{
			SearchView equipmentTypeSearchView = new SearchView(ViewModel.EquipmentTypeSearchVM);
			hboxEquipmentTypeSearch.Add(equipmentTypeSearchView);
			equipmentTypeSearchView.Show();

			btnEquipmentTypeAdd.Clicked += EquipmentTypeAdded;
			btnEquipmentTypeDelete.Clicked += EquipmentTypeDeleted;

			ytreeviewEquipmentTypes.RowActivated += EquipmentTypeAdded;
			ytreeviewEquipmentTypes.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedEquipmentTypes.RowActivated += EquipmentTypeDeleted;
			ytreeviewSelectedEquipmentTypes.Selection.Mode = SelectionMode.Multiple;

			ytreeviewEquipmentTypes.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EquipmentTypeReportNode>.Create()
				.AddColumn("Тип оборудования").AddTextRenderer(x => x.EquipmentType.GetEnumTitle())
				.Finish();

			ytreeviewEquipmentTypes.ItemsDataSource = ViewModel.EquipmentTypes;

			ytreeviewSelectedEquipmentTypes.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.EquipmentTypeReportNode>.Create()
				.AddColumn("Тип оборудования").AddTextRenderer(x => x.EquipmentType.GetEnumTitle())
				.Finish();

			ytreeviewSelectedEquipmentTypes.ItemsDataSource = ViewModel.SelectedEquipmentTypes;

			ybuttonSaveEquipmentTypes.Clicked += (sender, args) => { ViewModel.ButtonEquipmentTypesSaveClicked(); };
			ybuttonSaveEquipmentTypes.Sensitive = ViewModel.CanSaveCallCenterMotivationReportFilter;
		}

		private void ConfigureNomenclatures()
		{
			SearchView nomenclatureSearchView = new SearchView(ViewModel.NomenclatureSearchVM);
			hboxNomenclatureSearch.Add(nomenclatureSearchView);
			nomenclatureSearchView.Show();

			yenumKind.ShowSpecialStateAll = true;
			yenumKind.ItemsEnum = typeof(NomenclatureCategory);
			yenumKind.Binding.AddBinding(ViewModel, vm => vm.NomenclatureCategory, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumKind.Changed += ViewModel.NomenclatureSearchOnSearch;

			btnNomenclatureAdd.Clicked += NomenclatureAdded;
			btnNomenclatureDelete.Clicked += NomenclatureDeleted;

			ytreeviewNomenclatures.RowActivated += NomenclatureAdded;
			ytreeviewNomenclatures.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedNomenclatures.RowActivated += NomenclatureDeleted;
			ytreeviewSelectedNomenclatures.Selection.Mode = SelectionMode.Multiple;

			yentryProductGroup.SetEntityAutocompleteSelectorFactory(ViewModel.ProductGroupSelectorFactory);
			yentryProductGroup.Binding.AddBinding(ViewModel, vm => vm.ProductGroup, w => w.Subject);//.InitializeFromSource();
			yentryProductGroup.Changed += ViewModel.NomenclatureSearchOnSearch;

			ytreeviewNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.NomenclatureReportNode>.Create()
				.AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
				.WrapWidth(200).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("План день\nпо умол-\nчанию").AddTextRenderer(x => x.PlanDay.ToString())
				.AddColumn("План месяц\nпо умол-\nчанию").AddTextRenderer(x => x.PlanMonth.ToString())
				.Finish();

			ytreeviewSelectedNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclaturePlanReportViewModel.NomenclatureReportNode>.Create()
				.AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
				.WrapWidth(200).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("План день\nпо умол-\nчанию").AddTextRenderer(x => x.PlanDay.ToString())
				.AddColumn("План месяц\nпо умол-\nчанию").AddTextRenderer(x => x.PlanMonth.ToString())
				.Finish();

			ytreeviewSelectedNomenclatures.ItemsDataSource = ViewModel.SelectedNomenclatures;

			ybuttonSaveNomenclatures.Clicked += (sender, args) => { ViewModel.ButtonNomenclaturesSaveClicked(); };
			ybuttonSaveNomenclatures.Sensitive = ViewModel.CanSaveCallCenterMotivationReportFilter;

			ytreeviewNomenclatures.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewNomenclatures.Vadjustment.Value + ytreeviewNomenclatures.Vadjustment.PageSize < ytreeviewNomenclatures.Vadjustment.Upper || !ViewModel.IsNomenclatureNextPage)
				{
					return;
				}

				ViewModel.LoadNextNomenclatures(ytreeviewNomenclatures.Vadjustment.Value);
			};

			ViewModel.NomenclatureDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Application.Invoke((s, arg) =>
				{
					if(!ViewModel.IsDestroyed)
					{
						ytreeviewNomenclatures.ItemsDataSource = ViewModel.NomenclatureDataLoader.Items;
						GtkHelper.WaitRedraw();
						ytreeviewNomenclatures.Vadjustment.Value = ViewModel.NomenclatureLastScrollPosition;
						ViewModel.IsNomenclatureNextPage = true;
					}
				});
			};

			ViewModel.NomenclatureDataLoader.LoadData(false);
		}

		public void NomenclatureDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedNomenclatures.GetSelectedObjects<NomenclaturePlanReportViewModel.NomenclatureReportNode>();
			ViewModel.DeselectNomenclature(nodes);
			ViewModel.NomenclatureDataLoader.PageSize = ViewModel.NomenclatureDataLoader.Items.Count + nodes.Length;
			ViewModel.NomenclatureLastScrollPosition = ytreeviewNomenclatures.Vadjustment.Value;

			ViewModel.NomenclatureDataLoader.LoadData(ViewModel.IsNomenclatureNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewNomenclatures.Vadjustment.Value = ViewModel.NomenclatureLastScrollPosition;
			ViewModel.NomenclatureDataLoader.PageSize = ViewModel.PageSize;
		}

		private void NomenclatureAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewNomenclatures.GetSelectedObjects<NomenclaturePlanReportViewModel.NomenclatureReportNode>();
			ViewModel.SelectNomenclature(nodes);
			ViewModel.NomenclatureDataLoader.PageSize = ViewModel.NomenclatureDataLoader.Items.Count + nodes.Length;
			ViewModel.NomenclatureLastScrollPosition = ytreeviewNomenclatures.Vadjustment.Value;

			ViewModel.NomenclatureDataLoader.LoadData(ViewModel.IsNomenclatureNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewNomenclatures.Vadjustment.Value = ViewModel.NomenclatureLastScrollPosition;
			ytreeviewSelectedNomenclatures.Vadjustment.Value = ytreeviewSelectedNomenclatures.Vadjustment.Upper - ytreeviewSelectedNomenclatures.Vadjustment.PageSize;
			ViewModel.NomenclatureDataLoader.PageSize = ViewModel.PageSize;
		}

		private void EmployeeDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEmployees.GetSelectedObjects<NomenclaturePlanReportViewModel.EmployeeReportNode>();
			ViewModel.DeselectEmployee(nodes);

			ViewModel.EmployeeDataLoader.PageSize = ViewModel.EmployeeDataLoader.Items.Count + nodes.Length;
			ViewModel.EmployeeLastScrollPosition = ytreeviewEmployees.Vadjustment.Value;

			ViewModel.EmployeeDataLoader.LoadData(ViewModel.IsEmployeeNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewEmployees.Vadjustment.Value = ViewModel.EmployeeLastScrollPosition;
			ViewModel.EmployeeDataLoader.PageSize = ViewModel.PageSize;
		}

		private void EmployeeAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEmployees.GetSelectedObjects<NomenclaturePlanReportViewModel.EmployeeReportNode>();
			ViewModel.SelectEmployee(nodes);
			ViewModel.EmployeeDataLoader.PageSize = ViewModel.EmployeeDataLoader.Items.Count + nodes.Length;
			ViewModel.EmployeeLastScrollPosition = ytreeviewEmployees.Vadjustment.Value;

			ViewModel.EmployeeDataLoader.LoadData(ViewModel.IsEmployeeNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewEmployees.Vadjustment.Value = ViewModel.EmployeeLastScrollPosition;
			ytreeviewSelectedEmployees.Vadjustment.Value = ytreeviewSelectedEmployees.Vadjustment.Upper - ytreeviewSelectedEmployees.Vadjustment.PageSize;
			ViewModel.EmployeeDataLoader.PageSize = ViewModel.PageSize;
		}

		private void EquipmentKindDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEquipmentKinds.GetSelectedObjects<NomenclaturePlanReportViewModel.EquipmentKindReportNode>();
			ViewModel.DeselectEquipmentKind(nodes);

			ViewModel.EquipmentKindDataLoader.PageSize = ViewModel.EquipmentKindDataLoader.Items.Count + nodes.Length;
			ViewModel.EquipmentKindLastScrollPosition = ytreeviewEquipmentKinds.Vadjustment.Value;

			ViewModel.EquipmentKindDataLoader.LoadData(ViewModel.IsEquipmentKindNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewEquipmentKinds.Vadjustment.Value = ViewModel.EquipmentKindLastScrollPosition;
			ViewModel.EquipmentKindDataLoader.PageSize = ViewModel.PageSize;
		}

		private void EquipmentKindAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEquipmentKinds.GetSelectedObjects<NomenclaturePlanReportViewModel.EquipmentKindReportNode>();
			ViewModel.SelectEquipmentKind(nodes);
			ViewModel.EquipmentKindDataLoader.PageSize = ViewModel.EquipmentKindDataLoader.Items.Count + nodes.Length;
			ViewModel.EquipmentKindLastScrollPosition = ytreeviewEquipmentKinds.Vadjustment.Value;

			ViewModel.EquipmentKindDataLoader.LoadData(ViewModel.IsEquipmentKindNextPage = false);

			GtkHelper.WaitRedraw();
			ytreeviewEquipmentKinds.Vadjustment.Value = ViewModel.EquipmentKindLastScrollPosition;
			ytreeviewSelectedEquipmentKinds.Vadjustment.Value = ytreeviewSelectedEquipmentKinds.Vadjustment.Upper - ytreeviewSelectedEquipmentKinds.Vadjustment.PageSize;
			ViewModel.EquipmentKindDataLoader.PageSize = ViewModel.PageSize;
		}

		private void EquipmentTypeDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEquipmentTypes.GetSelectedObjects<NomenclaturePlanReportViewModel.EquipmentTypeReportNode>();
			ViewModel.DeselectEquipmentType(nodes);
		}

		private void EquipmentTypeAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEquipmentTypes.GetSelectedObjects<NomenclaturePlanReportViewModel.EquipmentTypeReportNode>();
			ViewModel.SelectEquipmentType(nodes);
		}
	}
}
