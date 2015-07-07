using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Шаблоны комментариев", ObjectName = "шаблон комментария")]
	public class CommentTemplate : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string comment;

		public virtual string Comment { 
			get { return comment; } 
			set { SetField (ref comment, value, () => Comment); }
		}

		#endregion

		public CommentTemplate ()
		{
			Comment = String.Empty;
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (Comment))
				yield return new ValidationResult ("Текст комментария должен быть заполнен.", new [] { "Comment" });
		}

		#endregion
	}
}

