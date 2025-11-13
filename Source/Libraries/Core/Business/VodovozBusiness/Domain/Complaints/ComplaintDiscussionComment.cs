using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
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
	public class ComplaintDiscussionComment : PropertyChangedBase, IDomainObject, IHasAttachedFilesInformations<ComplaintDiscussionCommentFileInformation>
	{
		private IObservableList<ComplaintDiscussionCommentFileInformation> _attachedFileInformations = new ObservableList<ComplaintDiscussionCommentFileInformation>();

		private int _id;

		public virtual int Id
		{
			get => _id;
			set
			{
				if(value == _id)
				{
					return;
				}

				_id = value;
				UpdateFileInformations();
			}
		}

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

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<ComplaintDiscussionCommentFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		public virtual string Title => $"Комментарий сотрудника \"{Author.ShortName}\"";

		public void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new ComplaintDiscussionCommentFileInformation
			{
				FileName = fileName,
				ComplaintDiscussionCommentId = Id
			});
		}

		public void DeleteFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.ComplaintDiscussionCommentId = Id;
			}
		}
	}
}
