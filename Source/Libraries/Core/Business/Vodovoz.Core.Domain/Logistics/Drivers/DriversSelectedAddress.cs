using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Logistics.Drivers
{
	/// <summary>
	/// Адрес, выбранный водителем для следующей точки маршрута
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Accusative = "адрес, выбранный водителем для следующей точки маршрута",
		AccusativePlural = "адреса, выбранные водителем для следующей точки маршрута",
		Genitive = "адреса, выбранного водителем для следующей точки маршрута",
		GenitivePlural = "адресов, выбранных водителем для следующей точки маршрута",
		Nominative = "адрес, выбранный водителем для следующей точки маршрута",
		NominativePlural = "адреса, выбранные водителем для следующей точки маршрута",
		Prepositional = "адресе, выбранном водителем для следующей точки маршрута",
		PrepositionalPlural = "адресах, выбранных водителем для следующей точки маршрута")]
	[HistoryTrace]
	public class DriversSelectedAddress : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _driverId;
		private int _nextAddressId;
		private int? _previousUncompletedAddressId;
		private DateTime _selectedAt;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Id водителя
		/// </summary>
		[Display(Name = "Id водителя")]
		public virtual int DriverId
		{
			get => _driverId;
			set => SetField(ref _driverId, value);
		}

		/// <summary>
		/// Id выбранного адреса МЛ
		/// </summary>
		[Display(Name = "Id выбранного адреса МЛ")]
		public virtual int NextAddressId
		{
			get => _nextAddressId;
			set => SetField(ref _nextAddressId, value);
		}

		/// <summary>
		/// Id предыдущего адреса МЛ, который не был завершен
		/// </summary>
		[Display(Name = "Id предыдущего адреса МЛ, который не был завершен")]
		public virtual int? PreviousUncompletedAddressId
		{
			get => _previousUncompletedAddressId;
			set => SetField(ref _previousUncompletedAddressId, value);
		}

		/// <summary>
		/// Время выбора адреса
		/// </summary>
		[Display(Name = "Время выбора адреса")]
		public virtual DateTime SelectedAt
		{
			get => _selectedAt;
			set => SetField(ref _selectedAt, value);
		}
	}
}
