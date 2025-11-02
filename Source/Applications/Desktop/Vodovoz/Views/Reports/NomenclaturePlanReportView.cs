using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Project.Search.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport;

namespace Vodovoz.Views.Reports
{
	public partial class NomenclaturePlanReportView : TabViewBase<NomenclaturePlanReportViewModel>
	{
		private bool _isDestroyed = false;
		public NomenclaturePlanReportView(NomenclaturePlanReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		#region Configure

		private void Configure()
		{
			ydateperiodReportDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodReportDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

			ycheckbuttonIncludeProceed.Binding.AddBinding(ViewModel.SelectedProceeds, p => p.InludeProceeds, w => w.Active).InitializeFromSource();

			buttonNomenclaturePlan.Clicked += (sender, args) => ViewModel.NomenclaturePlanCommand.Execute();
			buttonHelp.Clicked += (sender, args) => ViewModel.ShowInfoWindowCommand.Execute();

			ybuttonSaveProceeds.Clicked += (sender, args) => ViewModel.SaveProceedsCommand.Execute();
			ybuttonSaveProceeds.Binding.AddBinding(ViewModel, vm => vm.CanSaveCallCenterMotivationReportFilter, w => w.Sensitive).InitializeFromSource();

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

			ybuttonSaveEquipmentKinds.Binding.AddBinding(ViewModel,vm=> vm.CanSaveCallCenterMotivationReportFilter, w=>w.Sensitive).InitializeFromSource();
			ybuttonSaveEquipmentTypes.Binding.AddBinding(ViewModel, vm => vm.CanSaveCallCenterMotivationReportFilter, w => w.Sensitive).InitializeFromSource();
			ybuttonSaveNomenclatures.Binding.AddBinding(ViewModel, vm => vm.CanSaveCallCenterMotivationReportFilter, w => w.Sensitive).InitializeFromSource();

			buttonSaveReport.Clicked += (sender, args) => ViewModel.SaveReportCommand.Execute();
		}

		private void ConfigureTreeView()
		{
			var columnsConfig = Gamma.ColumnConfig.FluentColumnsConfig<NomenclaturePlanReportRow>.Create()
				.AddColumn("ФИО")
				.HeaderAlignment(.5f)
				.AddTextRenderer(row => row.Name);

			for(var i = 0; i < ViewModel.Report.Titles.Count; i++)
			{
				var index = i;

				columnsConfig.AddColumn($"{  ViewModel.Report.Titles[index] }")
					.MinWidth(70)
					.AddTextRenderer(row => row.Columns[index].ToString())
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
			yenumStatus.Changed += (sender, args) => ViewModel.EmployeeSearchCommand.Execute();

			btnEmployeeAdd.Clicked += EmployeeAdded;
			btnEmployeeDelete.Clicked += EmployeeDeleted;

			ytreeviewEmployees.RowActivated += EmployeeAdded;
			ytreeviewEmployees.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedEmployees.RowActivated += EmployeeDeleted;
			ytreeviewSelectedEmployees.Selection.Mode = SelectionMode.Multiple;

			ytreeviewEmployees.Binding.AddFuncBinding(ViewModel, vm => vm.EmployeeDataLoader.Items, w => w.ItemsDataSource).InitializeFromSource();

			ycomboboxSubdivision.ShowSpecialStateNot = true;
			ycomboboxSubdivision.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Subdivisions, w => w.ItemsList)
				.AddBinding(vm => vm.Subdivision, w => w.SelectedItem)
				.InitializeFromSource();
			ycomboboxSubdivision.Changed += (sender, args) => ViewModel.EmployeeSearchCommand.Execute();


			ytreeviewEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeReportColumn>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("ФИО").AddTextRenderer(x => x.FullName)
				.Finish();

			ytreeviewSelectedEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeReportColumn>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("ФИО").AddTextRenderer(x => x.FullName)
				.Finish();

			ytreeviewSelectedEmployees.ItemsDataSource = ViewModel.SelectedEmployees;

			ytreeviewEmployees.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewEmployees.Vadjustment.Value + ytreeviewEmployees.Vadjustment.PageSize < ytreeviewEmployees.Vadjustment.Upper ||
				   !ViewModel.IsEmployeeNextPage)
				{
					return;
				}

				ViewModel.LoadNextCommand.Execute(
					new ScrollPositionNode
					{
						ReportNodeType = NomenclaturePlanReportColumnType.Employee,
						ScrollPosition = ytreeviewEmployees.Vadjustment.Value
					});
			};

			ViewModel.EmployeeDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Gtk.Application.Invoke((s, arg) =>
				{
					if(!_isDestroyed)
					{
						ytreeviewEmployees.Binding.RefreshFromSource();
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

			ytreeviewEquipmentKinds.Binding.AddFuncBinding(ViewModel, vm => vm.EquipmentKindDataLoader.Items, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewEquipmentKinds.ColumnsConfig = FluentColumnsConfig<EquipmentKindReportColumn>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Вид оборудования").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewSelectedEquipmentKinds.ColumnsConfig = FluentColumnsConfig<EquipmentKindReportColumn>.Create()
				.AddColumn("Код").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("Вид оборудования").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewSelectedEquipmentKinds.ItemsDataSource = ViewModel.SelectedEquipmentKinds;

			ytreeviewEquipmentKinds.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewEquipmentKinds.Vadjustment.Value + ytreeviewEquipmentKinds.Vadjustment.PageSize < ytreeviewEquipmentKinds.Vadjustment.Upper ||
				   !ViewModel.IsEquipmentKindNextPage)
				{
					return;
				}

				ViewModel.LoadNextCommand.Execute(
					new ScrollPositionNode
					{
						ReportNodeType = NomenclaturePlanReportColumnType.EquipmentKind,
						ScrollPosition = ytreeviewEquipmentKinds.Vadjustment.Value
					});
			};

			ViewModel.EquipmentKindDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Gtk.Application.Invoke((s, arg) =>
				{
					if(!_isDestroyed)
					{
						ytreeviewEquipmentKinds.Binding.RefreshFromSource();
						GtkHelper.WaitRedraw();
						ytreeviewEquipmentKinds.Vadjustment.Value = ViewModel.EquipmentKindLastScrollPosition;
						ViewModel.IsEquipmentKindNextPage = true;
					}
				});
			};

			ybuttonSaveEquipmentKinds.Clicked += (sender, args) => ViewModel.EquipmentKindsSaveCommand.Execute();
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

			ytreeviewEquipmentTypes.ColumnsConfig = FluentColumnsConfig<EquipmentTypeReportColumn>.Create()
				.AddColumn("Тип оборудования").AddTextRenderer(x => x.EquipmentType.GetEnumTitle())
				.Finish();

			ytreeviewEquipmentTypes.ItemsDataSource = ViewModel.EquipmentTypes;

			ytreeviewSelectedEquipmentTypes.ColumnsConfig = FluentColumnsConfig<EquipmentTypeReportColumn>.Create()
				.AddColumn("Тип оборудования").AddTextRenderer(x => x.EquipmentType.GetEnumTitle())
				.Finish();

			ytreeviewSelectedEquipmentTypes.ItemsDataSource = ViewModel.SelectedEquipmentTypes;

			ybuttonSaveEquipmentTypes.Clicked += (sender, args) => ViewModel.EquipmentTypesSaveCommand.Execute();
		}

		private void ConfigureNomenclatures()
		{
			SearchView nomenclatureSearchView = new SearchView(ViewModel.NomenclatureSearchVM);
			hboxNomenclatureSearch.Add(nomenclatureSearchView);
			nomenclatureSearchView.Show();

			yenumKind.ShowSpecialStateAll = true;
			yenumKind.ItemsEnum = typeof(NomenclatureCategory);
			yenumKind.Binding.AddBinding(ViewModel, vm => vm.NomenclatureCategory, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumKind.Changed += (sender, args) => ViewModel.NomenclatureSearchCommand.Execute();

			btnNomenclatureAdd.Clicked += NomenclatureAdded;
			btnNomenclatureDelete.Clicked += NomenclatureDeleted;

			ytreeviewNomenclatures.RowActivated += NomenclatureAdded;
			ytreeviewNomenclatures.Selection.Mode = SelectionMode.Multiple;

			ytreeviewSelectedNomenclatures.RowActivated += NomenclatureDeleted;
			ytreeviewSelectedNomenclatures.Selection.Mode = SelectionMode.Multiple;

			entityentryProductGroup.ViewModel = ViewModel.ProductGroupEntityEntryViewModel;

			ytreeviewNomenclatures.Binding.AddFuncBinding(ViewModel, vm => vm.NomenclatureDataLoader.Items, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclatureReportColumn>.Create()
				.AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
				.WrapWidth(200).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("План день\nпо умол-\nчанию").AddTextRenderer(x => x.PlanDay.ToString())
				.AddColumn("План месяц\nпо умол-\nчанию").AddTextRenderer(x => x.PlanMonth.ToString())
				.Finish();

			ytreeviewSelectedNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclatureReportColumn>.Create()
				.AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
				.WrapWidth(200).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("План день\nпо умол-\nчанию").AddTextRenderer(x => x.PlanDay.ToString())
				.AddColumn("План месяц\nпо умол-\nчанию").AddTextRenderer(x => x.PlanMonth.ToString())
				.Finish();

			ytreeviewSelectedNomenclatures.ItemsDataSource = ViewModel.SelectedNomenclatures;

			ybuttonSaveNomenclatures.Clicked += (sender, args) => ViewModel.NomenclaturesSaveCommand.Execute();

			ytreeviewNomenclatures.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewNomenclatures.Vadjustment.Value + ytreeviewNomenclatures.Vadjustment.PageSize < ytreeviewNomenclatures.Vadjustment.Upper ||
				   !ViewModel.IsNomenclatureNextPage)
				{
					return;
				}

				ViewModel.LoadNextCommand.Execute(
					new ScrollPositionNode
					{
						ReportNodeType = NomenclaturePlanReportColumnType.Nomenclature,
						ScrollPosition = ytreeviewNomenclatures.Vadjustment.Value
					});
			};

			ViewModel.NomenclatureDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Gtk.Application.Invoke((s, arg) =>
				{
					if(!_isDestroyed)
					{
						ytreeviewNomenclatures.Binding.RefreshFromSource();
						GtkHelper.WaitRedraw();
						ytreeviewNomenclatures.Vadjustment.Value = ViewModel.NomenclatureLastScrollPosition;
						ViewModel.IsNomenclatureNextPage = true;
					}
				});
			};

			ViewModel.NomenclatureDataLoader.LoadData(false);
		}

		#endregion

		#region Select and deselect filter rows

		private void NomenclatureAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewNomenclatures.GetSelectedObjects<NomenclatureReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Select,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewNomenclatures.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		public void NomenclatureDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedNomenclatures.GetSelectedObjects<NomenclatureReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Deselect,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewNomenclatures.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		private void EmployeeAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEmployees.GetSelectedObjects<EmployeeReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Select,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewEmployees.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}
		private void EmployeeDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEmployees.GetSelectedObjects<EmployeeReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Deselect,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewEmployees.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		private void EquipmentKindAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEquipmentKinds.GetSelectedObjects<EquipmentKindReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Select,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewEquipmentKinds.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		private void EquipmentKindDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEquipmentKinds.GetSelectedObjects<EquipmentKindReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Deselect,
				NomenclaturePlanReportColumns = nodes,
				ScrollPosition = ytreeviewEquipmentKinds.Vadjustment.Value
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		private void EquipmentTypeAdded(object sender, EventArgs e)
		{
			var nodes = ytreeviewEquipmentTypes.GetSelectedObjects<EquipmentTypeReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Select,
				NomenclaturePlanReportColumns = nodes
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		private void EquipmentTypeDeleted(object sender, EventArgs e)
		{
			var nodes = ytreeviewSelectedEquipmentTypes.GetSelectedObjects<EquipmentTypeReportColumn>();
			var filterRow = new NomenclaturePlanFilterRowSelectNode
			{
				FilterRowSelectType = FilterRowSelectType.Deselect,
				NomenclaturePlanReportColumns = nodes
			};

			ViewModel.SelectNodeCommand.Execute(filterRow);
		}

		#endregion

		public override void Dispose()
		{
			_isDestroyed = true;
			ViewModel.NomenclatureDataLoader.CancelLoading();
			ViewModel.EmployeeDataLoader.CancelLoading();
			ViewModel.EquipmentKindDataLoader.CancelLoading();
			base.Destroy();
			ViewModel.Dispose();
		}
	}
}
