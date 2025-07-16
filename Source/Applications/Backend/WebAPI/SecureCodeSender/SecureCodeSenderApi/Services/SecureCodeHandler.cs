using System;
using System.Linq;
using System.Threading.Tasks;
using SecureCodeSender.Contracts.Requests;
using SecureCodeSender.Contracts.Responses;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.SecureCodes;
using Vodovoz.Settings.SecureCodes;

namespace SecureCodeSenderApi.Services
{
	public class SecureCodeHandler : ISecureCodeHandler
	{
		private readonly IUnitOfWork _uow;
		private readonly ISecureCodeSettings _secureCodeSettings;
		private readonly IGenericRepository<GeneratedSecureCode> _generatedSecureCodeRepository;
		private readonly IEmailSecureCodeSender _emailSecureCodeSender;

		public SecureCodeHandler(
			IUnitOfWork uow,
			ISecureCodeSettings secureCodeSettings,
			IGenericRepository<GeneratedSecureCode> generatedSecureCodeRepository,
			IEmailSecureCodeSender emailSecureCodeSender)
		{
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
				return Result.Failure<(string Code, int TimeForNextCode)>(
					new Error(nameof(SecureCodeHandler), "Нет реализации отправки на мобильный телефон"));
			}

			var resultCode = GenerateSecureCode(sendSecureCodeDto);

			switch(sendSecureCodeDto.Method)
			{
				case SendTo.Email:
					var sentResult =
						await _emailSecureCodeSender.SendCodeToEmail(_uow, CreateGeneratedSecureCode(resultCode.Code, sendSecureCodeDto));

					if(!sentResult)
					{
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
			var savedCodeData = _generatedSecureCodeRepository.GetLastOrDefault(
				_uow,
				x => x.Code == checkSecureCodeDto.Code
					&& x.UserPhone == checkSecureCodeDto.UserPhone
					&& x.Target == checkSecureCodeDto.Target
					&& x.UserAgent == checkSecureCodeDto.UserAgent
					&& x.ExternalCounterpartyId == checkSecureCodeDto.ExternalCounterpartyId);

			if(savedCodeData is null)
			{
				return CheckSecureCodeResponses.WrongCode;
			}

			if(savedCodeData.IsUsed)
			{
				return CheckSecureCodeResponses.CodeHasExpired;
			}
			
			if((DateTime.Now - savedCodeData.Created).Seconds > _secureCodeSettings.CodeLifetimeSeconds)
			{
				savedCodeData.IsUsed = true;
				_uow.Save(savedCodeData);
				_uow.Commit();
				
				return CheckSecureCodeResponses.CodeHasExpired;
			}
			
			return CheckSecureCodeResponses.Ok;
		}

		private GeneratedSecureCode CreateGeneratedSecureCode(string code, SendSecureCodeDto sendSecureCodeDto)
		{
			var generatedSecureCode = GeneratedSecureCode.Create(
				code,
				sendSecureCodeDto.Method,
				sendSecureCodeDto.Target,
				sendSecureCodeDto.UserPhone,
				Enum.Parse<Source>(sendSecureCodeDto.Source.ToString()),
				sendSecureCodeDto.Ip,
				sendSecureCodeDto.UserAgent,
				sendSecureCodeDto.ErpCounterpartyId,
				sendSecureCodeDto.ExternalCounterpartyId);
			
			_uow.Save(generatedSecureCode);
			_uow.Commit();

			return generatedSecureCode;
		}
	}
}
