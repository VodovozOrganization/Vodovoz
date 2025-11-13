using CustomerAppsApi.Library.Dto.Counterparties;
using QS.Utilities.Numeric;
using System;
using System.Text;
using Vodovoz.Core.Domain.Clients;
using System.Text.RegularExpressions;
using Vodovoz.Domain.Client;
using Vodovoz.Settings.Counterparty;

namespace CustomerAppsApi.Library.Validators
{
	public class CounterpartyModelValidator : ICounterpartyModelValidator
	{
		private const string _wrongCameFromId = "Неверно заполнено поле Откуда клиент";
		private const string _onlyNumbersPattern = @"^\d+$";
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

			return ValidationResult();
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

			return ValidationResult();
		}

		public string SendingCodeToEmailDtoValidate(SendingCodeToEmailDto codeToEmailDto)
		{
			// реализация будет позже, после слития интеграции юриков
			return string.Empty;
		}

		public string GetLegalCustomersByInnDtoValidate(GetLegalCustomersByInnDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateInn(dto.Inn);

			return ValidationResult();
		}
		
		public string CompanyWithActiveEmailRequestDataValidate(CompanyWithActiveEmailRequest dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateEmail(dto.Email);

			return ValidationResult();
		}

		private void ValidateSource(Source source)
		{
			var sourceValue = (int)source;
			if(sourceValue < (int)Source.MobileApp || sourceValue > (int)Source.KulerSaleWebSite)
			{
				_sb.AppendLine("Неизвестный источник запроса");
			}
		}

		public string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateName(dto.Name);
			ValidateTypeOfOwnership(dto.CodeTypeOfOwnership, dto.ShortTypeOfOwnership, dto.FullTypeOfOwnership);
			ValidateInn(dto.Inn, dto.ShortTypeOfOwnership);
			ValidateKpp(dto.Kpp, dto.ShortTypeOfOwnership);
			ValidateJurAddress(dto.JurAddress);
			ValidateTaxType(dto.TaxType);
			
			return ValidationResult();
		}

		public string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateNaturalCounterpartyId(dto.ErpNaturalCounterpartyId);
			ValidateLegalCounterpartyId(dto.ErpLegalCounterpartyId);
			
			return ValidationResult();
		}

		public string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateNaturalCounterpartyId(dto.ErpNaturalCounterpartyId);
			ValidateLegalCounterpartyId(dto.ErpLegalCounterpartyId);
			
			return ValidationResult();
		}

		public string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateNaturalCounterpartyId(dto.ErpNaturalCounterpartyId);
			ValidateLegalCounterpartyId(dto.ErpLegalCounterpartyId);
			ValidatePhoneId(dto.ErpPhoneId);
			
			return ValidationResult();
		}

		public string ConnectingNewPhoneToLegalCustomerValidate(ConnectingNewPhoneToLegalCustomerDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateNaturalCounterpartyId(dto.ErpNaturalCounterpartyId);
			ValidateLegalCounterpartyId(dto.ErpLegalCounterpartyId);
			ValidateContactInfo(dto.PhoneNumber);
			
			return ValidationResult();
		}

		public string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto)
		{
			_sb.Clear();

			ValidateSource(dto.Source);
			ValidateNaturalCounterpartyId(dto.ErpCounterpartyId);
			ValidateContactInfo(dto.PhoneNumber);
			
			return ValidationResult();
		}

		public string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto)
		{
			_sb.Clear();
			
			ValidateSource(dto.Source);
			ValidateEmail(dto.Email);
			ValidateLegalCounterpartyId(dto.ErpCounterpartyId);
			ValidatePassword(dto.Password);
			
			return ValidationResult();
		}

		private void ValidatePassword(string password)
		{
			if(string.IsNullOrWhiteSpace(password))
			{
				_sb.AppendLine("Не заполнен пароль");
			}
		}

		private void ValidateNaturalCounterpartyId(int naturalCounterpartyId)
		{
			if(naturalCounterpartyId <= 0)
			{
				_sb.AppendLine("Передан неверный Id физического лица");
			}
		}
		
		private void ValidateLegalCounterpartyId(int legalCounterpartyId)
		{
			if(legalCounterpartyId <= 0)
			{
				_sb.AppendLine("Передан неверный Id юридического лица");
			}
		}
		
		private void ValidatePhoneId(int phoneId)
		{
			if(phoneId <= 0)
			{
				_sb.AppendLine("Передан неверный Id телефона");
			}
		}

		private string ValidationResult()
		{
			return _sb.ToString().Trim('\r', '\n');
		}

		private void ValidateContactInfo(string counterpartyNumber)
		{
			if(string.IsNullOrWhiteSpace(counterpartyNumber))
			{
				_sb.AppendLine("Не заполнен номер телефона");
				return;
			}
			
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
			
			ValidateName(counterpartyDto.Name);
			ValidateTypeOfOwnership(counterpartyDto.ShortTypeOfOwnership);
			ValidateInn(counterpartyDto.Inn);
			ValidateKpp(counterpartyDto.Kpp, counterpartyDto.ShortTypeOfOwnership);
			ValidateTaxType(counterpartyDto.TaxType);
			ValidateJurAddress(counterpartyDto.JurAddress);
		}

		private void ValidateInn(string inn, string typeOfOwnership = null)
		{
			if(string.IsNullOrWhiteSpace(inn))
			{
				_sb.AppendLine("Не заполнено ИНН");
			}
			else
			{
				if(string.IsNullOrWhiteSpace(typeOfOwnership))
				{
					if(inn.Length != Counterparty.InnPrivateBusinessmanLength && inn.Length != Counterparty.InnOtherLegalPersonLength)
					{
						_sb.AppendLine($"ИНН должен содержать {Counterparty.InnOtherLegalPersonLength}" +
							$" или {Counterparty.InnPrivateBusinessmanLength} символов");
					}
				}
				else
				{
					if(typeOfOwnership == "ИП")
					{
						if(inn.Length != Counterparty.InnPrivateBusinessmanLength)
						{
							_sb.AppendLine($"ИНН должен содержать {Counterparty.InnPrivateBusinessmanLength} символов");
						}
					}
					else
					{
						if(inn.Length != Counterparty.InnOtherLegalPersonLength)
						{
							_sb.AppendLine($"ИНН должен содержать {Counterparty.InnOtherLegalPersonLength} символов");
						}
					}
				}
				
				CheckOnlyNumbers(inn, "ИНН");
			}
		}
		
		//TODO 5417: обговорить все условия
		private void ValidateKpp(string kpp, string typeOfOwnership)
		{
			if(string.IsNullOrWhiteSpace(typeOfOwnership))
			{
				ValidateCompanyKpp(kpp);
			}
			else
			{
				if(typeOfOwnership == "ИП" && kpp != null)
				{
					_sb.AppendLine("У ИП не заполняется КПП");
				}
				else
				{
					ValidateCompanyKpp(kpp);
				}
			}
		}

		private void ValidateCompanyKpp(string kpp)
		{
			if(string.IsNullOrWhiteSpace(kpp))
			{
				_sb.AppendLine("Не заполнено КПП");
			}
			else if(kpp.Length != Counterparty.KppLength)
			{
				_sb.AppendLine($"КПП должен содержать {Counterparty.KppLength} символов");
				CheckOnlyNumbers(kpp, "КПП");
			}
		}
		
		private void ValidateTypeOfOwnership(string shortTypeOfOwnership)
		{
			ValidateShortTypeOfOwnership(shortTypeOfOwnership);
		}

		private void ValidateTypeOfOwnership(string codeTypeOfOwnership, string shortTypeOfOwnership, string fullTypeOfOwnership)
		{
			ValidateShortTypeOfOwnership(shortTypeOfOwnership);
			
			if(string.IsNullOrWhiteSpace(fullTypeOfOwnership))
			{
				_sb.AppendLine("Не заполнено полное наименование ОПФ");
			}
			
			if(string.IsNullOrWhiteSpace(codeTypeOfOwnership))
			{
				_sb.AppendLine("Не заполнен код ОПФ");
			}
		}
		
		private void ValidateShortTypeOfOwnership(string shortTypeOfOwnership)
		{
			if(string.IsNullOrWhiteSpace(shortTypeOfOwnership))
			{
				_sb.AppendLine("Не заполнено краткое наименование ОПФ");
			}
		}
		
		private void ValidateName(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
			{
				_sb.AppendLine("Не заполнено наименование клиента");
			}
			else
			{
				if(name.Length > Counterparty.NameMaxSimbols)
				{
					_sb.AppendLine($"Наименование клиента не может превышать {Counterparty.NameMaxSimbols}");
				}
			}
		}
		
		private void ValidateJurAddress(string jurAddress)
		{
			if(string.IsNullOrWhiteSpace(jurAddress))
			{
				_sb.AppendLine("Не заполнен юридический адрес");
			}
		}
		
		private void ValidateTaxType(TaxType? taxType)
		{
			if(taxType is null)
			{
				_sb.AppendLine("Не указано налогообложение клиента");
			}
		}

		private void CheckOnlyNumbers(string value, string propertyName)
		{
			if(!Regex.IsMatch(value, _onlyNumbersPattern))
			{
				_sb.AppendLine($"{propertyName} должен содержать только цифры");
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
