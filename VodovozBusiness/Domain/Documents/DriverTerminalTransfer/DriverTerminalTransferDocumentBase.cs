using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.DriverTerminalTransfer
{
	public abstract class DriverTerminalTransferDocumentBase : PropertyChangedBase, IDomainObject
	{
		private Employee _author;
		private DateTime _createDate;
		private Employee _driverFrom;
		private Employee _driverTo;
		private RouteList _routeListFrom;
		private RouteList _routeListTo;

		public virtual int Id { get; set; }

		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		public virtual Employee DriverFrom
		{
			get => _driverFrom;
			set => SetField(ref _driverFrom, value);
		}

		public virtual Employee DriverTo
		{
			get => _driverTo;
			set => SetField(ref _driverTo, value);
		}

		public virtual RouteList RouteListFrom
		{
			get => _routeListFrom;
			set => SetField(ref _routeListFrom, value);
		}

		public virtual RouteList RouteListTo
		{
			get => _routeListTo;
			set => SetField(ref _routeListTo, value);
		}

		public abstract string Title { get; }
	}
}
