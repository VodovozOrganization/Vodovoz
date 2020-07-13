using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.ViewModel
{
	public class EmployeesVM : RepresentationModelEntityBase<Employee, EmployeesVMNode>
	{
		public EmployeeFilterViewModel Filter {
			get => RepresentationFilter as EmployeeFilterViewModel;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			EmployeesVMNode resultAlias = null;
			Employee employeeAlias = null;

			var query = UoW.Session.QueryOver<Employee>(() => employeeAlias);

			if(Filter?.Status != null)
				query.Where(e => e.Status == Filter.Status);

			if(Filter?.Category != null)
				query.Where(e => e.Category == Filter.Category);

			if(Filter?.RestrictWageParameterItemType != null) {
				WageParameterItem wageParameterItemAlias = null;
				var subquery = QueryOver.Of<EmployeeWageParameter>()
					.Left.JoinAlias(x => x.WageParameterItem, () => wageParameterItemAlias)
					.Where(() => wageParameterItemAlias.WageParameterItemType == Filter.RestrictWageParameterItemType.Value)
					.Where(p => p.EndDate == null || p.EndDate >= DateTime.Today)
					.Select(p => p.Employee.Id)
				;
				query.WithSubquery.WhereProperty(e => e.Id).In(subquery);
			}

			var result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
				   .Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
				)
				.OrderBy(x => x.LastName).Asc
				.OrderBy(x => x.Name).Asc
				.OrderBy(x => x.Patronymic).Asc
				.TransformUsing(NHibernate.Transform.Transformers.AliasToBean<EmployeesVMNode>())
				.List<EmployeesVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<EmployeesVMNode>.Create()
			.AddColumn("Код").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Ф.И.О.").SetDataProperty(node => node.FullName)
			.AddColumn("Категория").SetDataProperty(node => node.EmpCatEnum.GetEnumTitle())
				.MinWidth(200)
			.AddColumn("Статус").AddEnumRenderer(node => node.Status)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		public override bool PopupMenuExist => false;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Employee updatedSubject) => true;

		#endregion

		public EmployeesVM(IUnitOfWork uow, EmployeeFilterViewModel filterViewModel)
		{
			Filter = filterViewModel;
			UoW = uow;
		}

		public EmployeesVM(EmployeeFilterViewModel filterViewModel)
		{
			Filter = filterViewModel;
		}

		public EmployeesVM()
		{
			CreateRepresentationFilter = () => new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking };
		}

		public EmployeesVM(IUnitOfWork uow)
		{
			CreateRepresentationFilter = () => new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking };
			this.UoW = uow;
		}
	}

	public class EmployeesVMNode : QS.RepresentationModel.GtkUI.INodeWithEntryFastSelect
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string EmpLastName { get; set; }
		public string EmpFirstName { get; set; }
		public string EmpMiddleName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FullName => String.Format("{0} {1} {2}", EmpLastName, EmpFirstName, EmpMiddleName);

		public EmployeeCategory EmpCatEnum { get; set; }

		public EmployeeStatus Status { get; set; }

		public string RowColor => Status == EmployeeStatus.IsFired ? "grey" : "black";

		public string EntityTitle => FullName;
	}
}