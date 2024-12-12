using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.Utilities.Numeric;
using System;
using System.Text;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Counterparty;

namespace CustomerAppsApi.Library.Validators
{
	public class CounterpartyModelValidator : ICounterpartyModelValidator
	{
		private const string _wrongCameFromId = "Неверно заполнено поле Откуда клиент";
		private readonly PhoneFormatter _phoneFormatter;
		private readonly ICounterpartySettings _counterpartySettings;
		private readonly StringBuilder _sb;

		public CounterpartyModelValidator(PhoneFormatter phoneFormatter, ICounterpartySettings counterpartySettings)
		{
			_phoneFormatter = phoneFormatter ?? throw new ArgumentNullException(nameof(phoneFormatter));
			_counterpartySettings = counterpartySettings ?? throw new ArgumentNullException(nameof(counterpartySettings));
			_sb = new StringBuilder();
		}
		
		public string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			_sb.Clear();
			ValidateContactInfo(counterpartyContactInfoDto.PhoneNumber);
			ValidateCameFromProperty(counterpartyContactInfoDto.CameFromId);

			return _sb.ToString();
		}
		
		public string CounterpartyDtoValidate(CounterpartyDto counterpartyDto)
		{
			_sb.Clear();

			ValidateContactInfo(counterpartyDto.PhoneNumber);
			ValidateCameFromProperty(counterpartyDto.CameFromId);
			//Валидация электронной почты
			
			switch(counterpartyDto.PersonType)
			{
				case PersonType.legal:
					ValidateLegalCounterpartyInfo(counterpartyDto);
					break;
				case PersonType.natural:
					ValidateNaturalCounterpartyInfo(counterpartyDto);
					break;
			}

			return _sb.ToString();
		}

		public string SendingCodeToEmailDtoValidate(SendingCodeToEmailDto codeToEmailDto)
		{
			// реализация будет позже, после слития интеграции юриков
			return string.Empty;
		}

		private void ValidateContactInfo(string counterpartyNumber)
		{
			var phoneNumber = _phoneFormatter.FormatString(counterpartyNumber);
			if(string.IsNullOrWhiteSpace(phoneNumber))
			{
				_sb.AppendLine("Передан неверный формат номера телефона");
			}
		}

		private void ValidateLegalCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			_sb.AppendLine("Регистрация юридических лиц не возможна");
			return;
			
			if(string.IsNullOrWhiteSpace(counterpartyDto.Name))
			{
				_sb.AppendLine("Не заполнено наименование клиента");
			}
			if(string.IsNullOrWhiteSpace(counterpartyDto.Inn))
			{
				_sb.AppendLine("Не заполнено ИНН");
			}
			if(string.IsNullOrWhiteSpace(counterpartyDto.Kpp))
			{
				_sb.AppendLine("Не заполнено КПП");
			}
			if(string.IsNullOrWhiteSpace(counterpartyDto.TypeOfOwnership))
			{
				_sb.AppendLine("Не заполнена форма собственности контрагента");
			}
			if(counterpartyDto.TaxType is null)
			{
				_sb.AppendLine("Не указано налогообложение клиента");
			}
			if(string.IsNullOrWhiteSpace(counterpartyDto.JurAddress))
			{
				_sb.AppendLine("Не заполнен юридический адрес");
			}
		}
		
		private void ValidateNaturalCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			if(string.IsNullOrWhiteSpace(counterpartyDto.FirstName))
			{
				_sb.AppendLine("Не заполнено имя клиента");
			}
			if(string.IsNullOrWhiteSpace(counterpartyDto.Surname))
			{
				_sb.AppendLine("Не заполнено фамилия клиента");
			}
		}

		private void ValidateCameFromProperty(int cameFromId)
		{
			if(cameFromId == default(int)
				|| (cameFromId != _counterpartySettings.GetMobileAppCounterpartyCameFromId
					&& cameFromId != _counterpartySettings.GetWebSiteCounterpartyCameFromId))
			{
				_sb.AppendLine(_wrongCameFromId);
			}
		}
	}
}
