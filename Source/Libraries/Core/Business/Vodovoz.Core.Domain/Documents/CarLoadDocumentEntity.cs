using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Талон погрузки автомобиля
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны погрузки автомобилей",
		Nominative = "талон погрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadDocumentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _version;
		private EmployeeEntity _author;
		private RouteListEntity _routeList;
		private CarLoadDocumentLoadOperationState _loadOperationState;
		private IObservableList<CarLoadDocumentItemEntity> _items = new ObservableList<CarLoadDocumentItemEntity>();

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Версия
		/// </summary>
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}


		[Display(Name = "Автор")]
		public virtual EmployeeEntity Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		/// <summary>
		/// Маршрутный лист, к которому относится талон погрузки
		/// </summary>
		[Display(Name = "Маршрутный лист")]
		public virtual RouteListEntity RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		/// <summary>
		/// Статус талона погрузки
		/// </summary>
		[Display(Name = "Статус талона погрузки")]
		public virtual CarLoadDocumentLoadOperationState LoadOperationState
		{
			get => _loadOperationState;
			set => SetField(ref _loadOperationState, value);
		}

		/// <summary>
		/// Строки талона погрузки
		/// </summary>
		[Display(Name = "Строки")]
		public virtual IObservableList<CarLoadDocumentItemEntity> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}
	}
}
