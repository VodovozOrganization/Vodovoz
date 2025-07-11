using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.SecureCodes
{
	/// <summary>
	/// Сгенерированный код авторизации
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "сгенерированные коды",
		Nominative = "сгенерированный код",
		Prepositional = "сгенерированном коде",
		PrepositionalPlural = "сгенерированных кодах"
	)]
	[HistoryTrace]
	public class GeneratedSecureCode : PropertyChangedBase, IDomainObject
	{
		private string _code;
		private DateTime _created;
		private SendTo _method;
		private string _target;
		private string _userPhone;
		private Source _source;
		private string _ip;
		private string _userAgent;
		private int? _counterpartyId;
		private Guid? _externalCounterpartyId;
		private bool _isUsed;

		protected GeneratedSecureCode(){ }

		private GeneratedSecureCode(
			string code,
			SendTo method,
			string target,
			string userPhone,
			Source source,
			string ip,
			string userAgent,
			int? counterpartyId,
			Guid? externalCounterpartyId
		)
		{
			Created = DateTime.Now;
			Code = code;
			Method = method;
			Target = target;
			UserPhone = userPhone;
			Source = source;
			Ip = ip;
			UserAgent = userAgent;
			CounterpartyId = counterpartyId;
			ExternalCounterpartyId = externalCounterpartyId;
			IsUsed = false;
		}

		public int Id { get; set; }

		/// <summary>
		/// Код авторизации
		/// </summary>
		[Display(Name = "Код авторизации")]
		[IgnoreHistoryTrace]
		public string Code
		{
			get => _code;
			set => SetField(ref _code, value);
		}
		
		/// <summary>
		/// Создан
		/// </summary>
		[Display(Name = "Создан")]
		[IgnoreHistoryTrace]
		public DateTime Created
		{
			get => _created;
			set => SetField(ref _created, value);
		}

		/// <summary>
		/// Тип отправки <see cref="SendTo"/>
		/// </summary>
		[Display(Name = "Тип отправки")]
		public SendTo Method
		{
			get => _method;
			set => SetField(ref _method, value);
		}

		/// <summary>
		/// Куда отправляем код
		/// </summary>
		[Display(Name = "Куда отправляем код")]
		public string Target
		{
			get => _target;
			set => SetField(ref _target, value);
		}

		/// <summary>
		/// Телефон пользователя
		/// </summary>
		[Display(Name = "Телефон пользователя")]
		public string UserPhone
		{
			get => _userPhone;
			set => SetField(ref _userPhone, value);
		}

		/// <summary>
		/// Источник запроса
		/// </summary>
		[Display(Name = "Источник запроса")]
		public Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		/// <summary>
		/// Ip адрес пользователя
		/// </summary>
		[Display(Name = "Ip адрес пользователя")]
		public string Ip
		{
			get => _ip;
			set => SetField(ref _ip, value);
		}

		/// <summary>
		/// Характеристика агента
		/// </summary>
		[Display(Name = "Характеристика агента")]
		public string UserAgent
		{
			get => _userAgent;
			set => SetField(ref _userAgent, value);
		}

		/// <summary>
		/// Id клиента в Erp
		/// </summary>
		[Display(Name = "Id клиента")]
		public int? CounterpartyId
		{
			get => _counterpartyId;
			set => SetField(ref _counterpartyId, value);
		}

		/// <summary>
		/// Id клиента/пользователя в ИПЗ
		/// </summary>
		[Display(Name = "Id клиента/пользователя в ИПЗ")]
		public Guid? ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}
		
		/// <summary>
		/// Использован
		/// </summary>
		[Display(Name = "Использован")]
		public bool IsUsed
		{
			get => _isUsed;
			set => SetField(ref _isUsed, value);
		}

		public static GeneratedSecureCode Create(
			string code,
			SendTo method,
			string target,
			string userPhone,
			Source source,
			string ip,
			string userAgent,
			int? counterpartyId,
			Guid? externalCounterpartyId
			)
		{
			return new GeneratedSecureCode(
				code, method, target, userPhone, source, ip, userAgent, counterpartyId, externalCounterpartyId);
		}
	}
}
