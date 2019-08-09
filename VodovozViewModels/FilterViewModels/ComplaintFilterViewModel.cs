using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Project.Filter;
using QS.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.FilterViewModels
{
	public class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
	{
		public ComplaintFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		public void SetDefault(Subdivision subdivision)
		{
			if(Subdivisions == null)
				Subdivisions = new GenericObservableList<ComplaintFilterNode> { new ComplaintFilterNode { Subdivision = subdivision } };
			else if(Subdivisions.FirstOrDefault() == null)
				Subdivisions.Add(new ComplaintFilterNode { Subdivision = subdivision });
			else
				Subdivisions.First().Subdivision = subdivision;
		}

		private GenericObservableList<ComplaintFilterNode> subdivisions;
		public virtual GenericObservableList<ComplaintFilterNode> Subdivisions {
			get => subdivisions;
			set { SetField(ref subdivisions, value, () => Subdivisions);}
		}

		private ComplaintType? complaintType;
		public virtual ComplaintType? ComplaintType {
			get => complaintType;
			set => SetField(ref complaintType, value, () => ComplaintType);
		}

		private ComplaintStatuses? complaintStatus;
		public virtual ComplaintStatuses? ComplaintStatus {
			get => complaintStatus;
			set => SetField(ref complaintStatus, value, () => ComplaintStatus);
		}

		private Employee employee;
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}
	}

	public class ComplaintFilterNode : PropertyChangedBase
	{
		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		private DateTime startDate = DateTime.Now;
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime endDate = DateTime.Now;
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}

		public bool IsEmpty { get { return Subdivision == null; } }
	}
}
