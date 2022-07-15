using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "событие рассылки",
		NominativePlural = "события рассылки"
	)]
	[EntityPermission]
	public abstract class BulkEmailEvent : PropertyChangedBase, IDomainObject
	{
		private DateTime _actionTime;
		private BulkEmailEventType _type;
		private Counterparty _counterparty;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual DateTime ActionTime
		{
			get => _actionTime;
			set => SetField(ref _actionTime, value);
		}

		[Display(Name = "Тип события")]
		public virtual BulkEmailEventType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}
		
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		#endregion

		public enum BulkEmailEventType
		{
			[Display(Name = "Подписка")]
			Subscribing,
			[Display(Name = "Отписка")]
			Unsubscribing
		}

		public class BulkEmailEventTypeString : NHibernate.Type.EnumStringType
		{
			public BulkEmailEventTypeString() : base(typeof(BulkEmailEventType)) { }
		}
	}
}
