using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecureCodeSender.Contracts.Requests;
using SecureCodeSender.Contracts.Responses;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.SecureCodes;
using Vodovoz.Settings.SecureCodes;

namespace SecureCodeSenderApi.Services
{
	public class SecureCodeHandler : ISecureCodeHandler
	{
		private readonly ILogger<SecureCodeHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ISecureCodeSettings _secureCodeSettings;
		private readonly IGenericRepository<GeneratedSecureCode> _generatedSecureCodeRepository;
		private readonly IEmailSecureCodeSender _emailSecureCodeSender;

		public SecureCodeHandler(
			ILogger<SecureCodeHandler> logger,
			IUnitOfWork uow,
			ISecureCodeSettings secureCodeSettings,
			IGenericRepository<GeneratedSecureCode> generatedSecureCodeRepository,
			IEmailSecureCodeSender emailSecureCodeSender)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_secureCodeSettings = secureCodeSettings ?? throw new ArgumentNullException(nameof(secureCodeSettings));
			_generatedSecureCodeRepository =
				generatedSecureCodeRepository ?? throw new ArgumentNullException(nameof(generatedSecureCodeRepository));
			_emailSecureCodeSender = emailSecureCodeSender ?? throw new ArgumentNullException(nameof(emailSecureCodeSender));
		}
		
		public async Task<Result<(string Code, int TimeForNextCode)>> GenerateAndSendSecureCode(SendSecureCodeDto sendSecureCodeDto)
		{
			if(sendSecureCodeDto.Method == SendTo.Phone)
			{
				_logger.LogWarning("Нет реализации отправки на мобильный телефон");
				return Result.Failure<(string Code, int TimeForNextCode)>(
					new Error(nameof(SecureCodeHandler), "Нет реализации отправки на мобильный телефон"));
			}

			var resultCode = GenerateSecureCode(sendSecureCodeDto);
			var generatedSecureCode = CreateGeneratedSecureCode(resultCode.Code, sendSecureCodeDto);

			switch(sendSecureCodeDto.Method)
			{
				case SendTo.Email:
					var sentResult =
						await _emailSecureCodeSender.SendCodeToEmail(_uow, generatedSecureCode);

					if(!sentResult)
					{
						_logger.LogWarning("Не получилось отправить код на почту");
						UseCode(generatedSecureCode);
						return Result.Failure<(string Code, int TimeForNextCode)>(
							new Error(nameof(SecureCodeHandler), ResponseMessage.Error));
					}
					break;
			}
			
			return resultCode;
		}
		
		public (string Code, int TimeForNextCode) GenerateSecureCode(SendSecureCodeDto sendSecureCodeDto)
		{
			var code = new Random().Next(100000, 999999).ToString();
			
			//отлов действующих кодов, если будем глушить
			var notUsedCodes = _generatedSecureCodeRepository.Get(
				_uow,
				x => !x.IsUsed
					&& x.UserPhone == sendSecureCodeDto.UserPhone
					&& x.Target == sendSecureCodeDto.Target
					&& x.UserAgent == sendSecureCodeDto.UserAgent
					&& x.ExternalCounterpartyId == sendSecureCodeDto.ExternalCounterpartyId)
				.ToArray();

			foreach(var notUsedCode in notUsedCodes)
			{
				notUsedCode.IsUsed = true;
				_uow.Save(notUsedCode);
			}

			if(_uow.HasChanges)
			{
				_uow.Commit();
			}
			
			return (code, _secureCodeSettings.TimeForNextCodeSeconds);
		}
		
		public (int Response, string Message) CheckSecureCode(CheckSecureCodeDto checkSecureCodeDto)
		{
			var savedCodeData = _generatedSecureCodeRepository.Get(
				_uow,
				x => x.Code == checkSecureCodeDto.Code
					&& x.UserPhone == checkSecureCodeDto.UserPhone
					&& x.Target == checkSecureCodeDto.Target
					&& x.UserAgent == checkSecureCodeDto.UserAgent
					&& x.ExternalCounterpartyId == checkSecureCodeDto.ExternalCounterpartyId)
				.LastOrDefault();

			if(savedCodeData is null)
			{
				_logger.LogWarning("Код {Code} для {ExternalCounterpartyId} не прошел проверку {Message}",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId,
					CheckSecureCodeResponses.WrongCode.Message);
				
				return CheckSecureCodeResponses.WrongCode;
			}

			if(savedCodeData.IsUsed)
			{
				_logger.LogWarning("Код {Code} для {ExternalCounterpartyId} не прошел проверку {Message}",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId,
					CheckSecureCodeResponses.CodeHasExpired.Message);
				return CheckSecureCodeResponses.CodeHasExpired;
			}
			
			if((DateTime.Now - savedCodeData.Created).Seconds > _secureCodeSettings.CodeLifetimeSeconds)
			{
				UseCode(savedCodeData);
				_logger.LogWarning("Код {Code} для {ExternalCounterpartyId} уже истек",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId);
				return CheckSecureCodeResponses.CodeHasExpired;
			}
			
			UseCode(savedCodeData);
			_logger.LogInformation("Код {Code} для {ExternalCounterpartyId} прошел проверки",
				checkSecureCodeDto.Code,
				checkSecureCodeDto.ExternalCounterpartyId);
			return CheckSecureCodeResponses.Ok;
		}

		private GeneratedSecureCode CreateGeneratedSecureCode(string code, SendSecureCodeDto sendSecureCodeDto)
		{
			var generatedSecureCode = GeneratedSecureCode.Create(
				code,
				sendSecureCodeDto.Method,
				sendSecureCodeDto.Target,
				sendSecureCodeDto.UserPhone,
				sendSecureCodeDto.Source,
				sendSecureCodeDto.Ip,
				sendSecureCodeDto.UserAgent,
				sendSecureCodeDto.ErpCounterpartyId,
				sendSecureCodeDto.ExternalCounterpartyId);
			
			_uow.Save(generatedSecureCode);
			_uow.Commit();

			return generatedSecureCode;
		}
		
		private void UseCode(GeneratedSecureCode savedCodeData)
		{
			savedCodeData.IsUsed = true;
			_uow.Save(savedCodeData);
			_uow.Commit();
		}
	}
}
