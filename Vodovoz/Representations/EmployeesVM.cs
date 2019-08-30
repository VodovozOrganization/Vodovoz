using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Domain.WageCalculation;

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
			WageParameter wageParameterAlias = null;

			var query = UoW.Session.QueryOver<Employee>(() => employeeAlias);

			if(!Filter.ShowFired)
				query.Where(e => !e.IsFired);

			if(Filter.Category != null)
				query.Where(e => e.Category == Filter.Category);

			if(Filter.RestrictWageType.HasValue) {
				query.JoinAlias(e => e.WageCalculationParameter, () => wageParameterAlias);
				query.Where(() => wageParameterAlias.WageCalcType == Filter.RestrictWageType);
			}

			var result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.IsFired).WithAlias(() => resultAlias.IsFired)
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
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public EmployeesVM()
		{
			CreateDisposableUoW();
			CreateRepresentationFilter = () => new EmployeeFilterViewModel(ServicesConfig.CommonServices) { ShowFired = false };
		}

		public EmployeesVM(IUnitOfWork uow)
		{
			CreateRepresentationFilter = () => new EmployeeFilterViewModel(ServicesConfig.CommonServices) { ShowFired = false };
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

		public bool IsFired { get; set; }

		public string RowColor => IsFired ? "grey" : "black";

		public string EntityTitle => FullName;
	}
}