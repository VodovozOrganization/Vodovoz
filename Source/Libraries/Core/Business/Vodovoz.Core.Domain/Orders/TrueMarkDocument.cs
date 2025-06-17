using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Документ Честного знака
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "документ Честного знака",
		NominativePlural = "документы Честного Знака",
		Genitive = "документа Честного знака",
		GenitivePlural = "документов Честного знака",
		Accusative = "документа Честного знака",
		AccusativePlural = "документов Честного знака",
		Prepositional = "документе Честного знака",
		PrepositionalPlural = "документах Честного знака")]

	public partial class TrueMarkDocument : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationDate;
		private OrderEntity _order;
		private Guid? _guid;
		private string _errorMessage;
		private bool _isSuccess;
		private TrueMarkDocumentType _type;
		private OrganizationEntity _organization;

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
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Guid идентификатор документа
		/// </summary>
		[Display(Name = "Guid")]
		public virtual Guid? Guid
		{
			get => _guid;
			set => SetField(ref _guid, value);
		}

		/// <summary>
		/// Документ создан
		/// </summary>
		[Display(Name = "Документ создан")]
		public virtual bool IsSuccess
		{
			get => _isSuccess;
			set => SetField(ref _isSuccess, value);
		}

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		[Display(Name = "Сообщение об ошибке")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual TrueMarkDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		/// <summary>
		/// Организация на момент вывода из оборота
		/// </summary>
		[Display(Name = "Организация на момент вывода из оборота")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
	}
}
