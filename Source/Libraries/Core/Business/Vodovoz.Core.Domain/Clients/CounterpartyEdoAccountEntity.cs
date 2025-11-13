using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// ЭДО аккаунт контрагента
	/// </summary>
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "ЭДО аккаунты контрагента",
			Nominative = "ЭДО аккаунт контрагента",
			Accusative = "ЭДО аккаунта контрагента",
			Genitive = "ЭДО аккаунта контрагента"
		)
	]
	[HistoryTrace]
	public class CounterpartyEdoAccountEntity : PropertyChangedBase, IDomainObject
	{
		public const string TableName = "counterparty_edo_accounts";
		private bool _isDefault;
		private string _personalAccountIdInEdo;
		private ConsentForEdoStatus _consentForEdoStatus;
		private CounterpartyEntity _counterparty;
		private EdoOperator _edoOperator;
		private int? _organizationId;
		
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Использовать по умолчанию
		/// </summary>
		[Display(Name = "Использовать по умолчанию")]
		public virtual bool IsDefault
		{
			get => _isDefault;
			set => SetField(ref _isDefault, value);
		}
		
		/// <summary>
		/// Код аккаунта ЭДО
		/// </summary>
		[Display(Name = "Код аккаунта ЭДО")]
		public virtual string PersonalAccountIdInEdo
		{
			get => _personalAccountIdInEdo;
			set
			{
				var cleanedId = value == null
					? null
					: Regex.Replace(value, @"\s+", string.Empty);

				SetField(ref _personalAccountIdInEdo, cleanedId?.ToUpper());
			}
		}

		/// <summary>
		/// Клиент
		/// </summary>
		[Display(Name = "Клиент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// ЭДО оператор
		/// </summary>
		[Display(Name = "ЭДО оператор")]
		public virtual EdoOperator EdoOperator
		{
			get => _edoOperator;
			set => SetField(ref _edoOperator, value);
		}

		/// <summary>
		/// Код организации
		/// </summary>
		[Display(Name = "Код организации")]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}
		
		/// <summary>
		/// Согласие клиента на ЭДО
		/// </summary>
		[Display(Name = "Согласие клиента на ЭДО")]
		public virtual ConsentForEdoStatus ConsentForEdoStatus
		{
			get => _consentForEdoStatus;
			set => SetField(ref _consentForEdoStatus, value);
		}

		public virtual string Title
		{
			get
			{
				string personalAccountIdInEdo = null;
				string counterpartyId = null;

				if(!string.IsNullOrWhiteSpace(PersonalAccountIdInEdo))
				{
					personalAccountIdInEdo = $"({PersonalAccountIdInEdo})";
				}
				
				if(Counterparty != null)
				{
					counterpartyId = $"клиента {Counterparty.Id}";
				}
				
				return $"ЭДО аккаунт {Id} {counterpartyId} {EdoOperator?.Name} {personalAccountIdInEdo}";
			}
		}

		public static CounterpartyEdoAccountEntity Create(
			CounterpartyEntity counterpartyEntity,
			EdoOperator edoOperator,
			string personalAccountIdInEdo,
			int organizationId,
			bool isDefault,
			ConsentForEdoStatus consentForEdoStatus = ConsentForEdoStatus.Unknown
			)
		{
			return new CounterpartyEdoAccountEntity
			{
				Counterparty = counterpartyEntity,
				EdoOperator = edoOperator,
				PersonalAccountIdInEdo = personalAccountIdInEdo,
				OrganizationId = organizationId,
				IsDefault = isDefault,
				ConsentForEdoStatus = consentForEdoStatus
			};
		}
	}
}
