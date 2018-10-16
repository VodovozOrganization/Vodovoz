using System;
using Gamma.Utilities;
using Gamma.ColumnConfig;
using Vodovoz.Domain.Employees;
using Gtk;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModel
{
	public class EmployeesVM : RepresentationModelEntityBase<Employee, EmployeesVMNode>
	{
		public EmployeeFilter Filter {
			get {
				return RepresentationFilter as EmployeeFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			EmployeesVMNode resultAlias = null;
			Employee employeeAlias = null;

			var query = UoW.Session.QueryOver<Employee>(() => employeeAlias);

			if(!Filter.RestrictFired) {
				query.Where(e => !e.IsFired);
			}

			if(Filter.RestrictCategory != null) {
				query.Where(e => e.Category == Filter.RestrictCategory);
			}

			#region для ускорения редактора
			var result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.IsFired).WithAlias(() => resultAlias.IsFired)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
				   .Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
				   ).OrderBy(x => x.LastName).Asc
				.OrderBy(x => x.Name).Asc
				.OrderBy(x => x.Patronymic).Asc
				.TransformUsing(NHibernate.Transform.Transformers.AliasToBean<EmployeesVMNode>())
				.List<EmployeesVMNode>();
			#endregion

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<EmployeesVMNode>.Create()
			.AddColumn("Код").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Ф.И.О.").SetDataProperty(node => node.FullName)
			.AddColumn("Категория").SetDataProperty(node => node.EmpCatEnum.GetEnumTitle())
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		public override bool PopupMenuExist {
			get {
				return false;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Employee updatedSubject)
		{
			return true;
		}

		#endregion

		public EmployeesVM(EmployeeFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public EmployeesVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new EmployeeFilter(UoW);
		}

		public EmployeesVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

	}

	public class EmployeesVMNode : INodeWithEntryFastSelect
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string EmpLastName { get; set; }
		public string EmpFirstName { get; set; }
		public string EmpMiddleName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FullName { get { return String.Format("{0} {1} {2}", EmpLastName, EmpFirstName, EmpMiddleName); } }

		public EmployeeCategory EmpCatEnum { get; set; }

		public bool IsFired { get; set; }

		public string RowColor => IsFired ? "grey" : "black";

		public string EntityTitle => FullName;
	}
}