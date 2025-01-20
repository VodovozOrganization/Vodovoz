using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Employees;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "комментарии к обсуждению недовоза",
		Nominative = "комментарий к обсуждению недовоза"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveryDiscussionComment : PropertyChangedBase, IDomainObject, IHasAttachedFilesInformations<UndeliveryDiscussionCommentFileInformation>
	{
		private Employee _author;
		private DateTime _creationTime;
		private UndeliveryDiscussion _undeliveryDiscussion;
		private string _comment;
		private IObservableList<UndeliveryDiscussionCommentFileInformation> _attachedFileInformations =
			new ObservableList<UndeliveryDiscussionCommentFileInformation>();
		private int _id;

		public virtual int Id
		{
			get => _id;
			set
			{
				if(_id != value)
				{
					_id = value;

					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Обсуждение недовоза")]
		public virtual UndeliveryDiscussion UndeliveryDiscussion
		{
			get => _undeliveryDiscussion;
			set => SetField(ref _undeliveryDiscussion, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value, () => Comment);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<UndeliveryDiscussionCommentFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		public virtual string Title => $"Комментарий сотрудника \"{Author.ShortName}\"";

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new UndeliveryDiscussionCommentFileInformation
			{
				UndeliveryDiscussionCommentId = Id,
				FileName = fileName
			});
		}

		public virtual void DeleteFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.UndeliveryDiscussionCommentId = Id;
			}
		}
	}
}
