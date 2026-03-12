using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Документ
	/// </summary>
	public class Document : PropertyChangedBase, IDomainObject, IDocument
	{
		private int _id;
		private DateTime _timeStamp = DateTime.Now;
		private DateTime _version;
		private int? _authorId;
		private int? _lastEditorId;
		private DateTime _lastEditedTime;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Версия
		/// </summary>
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		/// <summary>
		/// Дата документа
		/// </summary>
		public virtual DateTime TimeStamp
		{
			get => _timeStamp;
			protected set => SetField (ref _timeStamp, value);
		}

		/// <summary>
		/// Автор
		/// </summary>
		[Display (Name = "Автор")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? AuthorId
		{
			get => _authorId;
			set => SetField(ref _authorId, value);
		}

		/// <summary>
		/// Последний редактор
		/// </summary>
		[Display (Name = "Последний редактор")]
		[HistoryIdentifier(TargetType = typeof(EmployeeEntity))]
		public virtual int? LastEditorId
		{
			get => _lastEditorId;
			set => SetField (ref _lastEditorId, value);
		}

		/// <summary>
		/// Последние изменения
		/// </summary>
		[Display (Name = "Последние изменения")]
		public virtual DateTime LastEditedTime
		{
			get => _lastEditedTime;
			set => SetField (ref _lastEditedTime, value);
		}

		public virtual bool CanEdit { get; set; }

		public virtual string DateString => TimeStamp.ToShortDateString() + " " + TimeStamp.ToShortTimeString();

		public virtual string Number => Id.ToString();

		/// <summary>
		/// Установка даты документа
		/// </summary>
		/// <param name="value">Дата документа</param>
		public virtual void SetTimeStamp(DateTime value)
		{
			TimeStamp = value;
		}
	}
}

