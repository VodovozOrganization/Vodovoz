using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ переноса терминала",
		NominativePlural = "документы переноса терминалов")]
	public class DriverTerminalTransferDocument : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Employee _author;
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		private DateTime _createDate;
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		private Employee _driverFrom;
		public virtual Employee DriverFrom
		{
			get => _driverFrom;
			set => SetField(ref _driverFrom, value);
		}

		private Employee _driverTo;
		public virtual Employee DriverTo
		{
			get => _driverTo;
			set => SetField(ref _driverTo, value);
		}

		private RouteList _routeListFrom;
		public virtual RouteList RouteListFrom
		{
			get => _routeListFrom;
			set => SetField(ref _routeListFrom, value);
		}

		private RouteList _routeListTo;
		public virtual RouteList RouteListTo
		{
			get => _routeListTo;
			set => SetField(ref _routeListTo, value);
		}

		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperationFrom;
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationFrom
		{
			get => _employeeNomenclatureMovementOperationFrom;
			set => SetField(ref _employeeNomenclatureMovementOperationFrom, value);
		}

		private EmployeeNomenclatureMovementOperation _employeeNomenclatureMovementOperationTo;
		public virtual EmployeeNomenclatureMovementOperation EmployeeNomenclatureMovementOperationTo
		{
			get => _employeeNomenclatureMovementOperationTo;
			set => SetField(ref _employeeNomenclatureMovementOperationTo, value);
		}
	}
}
