using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Common
{
	/// <summary>
	/// Сущность для хранения дат
	/// </summary>
	public class CalendarEntity : PropertyChangedBase, IDomainObject
	{
		private DateTime _date;
		
		public virtual int Id { get; set; }

		/// <summary>
		/// Дата
		/// </summary>
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}
	}
}
