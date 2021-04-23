using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Representations;

namespace Vodovoz.ReportsParameters.Orders
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class NomenclaturePlanReport : SingleUoWWidgetBase, IParametersWidget
    {
        public string Title => "Отчёт по мотивации КЦ";
        public event EventHandler<LoadReportEventArgs> LoadReport;

        private GenericObservableList<NomenclatureReportNode> nomenclatures, selectedNomenclatures;
        private GenericObservableList<EmployeeReportNode> employees, selectedEmployees;
        private IEnumerable<int> productGroupIds;
        private List<SelectedNomenclaturePlan> savedNomenclatures;

        public NomenclaturePlanReport()
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            UoW = UnitOfWorkFactory.CreateWithoutRoot();

            ybuttonSave.Clicked += YbuttonSave_Clicked;
            ybuttonSave.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
                "can_save_callcenter_motivation_report_filter");

            dateperiodReportDate.StartDate = dateperiodReportDate.EndDate = DateTime.Today;
            enumType.ShowSpecialStateAll = true;
            enumType.ItemsEnum = typeof(NomenclatureCategory);

            buttonCreateReport.Clicked += OnButtonCreateReportClicked;

            searchentityNomenclature.TextChanged += OnSearchNomenclatureTextChanged;
            searchentityEmployee.TextChanged += OnSearchEmployeeTextChanged;

            enumType.Changed += EnumType_Changed;
            btnNomenclatureAdd.Clicked += BtnNomenclatureAdd_Clicked;
            btnNomenclatureDelete.Clicked += BtnNomenclatureDelete_Clicked;
            btnEmployeeAdd.Clicked += BtnEmployeeAdd_Clicked;
            btnEmployeeDelete.Clicked += BtnEmployeeDelete_Clicked;

            ytreeviewNomenclatures.RowActivated += YtreeviewNomenclatures_RowActivated;
            ytreeviewSelectedNomenclatures.RowActivated += YtreeviewSelectedNomenclatures_RowActivated;
            ytreeviewEmployees.RowActivated += YtreeviewEmployees_RowActivated;
            ytreeviewSelectedEmployees.RowActivated += YtreeviewSelectedEmployees_RowActivated;

            yentryProductGroup.JournalButtons = Buttons.None;
            yentryProductGroup.RepresentationModel = new ProductGroupVM(UoW, new ProductGroupFilterViewModel());
            yentryProductGroup.Changed += YentryProductGroup_Changed;

            SubdivisionReportNode subdivisionResultAlias = null;
            var subdivisions = UoW.Session.QueryOver<Subdivision>()
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => subdivisionResultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => subdivisionResultAlias.Name)
                )
                .TransformUsing(Transformers.AliasToBean<SubdivisionReportNode>())
                .List<SubdivisionReportNode>()
                .OrderBy(x => x.Name);

            ycmbxSubdivision.ItemsList = subdivisions;
            ycmbxSubdivision.SetRenderTextFunc<SubdivisionReportNode>(x => x.Name);
            var nomenclaturePlanParametersProvider = new NomenclaturePlanParametersProvider(ParametersProvider.Instance);
            ycmbxSubdivision.SelectedItem = subdivisions.FirstOrDefault(x => x.Id == nomenclaturePlanParametersProvider.CallCenterSubdivisionId);
            ycmbxSubdivision.Changed += YcmbxSubdivision_Changed;

            ytreeviewNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclatureReportNode>.Create()
                .AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
                    .WrapWidth(200)
                .AddColumn("План день").AddTextRenderer(x => x.PlanDay.ToString())
                .AddColumn("План месяц").AddTextRenderer(x => x.PlanMonth.ToString())
                .Finish();

            ytreeviewSelectedNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclatureReportNode>.Create()
                .AddColumn("ТМЦ").AddTextRenderer(x => x.Name)
                    .WrapWidth(200)
                .AddColumn("План день").AddTextRenderer(x => x.PlanDay.ToString())
                .AddColumn("План месяц").AddTextRenderer(x => x.PlanMonth.ToString())
                .Finish();

            NomenclatureReportNode nomenclatureResultAlias = null;
            nomenclatures = new GenericObservableList<NomenclatureReportNode>(
                UoW.Session.QueryOver<Nomenclature>()
                .SelectList(list => list
                    .Select(x => x.Id).WithAlias(() => nomenclatureResultAlias.Id)
                    .Select(x => x.Name).WithAlias(() => nomenclatureResultAlias.Name)
                    .Select(x => x.PlanDay).WithAlias(() => nomenclatureResultAlias.PlanDay)
                    .Select(x => x.PlanMonth).WithAlias(() => nomenclatureResultAlias.PlanMonth)
                    .Select(x => x.ProductGroup.Id).WithAlias(() => nomenclatureResultAlias.ProductGroupId)
                    .Select(x => x.Category).WithAlias(() => nomenclatureResultAlias.Category)
                )
                .TransformUsing(Transformers.AliasToBean<NomenclatureReportNode>())
                .List<NomenclatureReportNode>()
                .OrderBy(x => x.Name)
                .ToList()
                );

            ytreeviewNomenclatures.ItemsDataSource = nomenclatures;

            savedNomenclatures = UoW.Session.QueryOver<SelectedNomenclaturePlan>()
                .List<SelectedNomenclaturePlan>()
                .ToList();


            selectedNomenclatures = new GenericObservableList<NomenclatureReportNode>(
               nomenclatures.Where(x => savedNomenclatures
                   .Select(s=>s.NomenclatureId).Contains(x.Id)).ToList()
            );

            ytreeviewSelectedNomenclatures.ItemsDataSource = selectedNomenclatures;

            foreach (NomenclatureReportNode nomenclatureReportNode in selectedNomenclatures)
            {
                nomenclatures.Remove(nomenclatureReportNode);
            }

            ytreeviewEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeReportNode>.Create()
                .AddColumn("ФИО").AddTextRenderer(x => x.FullName)
                .Finish();

            ytreeviewSelectedEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeReportNode>.Create()
                .AddColumn("ФИО").AddTextRenderer(x => x.FullName)
                .Finish();

            EmployeeReportNode employeeResultAlias = null;
            employees = new GenericObservableList<EmployeeReportNode>(
                UoW.Session.QueryOver<Employee>()
                    .SelectList(list => list
                        .SelectGroup(x => x.Id).WithAlias(() => employeeResultAlias.Id)
                        .Select(x => x.Name).WithAlias(() => employeeResultAlias.Name)
                        .Select(x => x.LastName).WithAlias(() => employeeResultAlias.LastName)
                        .Select(x => x.Patronymic).WithAlias(() => employeeResultAlias.Patronymic)
                        .Select(x => x.Subdivision.Id).WithAlias(() => employeeResultAlias.SubdivisionId)
                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeReportNode>())
                    .List<EmployeeReportNode>()
                    .OrderBy(x => x.FullName)
                    .ToList()
                );

            ytreeviewEmployees.ItemsDataSource = employees;

            ytreeviewSelectedEmployees.ItemsDataSource = selectedEmployees = new GenericObservableList<EmployeeReportNode>();
        }

        private void YcmbxSubdivision_Changed(object sender, EventArgs e)
        {
            RefreshEmployeeFilter();
        }

        private void YbuttonSave_Clicked(object sender, EventArgs e)
        {
            foreach (var savedNomenclature in savedNomenclatures.ToList())
            {
                if (!selectedNomenclatures.Select(x => x.Id).Contains(savedNomenclature.NomenclatureId))
                {
                    savedNomenclatures.Remove(savedNomenclature);

                    UoW.Delete(savedNomenclature);
                    UoW.Commit();
                }
            }
            foreach (NomenclatureReportNode selectedNode in selectedNomenclatures)
            {
                if (!savedNomenclatures.Select(x => x.NomenclatureId).Contains(selectedNode.Id))
                {
                    SelectedNomenclaturePlan selectedNomenclaturePlan = new SelectedNomenclaturePlan()
                    {
                        NomenclatureId = selectedNode.Id
                    };

                    UoW.Save(selectedNomenclaturePlan);
                    UoW.Commit();

                    savedNomenclatures.Add(selectedNomenclaturePlan);
                }
            }
        }

        private void YtreeviewNomenclatures_RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            SelectNomenclature(ytreeviewNomenclatures.GetSelectedObject<NomenclatureReportNode>());
        }

        private void YtreeviewSelectedNomenclatures_RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            DeselectNomenclature(ytreeviewSelectedNomenclatures.GetSelectedObject<NomenclatureReportNode>());
        }

        private void YtreeviewEmployees_RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            SelectEmployee(ytreeviewEmployees.GetSelectedObject<EmployeeReportNode>());
        }

        private void YtreeviewSelectedEmployees_RowActivated(object o, Gtk.RowActivatedArgs args)
        {
            DeselectEmployee(ytreeviewSelectedEmployees.GetSelectedObject<EmployeeReportNode>());
        }

        private void SelectNomenclature(NomenclatureReportNode node)
        {
            if (node == null)
                return;

            selectedNomenclatures.Add(node);
            nomenclatures.Remove(node);
            RefreshNomenclatureFilter();
        }

        private void DeselectNomenclature(NomenclatureReportNode node)
        {
            if (node == null)
                return;

            nomenclatures.Add(node);
            selectedNomenclatures.Remove(node);
            RefreshNomenclatureFilter();
        }

        private void SelectEmployee(EmployeeReportNode node)
        {
            if (node == null)
                return;

            selectedEmployees.Add(node);
            employees.Remove(node);
            RefreshEmployeeFilter();
        }

        private void DeselectEmployee(EmployeeReportNode node)
        {
            if (node == null)
                return;

            employees.Add(node);
            selectedEmployees.Remove(node);
            RefreshEmployeeFilter();
        }

        private void BtnNomenclatureAdd_Clicked(object sender, EventArgs e)
        {
            SelectNomenclature(ytreeviewNomenclatures.GetSelectedObject<NomenclatureReportNode>());
        }

        private void BtnNomenclatureDelete_Clicked(object sender, EventArgs e)
        {
            DeselectNomenclature(ytreeviewSelectedNomenclatures.GetSelectedObject<NomenclatureReportNode>());
        }

        private void BtnEmployeeAdd_Clicked(object sender, EventArgs e)
        {
            SelectEmployee(ytreeviewEmployees.GetSelectedObject<EmployeeReportNode>());
        }

        private void BtnEmployeeDelete_Clicked(object sender, EventArgs e)
        {
            DeselectEmployee(ytreeviewSelectedEmployees.GetSelectedObject<EmployeeReportNode>());
        }

        private void OnSearchNomenclatureTextChanged(object sender, EventArgs e)
        {
            RefreshNomenclatureFilter();
            ytreeviewNomenclatures.SearchHighlightText = searchentityNomenclature.Text;
        }

        private void OnSearchEmployeeTextChanged(object sender, EventArgs e)
        {
            RefreshEmployeeFilter();
            ytreeviewEmployees.SearchHighlightText = searchentityNomenclature.Text;
        }

        private void EnumType_Changed(object sender, EventArgs e)
        {
            RefreshNomenclatureFilter();
        }

        private void YentryProductGroup_Changed(object sender, EventArgs e)
        {
            RefreshNomenclatureFilter();
        }

        private void RefreshNomenclatureFilter()
        {
            NomenclatureCategory? category = (enumType.SelectedItem as NomenclatureCategory?);
            var selectedProductGroup = (yentryProductGroup.Subject as ProductGroup);

            if (selectedProductGroup != null)
            {
                var productGroups = GetProductGroupsRecursive(selectedProductGroup);
                productGroupIds = productGroups.Select(x => x.Id);
            }
            else
            {
                productGroupIds = null;
            }

            var searchText = searchentityNomenclature.Text;

            ytreeviewNomenclatures.ItemsDataSource = nomenclatures
                .Where(x => x.Category == (category ?? x.Category)
                            && (productGroupIds == null || productGroupIds.Contains(x.ProductGroupId))
                            && x.Name.ToLower().Contains(searchText.ToLower()))
                .OrderBy(x => x.Name)
                .ToList();
        }

        private void RefreshEmployeeFilter()
        {
            SubdivisionReportNode subdivision = (ycmbxSubdivision.SelectedItem as SubdivisionReportNode);

            ytreeviewEmployees.ItemsDataSource = employees
                .Where(x => (subdivision == null || x.SubdivisionId == subdivision.Id)
                            && x.FullName.ToLower().Contains(searchentityEmployee.Text.ToLower()))
                .OrderBy(x => x.FullName)
                .ToList();
        }

        private void OnButtonCreateReportClicked(object sender, EventArgs e)
        {
            var reportInfo = GetReportInfo();
            LoadReport?.Invoke(this, new LoadReportEventArgs(reportInfo));
        }

        private ReportInfo GetReportInfo()
        {
            var nomenclatureIds = selectedNomenclatures.Select(x => x.Id);
            var employeeIds = selectedEmployees.Select(x => x.Id);

            var parameters = new Dictionary<string, object>
            {
                { "start_date", dateperiodReportDate.StartDate },
                { "end_date", dateperiodReportDate.EndDate },
                { "nomenclatures", nomenclatureIds },
                { "employees", employeeIds }
            };

            return new ReportInfo
            {
                Identifier = "Orders.NomenclaturePlanReport",
                UseUserVariables = true,
                Parameters = parameters
            };
        }

        public class NomenclatureReportNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PlanDay { get; set; }
            public int PlanMonth { get; set; }
            public int ProductGroupId { get; set; }
            public NomenclatureCategory Category { get; set; }
        }

        public class EmployeeReportNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Patronymic { get; set; }
            public int SubdivisionId { get; set; }
            public string FullName => String.Format("{0} {1} {2}", LastName, Name, Patronymic);
        }

        public class SubdivisionReportNode
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private List<ProductGroup> GetProductGroupsRecursive(ProductGroup parentProductGroup)
        {
            var productGroups = new List<ProductGroup>();
            productGroups.Add(parentProductGroup);
            foreach (var productGroup in parentProductGroup.Childs)
            {
                productGroups.AddRange(GetProductGroupsRecursive(productGroup));
            }
            return productGroups;
        }
    }
}
