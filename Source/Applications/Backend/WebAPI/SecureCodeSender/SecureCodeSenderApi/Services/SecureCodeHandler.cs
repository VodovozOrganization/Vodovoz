using System;
using System.Linq;
using System.Threading.Tasks;
using DateTimeHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureCodeSender.Contracts.Requests;
using SecureCodeSender.Contracts.Responses;
using QS.DomainModel.UoW;
using SecureCodeSenderApi.Configs;
using SecureCodeSenderApi.Errors;
using Sms.External.Interface;
using Telegram.Contracts.Requests;
using Telegram.Contracts.Response;
using Telegram.GatewayApi.Client;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.SecureCodes;
using Vodovoz.Infrastructure.WebApi.Caching.Redis.Services;
using Vodovoz.Settings.SecureCodes;

namespace SecureCodeSenderApi.Services
{
	public class SecureCodeHandler : ISecureCodeHandler
	{
		private readonly ILogger<SecureCodeHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly SenderOptions _senderOptions;
		private readonly ISecureCodeSettings _secureCodeSettings;
		private readonly IGenericRepository<GeneratedSecureCode> _generatedSecureCodeRepository;
		private readonly ITelegramGatewayApiClient _gatewayApiClient;
		private readonly IEmailSecureCodeSender _emailSecureCodeSender;
		private readonly ISmsSender _smsRuSendController;
		private readonly IGarnetCacheService _garnetCacheService;

		public SecureCodeHandler(
			ILogger<SecureCodeHandler> logger,
			IUnitOfWork uow,
			IOptionsSnapshot<SenderOptions> senderOptions,
			ISecureCodeSettings secureCodeSettings,
			IGenericRepository<GeneratedSecureCode> generatedSecureCodeRepository,
			ITelegramGatewayApiClient gatewayApiClient,
			IEmailSecureCodeSender emailSecureCodeSender,
			ISmsSender smsRuSendController,
			IGarnetCacheService garnetCacheService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_senderOptions = (senderOptions ?? throw new ArgumentNullException(nameof(senderOptions))).Value;
			_secureCodeSettings = secureCodeSettings ?? throw new ArgumentNullException(nameof(secureCodeSettings));
			_generatedSecureCodeRepository =
				generatedSecureCodeRepository ?? throw new ArgumentNullException(nameof(generatedSecureCodeRepository));
			_gatewayApiClient = gatewayApiClient ?? throw new ArgumentNullException(nameof(gatewayApiClient));
			_emailSecureCodeSender = emailSecureCodeSender ?? throw new ArgumentNullException(nameof(emailSecureCodeSender));
			_smsRuSendController = smsRuSendController ?? throw new ArgumentNullException(nameof(smsRuSendController));
			_garnetCacheService = garnetCacheService ?? throw new ArgumentNullException(nameof(garnetCacheService));
		}
		
		/// <inheritdoc/>
		public async Task<Result> GenerateAndSendSecureCode(SendSecureCodeDto sendSecureCodeDto)
		{
			switch(sendSecureCodeDto.Method)
			{
				case SendTo.Email:
					return await SendToEmail(sendSecureCodeDto);
				case SendTo.Telegram:
					return await SendToTelegram(sendSecureCodeDto);
				case SendTo.Phone:
					return GenerateAndSendBySms(sendSecureCodeDto);
			}

			return Result.Failure(
				new Error(nameof(SecureCodeHandler), ResponseMessage.Error));
		}
		
		/// <inheritdoc/>
		public async Task<(int Response, string Message)> CheckSecureCode(CheckSecureCodeDto checkSecureCodeDto)
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
				//Пробуем найти код, если он отправлялся и генерировался через Telegram
				savedCodeData = _generatedSecureCodeRepository.Get(
						_uow,
						x => x.UserPhone == checkSecureCodeDto.UserPhone
							&& x.Method == SendTo.Telegram
							&& x.Target == checkSecureCodeDto.Target
							&& x.UserAgent == checkSecureCodeDto.UserAgent
							&& x.ExternalCounterpartyId == checkSecureCodeDto.ExternalCounterpartyId)
					.LastOrDefault();

				if(savedCodeData != null)
				{
					if(!string.IsNullOrWhiteSpace(savedCodeData.TelegramRequestId))
					{
						var response = await _gatewayApiClient.CheckVerificationStatus(
							CheckVerificationStatusRequest.Create(savedCodeData.TelegramRequestId, checkSecureCodeDto.Code));

						var verificationStatus = response?.Result?.VerificationStatus;
						
						if(verificationStatus != null)
						{
							switch (verificationStatus.Status)
							{
								case VerificationStatusType.CodeValid:
									_logger.LogInformation("Код {Code} для {ExternalCounterpartyId} прошел проверки",
										checkSecureCodeDto.Code,
										checkSecureCodeDto.ExternalCounterpartyId);
									return CheckSecureCodeResponses.Ok;
								case VerificationStatusType.Expired:
									_logger.LogWarning("Код {Code} для {ExternalCounterpartyId} не прошел проверку {Message}",
										checkSecureCodeDto.Code,
										checkSecureCodeDto.ExternalCounterpartyId,
										CheckSecureCodeResponses.CodeHasExpired.Message);
									return CheckSecureCodeResponses.CodeHasExpired;
							}
						}
					}
				}
				
				_logger.LogWarning("Введенный код {Code} для {ExternalCounterpartyId} не прошел проверку {Message}",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId,
					CheckSecureCodeResponses.WrongCode.Message);
				
				return CheckSecureCodeResponses.WrongCode;
			}

			if(savedCodeData.IsUsed.HasValue && savedCodeData.IsUsed.Value)
			{
				_logger.LogWarning("Введенный код {Code} для {ExternalCounterpartyId} не прошел проверку {Message}",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId,
					CheckSecureCodeResponses.CodeHasExpired.Message);
				return CheckSecureCodeResponses.CodeHasExpired;
			}

			if((DateTime.Now - savedCodeData.Created).TotalSeconds > _secureCodeSettings.CodeLifetimeSeconds)
			{
				UseCode(savedCodeData);
				_logger.LogWarning("Введенный код {Code} для {ExternalCounterpartyId} уже истек",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId);
				return CheckSecureCodeResponses.CodeHasExpired;
			}
			
			UseCode(savedCodeData);
			_logger.LogInformation("Введенный код {Code} для {ExternalCounterpartyId} прошел проверки",
				checkSecureCodeDto.Code,
				checkSecureCodeDto.ExternalCounterpartyId);
			return CheckSecureCodeResponses.Ok;
		}

		private async Task<Result> SendToEmail(SendSecureCodeDto sendSecureCodeDto)
		{
			var code = GenerateSecureCode();
			UpdateNotUsedCodes(sendSecureCodeDto);
			var generatedSecureCodeEntity = CreateGeneratedSecureCode(code, sendSecureCodeDto);
					
			var sentResult =
				await _emailSecureCodeSender.SendCode(_uow, generatedSecureCodeEntity);

			if(!sentResult)
			{
				_logger.LogWarning("Не получилось отправить код на почту");
				UseCode(generatedSecureCodeEntity);
				return Result.Failure(
					new Error(nameof(SecureCodeHandler), ResponseMessage.Error));
			}
					
			return Result.Success();
		}
		
		private async Task<Result> SendToTelegram(SendSecureCodeDto sendSecureCodeDto)
		{
			var phoneNumber = "+" + sendSecureCodeDto.Target;
			var checkAttemptsResult = await CheckAttempts(sendSecureCodeDto, phoneNumber);
			
			if(checkAttemptsResult.IsFailure)
			{
				return checkAttemptsResult;
			}

			var checkSendToTelegram = (await CheckSendAbilityToTelegram(phoneNumber)).Value;

			if(checkSendToTelegram != null)
			{
				if(!checkSendToTelegram.Ok)
				{
					_logger.LogWarning(
						"Не удалось проверить доступность отправки в Телеграм по номеру {PhoneNumber} {ErrorMessage}",
						phoneNumber,
						checkSendToTelegram.Error);

					return Result.Failure(SecureCodeSenderErrors.TelegramCheckSendAbilityFailed());
				}

				return await SendVerificationMessage(sendSecureCodeDto, phoneNumber, checkSendToTelegram);
			}
					
			_logger.LogWarning("Не удалось проверить доступность отправки в Телеграм по номеру {PhoneNumber}", phoneNumber);
			return Result.Failure(SecureCodeSenderErrors.TelegramCheckSendAbilityFailed(true));
		}

		private async Task<Result> CheckAttempts(
			SendSecureCodeDto sendSecureCodeDto,
			string phoneNumber)
		{
			var cachedValue = await _garnetCacheService.GetStringAsync(sendSecureCodeDto.Target);

			if(!string.IsNullOrWhiteSpace(cachedValue))
			{
				var intValue = int.Parse(cachedValue);

				if(intValue > _senderOptions.SendToTelegramAttemptsCountLimit)
				{
					_logger.LogWarning("Исчерпан лимит отправок в Телеграм по номеру {PhoneNumber}", phoneNumber);
					return Result.Failure(
						SecureCodeSenderErrors.CodeMaxSentAttemptsExceeded("Телеграм"));
				}

				await _garnetCacheService.SetStringAsync(sendSecureCodeDto.Target, (intValue + 1).ToString());
			}
			else
			{
				await _garnetCacheService.SetStringAsync(sendSecureCodeDto.Target, "1", DateTime.Today.LatestDayTime() - DateTime.Now);
			}

			return Result.Success();
		}
		
		private async Task<Result<ResponseDto>> CheckSendAbilityToTelegram(string phoneNumber)
		{
			ResponseDto checkSendToTelegram = null;
			
			try
			{
				checkSendToTelegram =
					await _gatewayApiClient.CheckSendAbility(CheckSendAbilityRequest.Create(phoneNumber));
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось проверить доступность отправки в Телеграм по номеру {PhoneNumber}",
					phoneNumber);

				return Result.Failure<ResponseDto>(SecureCodeSenderErrors.TelegramCheckSendAbilityFailed(true));
			}

			return Result.Success(checkSendToTelegram);
		}
		
		private async Task<Result> SendVerificationMessage(
			SendSecureCodeDto sendSecureCodeDto,
			string phoneNumber,
			ResponseDto checkSendToTelegram)
		{
			ResponseDto sendResponse;
			
			try
			{
				sendResponse = await _gatewayApiClient.SendVerificationMessage(
					SendVerificationMessageRequest.Create(
						phoneNumber,
						checkSendToTelegram.Result.RequestId,
						codeLength: _senderOptions.CodeLength,
						ttl: _secureCodeSettings.CodeLifetimeSeconds));
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось отправить код авторизации в Телеграм по номеру {PhoneNumber}",
					phoneNumber);

				return Result.Failure(SecureCodeSenderErrors.SentFailed());
			}
						
			CreateGeneratedSecureCode(null, sendSecureCodeDto, sendResponse.Result.RequestId);
			return Result.Success();
		}

		private string GenerateSecureCode()
		{
			var code = new Random().Next(100000, 999999).ToString();
			return code;
		}

		private void UpdateNotUsedCodes(SendSecureCodeDto sendSecureCodeDto)
		{
			//отлов действующих кодов
			var notUsedCodes = _generatedSecureCodeRepository.Get(
					_uow,
					x => x.IsUsed != null
						&& !x.IsUsed.Value
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
		}

		private Result GenerateAndSendBySms(SendSecureCodeDto sendSecureCodeDto)
		{
			var code = GenerateSecureCode();
			UpdateNotUsedCodes(sendSecureCodeDto);
			var generatedCodeEntity = CreateGeneratedSecureCode(code, sendSecureCodeDto);
			var phoneNumber = "+" + sendSecureCodeDto.Target;

			try
			{
				var sendStatus = _smsRuSendController.SendSms(
					SmsMessage.Create(
						phoneNumber,
						generatedCodeEntity.Id.ToString(),
						$"{code} - код для входа на сайт vodovoz-spb.ru"));

				if(!sendStatus.IsSuccefullStatus())
				{
					_logger.LogWarning("Не удалось отправить смс. Причина: {SendSmsWarning}", sendStatus);
					UseCode(generatedCodeEntity);
					return Result.Failure(new Error(nameof(SecureCodeHandler), ResponseMessage.Error));
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке смс на {Phone}", phoneNumber);
				UseCode(generatedCodeEntity);
				return Result.Failure(new Error(nameof(SecureCodeHandler), ResponseMessage.Error));
			}

			return Result.Success();
		}

		private GeneratedSecureCode CreateGeneratedSecureCode(string code, SendSecureCodeDto sendSecureCodeDto, string telegramRequestId = null)
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
				sendSecureCodeDto.ExternalCounterpartyId,
				telegramRequestId);
			
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
