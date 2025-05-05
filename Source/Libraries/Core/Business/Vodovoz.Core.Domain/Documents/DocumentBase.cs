using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Domain.Documents
{
	public class DocumentBase : PropertyChangedBase, IDomainObject, IDocument
	{
		private DateTime _timeStamp = DateTime.Now;
		private DateTime _version;
		EmployeeEntity _author;
		EmployeeEntity _lastEditor;
		DateTime _lastEditedTime;

		public virtual int Id { get; set; }

		public virtual bool CanEdit { get; set; }

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
			set => SetField(ref _timeStamp, value);
		}


		[Display(Name = "Автор")]
		public virtual EmployeeEntity Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}


		[Display(Name = "Последний редактор")]
		public virtual EmployeeEntity LastEditor
		{
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}


		[Display(Name = "Последние изменения")]
		public virtual DateTime LastEditedTime
		{
			get => _lastEditedTime;
			set => SetField(ref _lastEditedTime, value);
		}

		public virtual string DateString => TimeStamp.ToShortDateString() + " " + TimeStamp.ToShortTimeString();

		public virtual string Number => Id.ToString();
	}
}
