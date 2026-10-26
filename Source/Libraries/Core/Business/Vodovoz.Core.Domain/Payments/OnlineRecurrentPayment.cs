using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Рекуррентный онлайн платеж
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Рекуррентные онлайн платежи",
		Nominative = "Рекуррентный онлайн платеж",
		Prepositional = "Рекуррентном онлайн платеже",
		PrepositionalPlural = "Рекуррентных онлайн платежах"
	)]
	public class OnlineRecurrentPayment : PropertyChangedBase, IDomainObject
	{
		private string _token;
		private string _accountId;
		private string _cardLastFour;
		private string _cardType;
		private Guid? _externalCounterpartyId;
		private int _onlineOrderTemplateId;
		private Source _source;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Идентификатор шаблона онлайн заказа
		/// </summary>
		[Display(Name = "Идентификатор шаблона онлайн заказа")]
		public virtual int OnlineOrderTemplateId
		{
			get => _onlineOrderTemplateId;
			set => SetField(ref _onlineOrderTemplateId, value);
		}
		
		/// <summary>
		/// Идентификатор внешнего клиента/пользователя
		/// </summary>
		[Display(Name = "Идентификатор внешнего клиента/пользователя")]
		public virtual Guid? ExternalCounterpartyId
		{
			get => _externalCounterpartyId;
			set => SetField(ref _externalCounterpartyId, value);
		}

		/// <summary>
		/// Источник(ИПЗ)
		/// </summary>
		[Display(Name = "ИПЗ")]
		public virtual Source Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		/// <summary>
		/// Токен рекуррентного платежа
		/// </summary>
		[Display(Name = "Токен")]
		public virtual string Token
		{
			get => _token;
			set => SetField(ref _token, value);
		}

		/// <summary>
		/// ID аккаунта
		/// </summary>
		[Display(Name = "ID аккаунта")]
		public virtual string AccountId
		{
			get => _accountId;
			set => SetField(ref _accountId, value);
		}

		/// <summary>
		/// Последние 4 цифры карты
		/// </summary>
		[Display(Name = "Последние 4 цифры карты")]
		public virtual string CardLastFour
		{
			get => _cardLastFour;
			set => SetField(ref _cardLastFour, value);
		}

		/// <summary>
		/// Тип карты(Мир, Виза и т.д.)
		/// </summary>
		[Display(Name = "Тип карты")]
		public virtual string CardType
		{
			get => _cardType;
			set => SetField(ref _cardType, value);
		}

		internal static OnlineRecurrentPayment Create(
			Source source,
			int templateId,
			Guid? externalCounterpartyId,
			TemplateRecurrentPaymentData recurrentData) =>
			new OnlineRecurrentPayment
			{
				Source = source,
				OnlineOrderTemplateId = templateId,
				ExternalCounterpartyId = externalCounterpartyId,
				Token = recurrentData.Token,
				AccountId = recurrentData.AccountId,
				CardType = recurrentData.CardType,
				CardLastFour = recurrentData.CardLastFour
			};
	}
}
