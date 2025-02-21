using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskProblem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationTime;
		private DateTime _updateTime;
		private EdoTask _edoTask;
		private string _sourceName;
		private TaskProblemState _state;
		private IObservableList<EdoTaskItem> _taskItems = new ObservableList<EdoTaskItem>();

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Время изменения")]
		public virtual DateTime UpdateTime
		{
			get => _updateTime;
			set => SetField(ref _updateTime, value);
		}

		[Display(Name = "Задача")]
		public virtual EdoTask EdoTask
		{
			get => _edoTask;
			set => SetField(ref _edoTask, value);
		}

		[Display(Name = "Источник")]
		public virtual string SourceName
		{
			get => _sourceName;
			set => SetField(ref _sourceName, value);
		}

		[Display(Name = "Состояние")]
		public virtual TaskProblemState State
		{
			get => _state;
			set => SetField(ref _state, value);
		}

		[Display(Name = "Проблемные строки задачи")]
		public virtual IObservableList<EdoTaskItem> TaskItems
		{
			get => _taskItems;
			set => SetField(ref _taskItems, value);
		}
	}
}
