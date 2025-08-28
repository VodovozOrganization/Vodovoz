using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Приоритет погрузки МЛ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "приоритеты погрузки МЛ",
		Nominative = "приоритет погрузки МЛ")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadingDailyQueue : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _date;
		private RouteListEntity _routeList;
		private int _dailyNumber;

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
		/// Дата погрузки
		/// </summary>
		[Display(Name = "Дата погрузки")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		/// <summary>
		/// Маршрутный лист, к которому относится приоритет погрузки
		/// </summary>
		[Display(Name = "Маршрутный лист")]
		public virtual RouteListEntity RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		/// <summary>		
		/// Номер приоритета
		/// </summary>
		[Display(Name = "Номер приоритета")]
		public virtual int DailyNumber
		{
			get => _dailyNumber;
			set => SetField(ref _dailyNumber, value);
		}

		public virtual string Title => $"Приоритет погрузки {Id} для МЛ {RouteList.Id}";
	}
}
