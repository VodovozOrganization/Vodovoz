using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Warehouses;

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
	public class CarLoadDocumentEntity : Document
	{
		private RouteListEntity _routeList;
		private Warehouse _warehouse;
		private CarLoadDocumentLoadOperationState _loadOperationState;
		private string _comment;
		private IObservableList<CarLoadDocumentItemEntity> _items = new ObservableList<CarLoadDocumentItemEntity>();

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
		/// Склад
		/// </summary>
		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
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
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
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

		public virtual string Title => $"Талон погрузки №{Id} от {TimeStamp:d}";
	}
}
