using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ Честного знака",
		NominativePlural = "документы Честного Знака")]
	[EntityPermission]
	[HistoryTrace]

	public  class TrueApiDocument : PropertyChangedBase, IDomainObject
	{
		private DateTime _creationDate;
		private Order _order;
		private Guid? _guid;
		private string _errorMessage;
		private bool _isSuccess;
		public virtual int Id { get; set; }

		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Guid")]
		public virtual Guid? Guid
		{
			get => _guid;
			set => SetField(ref _guid, value);
		}

		[Display(Name = "Документ создан")]
		public virtual bool IsSuccess
		{
			get => _isSuccess;
			set => SetField(ref _isSuccess, value);
		}

		[Display(Name = "Сообщение об ошибке")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}
	}

}
