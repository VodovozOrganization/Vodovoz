using System;
using System.Text;
using CustomerAppsApi.Controllers;
using QS.Utilities.Numeric;
using Vodovoz.Domain.Client;
using Vodovoz.Parameters;

namespace CustomerAppsApi.Validators
{
	public class CounterpartyModelValidator
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
			ValidateContactInfo(counterpartyContactInfoDto.ExternalCounterpartyId, counterpartyContactInfoDto.PhoneNumber);

			return _sb.ToString();
		}
		
		public string CounterpartyDtoValidate(CounterpartyDto counterpartyDto)
		{
			_sb.Clear();

			ValidateContactInfo(counterpartyDto.ExternalCounterpartyId, counterpartyDto.PhoneNumber);
			ValidateCameFromProperty(counterpartyDto.CounterpartyFrom, counterpartyDto.CameFromId);
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

		private void ValidateContactInfo(int externalCounterpartyId, string counterpartyNumber)
		{
			if(externalCounterpartyId == 0)
			{
				_sb.AppendLine($"Нельзя идентифицировать пользователя {nameof(externalCounterpartyId)} = 0");
			}

			var phoneNumber = _phoneFormatter.FormatString(counterpartyNumber);
			if(string.IsNullOrWhiteSpace(phoneNumber))
			{
				_sb.AppendLine("Передан неверный формат номера телефона");
			}
		}

		private void ValidateLegalCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
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

		private void ValidateCameFromProperty(CounterpartyFrom counterpartyFrom, int cameFromId)
		{
			if(cameFromId == default(int))
			{
				_sb.AppendLine(_wrongCameFromId);
				return;
			}
			
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					if(cameFromId != _counterpartySettings.GetMobileAppCounterpartyCameFromId)
					{
						_sb.AppendLine(_wrongCameFromId);
					}
					break;
				case CounterpartyFrom.WebSite:
					if(cameFromId != _counterpartySettings.GetWebSiteCounterpartyCameFromId)
					{
						_sb.AppendLine(_wrongCameFromId);
					}
					break;
			}
		}
	}
}
