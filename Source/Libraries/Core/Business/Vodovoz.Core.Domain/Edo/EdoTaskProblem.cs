using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Представляет проблему, связанную с задачей ЭДО
	/// </summary>
	public class EdoTaskProblem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationTime;
		private DateTime _updateTime;
		private EdoTaskProblemType _type;
		private EdoTask _edoTask;
		private string _sourceName;
		private TaskProblemState _state;
		private IObservableList<EdoTaskItem> _taskItems = new ObservableList<EdoTaskItem>();
		private IObservableList<EdoProblemCustomItem> _customItems = new ObservableList<EdoProblemCustomItem>();

		/// <summary>
		/// Уникальный идентификатор проблемы
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата и время создания проблемы
		/// </summary>
		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		/// <summary>
		/// Дата и время последнего изменения проблемы
		/// </summary>
		[Display(Name = "Время изменения")]
		public virtual DateTime UpdateTime
		{
			get => _updateTime;
			set => SetField(ref _updateTime, value);
		}

		/// <summary>
		/// Тип проблемы
		/// </summary>
		[Display(Name = "Тип")]
		public virtual EdoTaskProblemType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		/// <summary>
		/// Задача ЭДО, к которой относится проблема
		/// </summary>
		[Display(Name = "Задача")]
		public virtual EdoTask EdoTask
		{
			get => _edoTask;
			set => SetField(ref _edoTask, value);
		}

		/// <summary>
		/// Название источника, вызвавшего проблему
		/// </summary>
		[Display(Name = "Источник")]
		public virtual string SourceName
		{
			get => _sourceName;
			set => SetField(ref _sourceName, value);
		}

		/// <summary>
		/// Текущее состояние проблемы
		/// </summary>
		[Display(Name = "Состояние")]
		public virtual TaskProblemState State
		{
			get => _state;
			set => SetField(ref _state, value);
		}

		/// <summary>
		/// Список строк задачи, связанных с проблемой
		/// </summary>
		[Display(Name = "Проблемные строки задачи")]
		public virtual IObservableList<EdoTaskItem> TaskItems
		{
			get => _taskItems;
			set => SetField(ref _taskItems, value);
		}

		/// <summary>
		/// Список пользовательских строк проблемы
		/// </summary>
		[Display(Name = "Строки проблемы")]
		public virtual IObservableList<EdoProblemCustomItem> CustomItems
		{
			get => _customItems;
			set => SetField(ref _customItems, value);
		}
	}
}
