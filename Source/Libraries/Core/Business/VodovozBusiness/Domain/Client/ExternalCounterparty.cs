using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Contacts;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Клиент с внешнего ресурса
	/// </summary>
	public class ExternalCounterparty : PropertyChangedBase, IDomainObject
	{
		private Guid _externalCounterpartyId;
		private Phone _phone;
		private Email _email;
		private bool _isArchive;
		private DateTime? _creationDate;

		public virtual int Id { get; set; }

		/// <summary>
		/// Идентификатор пользователя в ИПЗ
		/// </summary>
		[Display(Name = "Внешний код клиента")]
		public virtual Guid ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}

		/// <summary>
		/// Телефон
		/// </summary>
		[Display(Name = "Телефон клиента")]
		public virtual Phone Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		/// <summary>
		/// Почта
		/// </summary>
		[Display(Name = "Электронная почта")]
		public virtual Email Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		/// <summary>
		/// Архивный
		/// </summary>
		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Откуда клиент
		/// </summary>
		public virtual CounterpartyFrom CounterpartyFrom { get; }

		public static ExternalCounterparty Create(Phone phone, Email email, Guid externalCounterpartyId, CounterpartyFrom counterpartyFrom)
		{
			ExternalCounterparty externalCounterparty = null;
			
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.WebSite:
					externalCounterparty = new WebSiteCounterparty();
					break;
				case CounterpartyFrom.MobileApp:
					externalCounterparty = new MobileAppCounterparty();
					break;
				case CounterpartyFrom.AiBot:
					externalCounterparty = new AiBotCounterparty();
					break;
			}
			
			externalCounterparty?.FillData(phone, email, externalCounterpartyId);

			return externalCounterparty;
		}
		
		private void FillData(Phone phone, Email email, Guid externalCounterpartyId)
		{
			Email = email;
			Phone = phone;
			ExternalCounterpartyId = externalCounterpartyId;
			CreationDate =  DateTime.Now;
		}
	}
}
