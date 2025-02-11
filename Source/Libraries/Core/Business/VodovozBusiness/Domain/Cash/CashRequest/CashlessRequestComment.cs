using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Domain.Cash.CashRequest
{
	/// <summary>
	/// Комментарий к заявке на оплату по безналу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "комментарий к заявке на оплату по безналу",
		NominativePlural = "комментарии к заявке на оплату по безналу",
		Accusative = "комментарий к заявке на оплату по безналу",
		AccusativePlural = "комментарии к заявке на оплату по безналу",
		Prepositional = "комментарии к заявке на оплату по безналу",
		PrepositionalPlural = "комментариях к заявке на оплату по безналу",
		Genitive = "комментария к заявке на оплату по безналу",
		GenitivePlural = "комментариев к заявке на оплату по безналу")]
	[HistoryTrace]
	[EntityPermission]
	public class CashlessRequestComment : PropertyChangedBase, IDomainObject, IHasAttachedFilesInformations<CashlessRequestCommentFileInformation>
	{
		private IObservableList<CashlessRequestCommentFileInformation> _attachedFileInformations = new ObservableList<CashlessRequestCommentFileInformation>();

		private int _id;
		private int _cashlessRequestId;
		private string _text;
		private DateTime _createdAt;
		private int _authorId;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
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

		/// <summary>
		/// Идентификатор заявки на оплату по безналу
		/// </summary>
		[Display(Name = "Идентификатор заявки на оплату по безналу")]
		[HistoryIdentifier(TargetType = typeof(CashlessRequest))]
		public virtual int CashlessRequestId
		{
			get => _cashlessRequestId;
			set => SetField(ref _cashlessRequestId, value);
		}

		/// <summary>
		/// Текст
		/// </summary>
		[Display(Name = "Текст")]
		public virtual string Text
		{
			get => _text;
			set => SetField(ref _text, value);
		}

		/// <summary>
		/// Время создания
		/// </summary>
		[Display(Name = "Время создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}

		/// <summary>
		/// Автор
		/// </summary>
		[Display(Name = "Автор")]
		[HistoryIdentifier(TargetType = typeof(Employee))]
		public virtual int AuthorId
		{
			get => _authorId;
			set => SetField(ref _authorId, value);
		}

		/// <summary>
		/// Список информации о прикрепленных файлах
		/// </summary>
		public virtual IObservableList<CashlessRequestCommentFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		/// <summary>
		/// Добавление информации о файле
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new CashlessRequestCommentFileInformation
			{
				FileName = fileName,
				CashlessRequestCommentId = Id
			});
		}

		/// <summary>
		/// Удаление информации о файле
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		public virtual void DeleteFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		/// <summary>
		/// Обновление информации о файлах
		/// 
		/// Обнолвяет идентификатор комментария в информации о файлах
		/// </summary>
		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CashlessRequestCommentId = Id;
			}
		}
	}
}
