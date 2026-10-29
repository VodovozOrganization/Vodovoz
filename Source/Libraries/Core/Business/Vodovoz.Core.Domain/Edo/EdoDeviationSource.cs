using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Описание типа отклонения документооборота ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "описание отклонения документооборота ЭДО",
		NominativePlural = "описания отклонений документооборота ЭДО"
	)]
	public class EdoDeviationSource : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private EdoDeviationType _deviationType;
		private string _description;
		private string _errorMessage;
		private TimeSpan _timeout;
		private bool _isActive;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип отклонения. Уникален в пределах справочника
		/// </summary>
		[Display(Name = "Тип отклонения")]
		public virtual EdoDeviationType DeviationType
		{
			get => _deviationType;
			set => SetField(ref _deviationType, value);
		}

		/// <summary>
		/// Описание
		/// </summary>
		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		/// <summary>
		/// Сообщение об ошибке, отображаемое при обнаружении отклонения этого типа
		/// </summary>
		[Display(Name = "Сообщение об ошибке")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		/// <summary>
		/// Таймаут, превышение которого считается отклонением
		/// </summary>
		[Display(Name = "Таймаут")]
		public virtual TimeSpan Timeout
		{
			get => _timeout;
			set => SetField(ref _timeout, value);
		}

		/// <summary>
		/// Признак того, что валидация по этому типу отклонения выполняется
		/// </summary>
		[Display(Name = "Активен")]
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}
	}
}
