using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
				NominativePlural = "комментарии результата к недовезённому заказу",
				Nominative = "комментарий результата к недовезённому заказу",
				Prepositional = "комментарии разультата к недовезённому заказу",
				PrepositionalPlural = "комментариях результата к недовезённому заказу"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class UndeliveredOrderResultComment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private UndeliveredOrder _undeliveredOrder;
		[Display(Name = "Заказ")]
		public virtual UndeliveredOrder UndeliveredOrder
		{
			get => _undeliveredOrder;
			set => SetField(ref _undeliveredOrder, value);
		}

		private Employee _author;
		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		private string _comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		private DateTime _creationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value, () => CreationTime);
		}

		public virtual string Title => $"Комментарий результата сотрудника \"{Author.ShortName}\"";
	}
}
