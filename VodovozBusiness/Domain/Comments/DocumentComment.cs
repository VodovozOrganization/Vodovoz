using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Client;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Comments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "комментарии",
		Nominative = "комментарий"
	)]
	public class DocumentComment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Employee author;
		[Display(Name = "Автор комментария")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value);
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private IList<CallTask> callTasks;
		[Display(Name = "Задачи по обзвону")]
		public virtual IList<CallTask> CallTasks {
			get => callTasks;
			set => SetField(ref callTasks, value);
		}

		GenericObservableList<CallTask> observableComments;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CallTask> ObservableComments {
			get {
				if(observableComments == null)
					observableComments = new GenericObservableList<CallTask>(CallTasks);
				return observableComments;
			}
		}

		public DocumentComment() { }
	}
}
