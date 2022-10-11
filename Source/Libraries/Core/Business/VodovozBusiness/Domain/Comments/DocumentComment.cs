using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

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

		public DocumentComment() { }
	}
}
