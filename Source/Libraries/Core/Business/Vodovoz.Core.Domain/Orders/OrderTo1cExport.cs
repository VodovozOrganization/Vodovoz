using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Заказы, в которых изменялись значимые для 1с поля
	/// </summary>

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "изменённые заказы для экспорта в 1с",
		Nominative = "изменённый заказ для экспорта в 1с"
	)]

	public class OrderTo1cExport : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderEntity _order;
		private DateTime _lastOrderChangeDate;
		private DateTime? _exportDate;
		private string _error;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Код заказа (для удалённого заказа)")]
		public virtual int OrderId { get; set; }


		[Display(Name = "Дата изменения заказа")]
		public virtual DateTime LastOrderChangeDate
		{
			get => _lastOrderChangeDate;
			set => SetField(ref _lastOrderChangeDate, value);
		}

		[Display(Name = "Дата экспорта")]
		public virtual DateTime? LastExportDate
		{
			get => _exportDate;
			set => SetField(ref _exportDate, value);
		}

		[Display(Name = "Ошибка")]
		public virtual string Error
		{
			get => _error;
			set => SetField(ref _error, value);
		}
	}
}
