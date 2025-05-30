using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Domain.Client
{
	public class ExternalCounterparty : ExternalCounterpartyEntity
	{
		private Phone _phone;
		private Email _email;

		[Display(Name = "Телефон клиента")]
		public new virtual Phone Phone
		{
			get => _phone;
			set => SetField(ref _phone, value);
		}

		[Display(Name = "Электронная почта")]
		public new virtual Email Email
		{
			get => _email;
			set => SetField(ref _email, value);
		}
	}
}
