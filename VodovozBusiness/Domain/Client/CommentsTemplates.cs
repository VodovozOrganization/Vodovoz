using System;
using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{
	public class CommentsTemplates : PropertyChangedBase, IDomainObject
	{
		public virtual IUnitOfWork UoW { get; set; }

		#region Свойства

		public virtual int Id { get; set; }

		string commentTemplate;

		[Display(Name = "Шаблон")]
		public virtual string CommentTemplate {
			get { return commentTemplate; }
			set { SetField(ref commentTemplate, value, () => CommentTemplate); }
		}

		CommentsGroups commentsTmpGroups;

		[Display(Name = "Группа")]
		public virtual CommentsGroups CommentsTmpGroups {
			get { return commentsTmpGroups; }
			set { SetField(ref commentsTmpGroups, value, () => CommentsTmpGroups); }
		}

		#endregion


		public static IUnitOfWorkGeneric<CommentsTemplates> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<CommentsTemplates>();
			return uow;
		}
	}
}
