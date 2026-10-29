using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Абстрактный базовый класс для пользовательского элемента проблемы ЭДО.
	/// </summary>
	public abstract class EdoProblemCustomItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private EdoTaskProblem _problem;

		/// <summary>
		/// Уникальный идентификатор элемента
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Проблема, к которой относится данный элемент
		/// </summary>
		[Display(Name = "Проблема")]
		public virtual EdoTaskProblem Problem
		{
			get => _problem;
			set => SetField(ref _problem, value);
		}

		/// <summary>
		/// Тип пользовательского элемента проблемы
		/// </summary>
		public abstract EdoProblemCustomItemType Type { get; }
	}
}
