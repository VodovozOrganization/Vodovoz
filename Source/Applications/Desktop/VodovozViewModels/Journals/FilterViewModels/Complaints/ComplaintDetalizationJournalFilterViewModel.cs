using System.Collections.Generic;
using QS.Project.Filter;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Complaints
{
    public class ComplaintDetalizationJournalFilterViewModel : FilterViewModelBase<ComplaintDetalizationJournalFilterViewModel>
    {
        private ComplaintObject _complaintObject;
        private ComplaintKind _complaintOKind;

        public ComplaintDetalizationJournalFilterViewModel()
        {
            ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
            ComplaintKinds = UoW.Session.QueryOver<ComplaintKind>().List();
        }

        public ComplaintObject ComplaintObject
        {
            get => _complaintObject;
            set => UpdateFilterField(ref _complaintObject, value);
        }

        public ComplaintKind ComplaintKind
        {
            get => _complaintOKind;
            set => UpdateFilterField(ref _complaintOKind, value);
        }

        public IList<ComplaintObject> ComplaintObjects { get; }
        public IList<ComplaintKind> ComplaintKinds { get; }
    }
}
