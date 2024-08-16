using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Employees;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии к обсуждению рекламации",
		Nominative = "комментарий к обсуждению рекламации"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ComplaintDiscussionComment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Employee author;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value, () => Author);
		}

		private DateTime creationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime {
			get => creationTime;
			set => SetField(ref creationTime, value, () => CreationTime);
		}

		private ComplaintDiscussion complaintDiscussion;
		[Display(Name = "Обсуждение рекламации")]
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

		public IList<ComplaintFile> ComplaintFiles => Files.Cast<ComplaintFile>().ToList();

		public virtual string Title => $"Комментарий сотрудника \"{Author.ShortName}\"";
	}
}
