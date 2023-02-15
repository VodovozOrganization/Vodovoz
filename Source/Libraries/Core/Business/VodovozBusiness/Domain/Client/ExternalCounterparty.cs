using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	public class MobileAppCounterparty : ExternalCounterparty
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.MobileApp;
	}

	public class WebSiteCounterparty : ExternalCounterparty
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.WebSite;
	}

	public class ExternalCounterparty : PropertyChangedBase, IDomainObject
	{
		private int _externalCounterpartyCounterpartyId;
		private Phone _phone;
		private Email _email;
		private bool _isArchive;

		public virtual int Id { get; set; }

		[Display(Name = "Внешний номер клиента")]
		public virtual int ExternalCounterpartyId
		{
			get => _externalCounterpartyCounterpartyId;
			set => SetField(ref _externalCounterpartyCounterpartyId, value);
		}

		[Display(Name = "Телефон клиента")]
		public virtual Phone Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Электронная почта")]
		public virtual Email Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}

		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		public virtual CounterpartyFrom CounterpartyFrom { get; }
	}

	public enum CounterpartyFrom
	{
		MobileApp,
		WebSite
	}
}
