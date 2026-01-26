using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Экспорт заказа в 1С
	/// </summary>

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "экспорт заказов в 1С",
		Nominative = "экспорт заказа в 1С"
	)]

	public class OrderTo1cExport : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderEntity _order;
		private bool _isExported;
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

		[Display(Name = "Экспортирован?")]
		public virtual bool IsExported
		{
			get => _isExported;
			set => SetField(ref _isExported, value);
		}

		[Display(Name = "Дата изменения заказа")]
		public virtual DateTime LastOrderChangeDate
		{
			get => _lastOrderChangeDate;
			set => SetField(ref _lastOrderChangeDate, value);
		}

		[Display(Name = "Дата экспорта")]
		public virtual DateTime? ExportDate
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
