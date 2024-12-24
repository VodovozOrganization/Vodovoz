using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class EdoTask : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private EdoTaskType _taskType;
		private EdoTaskStatus _status;
		private DateTime? _startTime;
		private DateTime? _endTime;
		private ObservableList<EdoTaskProblem> _problems;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Тип")]
		public virtual EdoTaskType TaskType
		{
			get => _taskType;
			set => SetField(ref _taskType, value);
		}

		[Display(Name = "Статус")]
		public virtual EdoTaskStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Время начала")]
		public virtual DateTime? StartTime
		{
			get => _startTime;
			set => SetField(ref _startTime, value);
		}

		[Display(Name = "Время завершения")]
		public virtual DateTime? EndTime
		{
			get => _endTime;
			set => SetField(ref _endTime, value);
		}

		[Display(Name = "Проблемы")]
		public virtual ObservableList<EdoTaskProblem> Problems
		{
			get => _problems;
			set => SetField(ref _problems, value);
		}

	}
}
