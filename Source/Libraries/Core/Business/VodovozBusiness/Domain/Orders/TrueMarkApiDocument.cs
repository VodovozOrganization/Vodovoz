using NHibernate.Type;
using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ Честного знака",
		NominativePlural = "документы Честного Знака")]

	public  class TrueMarkApiDocument : PropertyChangedBase, IDomainObject
	{
		private DateTime _creationDate;
		private Order _order;
		private Guid? _guid;
		private string _errorMessage;
		private bool _isSuccess;
		private TrueMarkApiDocumentType _type;
		private Organization _organization;

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

		[Display(Name = "Тип документа")]
		public virtual TrueMarkApiDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Организация на момент вывода из оборота")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		public enum TrueMarkApiDocumentType
		{
			[Display(Name = "Вывод из оборота")]
			Withdrawal,
			[Display(Name = "Отмена вывода из оборота")]
			WithdrawalCancellation
		}

	}
}
