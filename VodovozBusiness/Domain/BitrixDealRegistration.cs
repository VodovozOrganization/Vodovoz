using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	public class BitrixDealRegistration : PropertyChangedBase, IDomainObject
	{
		private uint _bitrixId;
		private DateTime _createDate;
		private DateTime? _processedDate;
		private bool _success;
		private bool _needSync;
		private Order _order;
		private string _errorDescription;

		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Дата обработки")]
		public virtual DateTime? ProcessedDate
		{
			get => _processedDate;
			set => SetField(ref _processedDate, value);
		}

		[Display(Name = "Id в Битриксе")]
		public virtual uint BitrixId
		{
			get => _bitrixId;
			set => SetField(ref _bitrixId, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Выполнено успешно")]
		public virtual bool Success
		{
			get => _success;
			set => SetField(ref _success, value);
		}

		[Display(Name = "Необходима синхронизация")]
		public virtual bool NeedSync
		{
			get => _needSync;
			set => SetField(ref _needSync, value);
		}

		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}
	}
}