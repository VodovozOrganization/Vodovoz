using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
{
	public class TaxcomEdoSettings : PropertyChangedBase, IDomainObject
	{
		private string _login;
		private string _password;
		private string _edoAccount;
		private int _organizationId;

		public virtual int Id { get; set; }

		/// <summary>
		/// Логин от кабинета в Такскоме
		/// </summary>
		public virtual string Login
		{
			get => _login;
			set => SetField(ref _login, value);
		}

		/// <summary>
		/// Пароль от кабинета в Такскоме
		/// </summary>
		public virtual string Password
		{
			get => _password;
			set => SetField(ref _password, value);
		}

		/// <summary>
		/// Id кабинета в Такскоме
		/// </summary>
		public virtual string EdoAccount
		{
			get => _edoAccount;
			set => SetField(ref _edoAccount, value);
		}

		/// <summary>
		/// Id нашей организации
		/// </summary>
		public virtual int OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}
	}
}
