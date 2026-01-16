using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients.Accounts
{
	/// <summary>
	/// Активация профиля юр лица в ИПЗ
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Активации аккаунтов юр лиц в ИПЗ",
		Nominative = "Активация аккаунта юр лица в ИПЗ",
		Prepositional = "Активации аккаунта юр лица в ИПЗ",
		PrepositionalPlural = "Активациях аккаунтов юр лиц в ИПЗ"
	)]
	[HistoryTrace]
	public class ExternalLegalCounterpartyAccountActivation : PropertyChangedBase
	{
		private ExternalLegalCounterpartyAccount _externalAccount;
		private AddingPhoneNumberState _addingPhoneNumberState;
		private AddingReasonForLeavingState _addingReasonForLeavingState;
		private AddingEdoAccountState _addingEdoAccountState;
		private TaxServiceCheckState _taxServiceCheckState;
		private TrueMarkCheckState _trueMarkCheckState;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }
		
		/// <summary>
		/// Аккаунт юр лица в ИПЗ
		/// </summary>
		[Display(Name = "Аккаунт юр лица в ИПЗ")]
		public virtual ExternalLegalCounterpartyAccount ExternalAccount
		{
			get => _externalAccount;
			set => SetField(ref _externalAccount, value);
		}

		/// <summary>
		/// Состояние добавления номера телефона
		/// </summary>
		[Display(Name = "Состояние добавления номера телефона")]
		public virtual AddingPhoneNumberState AddingPhoneNumberState
		{
			get => _addingPhoneNumberState;
			set => SetField(ref _addingPhoneNumberState, value);
		}
		
		//TODO 5608: что будем делать, если в ДВ поменяли цель покупки воды на NONE или просто сбросили ее?
		/// <summary>
		/// Состояние добавления причины покупки воды
		/// </summary>
		[Display(Name = "Состояние добавления причины покупки воды")]
		public virtual AddingReasonForLeavingState AddingReasonForLeavingState
		{
			get => _addingReasonForLeavingState;
			set => SetField(ref _addingReasonForLeavingState, value);
		}
		
		/// <summary>
		/// Состояние добавления ЭДО аккаунта
		/// </summary>
		[Display(Name = "Состояние добавления ЭДО аккаунта")]
		public virtual AddingEdoAccountState AddingEdoAccountState
		{
			get => _addingEdoAccountState;
			set => SetField(ref _addingEdoAccountState, value);
		}
		
		/// <summary>
		/// Состояние проверки в ФНС
		/// </summary>
		[Display(Name = "Состояние проверки в ФНС")]
		public virtual TaxServiceCheckState TaxServiceCheckState
		{
			get => _taxServiceCheckState;
			set => SetField(ref _taxServiceCheckState, value);
		}
		
		/// <summary>
		/// Состояние проверки регистрации в ЧЗ
		/// </summary>
		[Display(Name = "Состояние проверки регистрации в ЧЗ")]
		public virtual TrueMarkCheckState TrueMarkCheckState
		{
			get => _trueMarkCheckState;
			set => SetField(ref _trueMarkCheckState, value);
		}

		/// <summary>
		/// Состояние активации профиля юр лица в ИПЗ
		/// </summary>
		public virtual ExternalLegalCounterpartyActivationState State
		{
			get
			{
				if(AddingPhoneNumberState == AddingPhoneNumberState.Done
					&& AddingReasonForLeavingState == AddingReasonForLeavingState.Done
					&& AddingEdoAccountState == AddingEdoAccountState.Done
					&& TaxServiceCheckState == TaxServiceCheckState.Done
					&& TrueMarkCheckState == TrueMarkCheckState.Done)
				{
					return ExternalLegalCounterpartyActivationState.Done;
				}

				return ExternalLegalCounterpartyActivationState.InProgress;
			}
		}
	}
}
