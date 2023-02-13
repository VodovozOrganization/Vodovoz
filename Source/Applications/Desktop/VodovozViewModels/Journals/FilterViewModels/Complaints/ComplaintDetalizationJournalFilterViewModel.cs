using QS.Project.Filter;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Complaints
{
	public class ComplaintDetalizationJournalFilterViewModel
		: FilterViewModelBase<ComplaintDetalizationJournalFilterViewModel>
	{
		private ComplaintObject _complaintObject;
		private ComplaintKind _complaintOKind;
		private IEnumerable<ComplaintKind> _visibleComplaintKinds;
		private ComplaintKind _restrictComplaintKind;
		private ComplaintObject _restrictComplaintObject;

		public ComplaintDetalizationJournalFilterViewModel()
		{
			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
			ComplaintKinds = UoW.Session.QueryOver<ComplaintKind>().List();
		}

		public ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(UpdateFilterField(ref _complaintObject, value))
				{
					if(value is null)
					{
						VisibleComplaintKinds = Enumerable.Empty<ComplaintKind>();
					}
					else
					{
						VisibleComplaintKinds = ComplaintKinds
							.Where(ck => ck.ComplaintObject == value);
					}
					OnPropertyChanged(nameof(CanChangeComplaintKind));
				}
			}
		}

		public ComplaintKind ComplaintKind
		{
			get => _complaintOKind;
			set => UpdateFilterField(ref _complaintOKind, value);
		}

		public IList<ComplaintObject> ComplaintObjects { get; }

		public IList<ComplaintKind> ComplaintKinds { get; }

		public ComplaintKind RestrictComplaintKind
		{
			get => _restrictComplaintKind;
			set
			{
				if(UpdateFilterField(ref _restrictComplaintKind, value))
				{
					ComplaintKind = value;
					RestrictComplaintObject = ComplaintObjects
						.Where(x => x.Id == value?.ComplaintObject?.Id)
						.FirstOrDefault();
				}
			}
		}

		public ComplaintObject RestrictComplaintObject
		{
			get => _restrictComplaintObject;
			set
			{
				if(UpdateFilterField(ref _restrictComplaintObject, value))
				{
					ComplaintObject = value;
				}
			}
		}

		public bool CanChangeComplaintObject => RestrictComplaintObject is null;

		public bool CanChangeComplaintKind => RestrictComplaintKind is null
			&& ComplaintObject != null;

		public IEnumerable<ComplaintKind> VisibleComplaintKinds
		{
			get => _visibleComplaintKinds;
			private set => UpdateFilterField(ref _visibleComplaintKinds, value);
		}
	}
}
