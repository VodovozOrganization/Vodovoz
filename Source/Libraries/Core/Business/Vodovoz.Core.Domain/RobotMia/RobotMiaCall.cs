using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Extensions;

namespace Vodovoz.Core.Domain.RobotMia
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "зарегистрированные звонки робота Мия",
		Nominative = "зарегистрированный звонок робота Мия")]
	public class RobotMiaCall : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private Guid _callGuid;
		private DateTime _registeredAt;
		private string _normalizedPhoneNumber;
		private int? _сounterpartyid;
		private int? _createdOrderId;

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
		/// Уникальный идентификатор звонка
		/// </summary>
		[Display(Name = "Уникальный идентификатор звонка")]
		public virtual Guid CallGuid
		{
			get => _callGuid;
			set => SetField(ref _callGuid, value);
		}

		/// <summary>
		/// Время звонка
		/// </summary>
		[Display(Name = "Время звонка")]
		public virtual DateTime RegisteredAt
		{
			get => _registeredAt;
			set => SetField(ref _registeredAt, value);
		}

		/// <summary>
		/// Нормализованный телефон
		/// </summary>
		[Display(Name = "Нормализованный телефон")]
		public virtual string NormalizedPhoneNumber
		{
			get => _normalizedPhoneNumber;
			set
			{
				if(value != value.NormalizePhone())
				{
					throw new ArgumentException("Нельзя установить не нормализованный телефон, см. метод Vodovoz.Core.Domain.Extensions.NormalizePhone.NormalizePhone", nameof(value));
				}

				SetField(ref _normalizedPhoneNumber, value);
			}
		}

		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		[Display(Name = "Код контрагента")]
		//[HistoryIdentifier(typeof(Counterparty))] TODO: Убрать комментарий, когда перенесут контрагента
		public virtual int? CounterpartyId
		{
			get => _сounterpartyid;
			set => SetField(ref _сounterpartyid, value);
		}

		/// <summary>
		/// Код заказа, созданного по звонку
		/// </summary>
		[Display(Name = "Код заказа, созданного по звонку")]
		public virtual int? CreatedOrderId
		{
			get => _createdOrderId;
			set => SetField(ref _createdOrderId, value);
		}

		public static RobotMiaCall Create(Guid callId, string phoneNumber, DateTime? dateTime = default)
		{
			return new RobotMiaCall
			{
				NormalizedPhoneNumber = phoneNumber,
				RegisteredAt = dateTime ?? DateTime.Now,
				CallGuid = callId
			};
		}
	}
}
