using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии к обсуждению жалобы",
		Nominative = "комментарий к обсуждению жалобы"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDiscussionComment : PropertyChangedBase, IDomainObject
	{
		public int Id { get; set; }

		private ComplaintDiscussion complaintDiscussion;
		[Display(Name = "Обсуждение жалобы")]
		public virtual ComplaintDiscussion ComplaintDiscussion {
			get => complaintDiscussion;
			set => SetField(ref complaintDiscussion, value, () => ComplaintDiscussion);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		IList<ComplaintFile> files = new List<ComplaintFile>();
		[Display(Name = "Приложенные файлы")]
		public virtual IList<ComplaintFile> Files {
			get => files;
			set => SetField(ref files, value, () => Files);
		}

		GenericObservableList<ComplaintFile> observableFiles;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintFile> ObservableFiles {
			get {
				if(observableFiles == null)
					observableFiles = new GenericObservableList<ComplaintFile>(Files);
				return observableFiles;
			}
		}

	}
}
