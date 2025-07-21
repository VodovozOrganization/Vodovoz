using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class EdoTask : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationTime;
		private DateTime _version;
		private EdoTaskStatus _status;
		private DateTime? _startTime;
		private DateTime? _endTime;
		private IObservableList<EdoTaskProblem> _problems;
		private string _cancellationReason;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Тип")]
		public abstract EdoTaskType TaskType { get; }

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
		public virtual IObservableList<EdoTaskProblem> Problems
		{
			get => _problems;
			set => SetField(ref _problems, value);
		}

		[Display(Name = "Причина отмены")]
		public virtual string CancellationReason
		{
			get => _cancellationReason;
			set => SetField(ref _cancellationReason, value);
		}
	}
}
