using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SecureCodeSender.Contracts.Requests;
using SecureCodeSenderApi.Services.Validators;

namespace SecureCodeSenderApi.Services
{
	public class SecureCodeServiceValidator : ISecureCodeServiceValidator
	{
		private readonly ILogger<SecureCodeServiceValidator> _logger;
		private readonly IEmailMethodValidator _emailMethodValidator;
		private readonly IUserPhoneValidator _userPhoneValidator;
		private readonly IIpValidator _ipValidator;
		private readonly ISecureCodeValidator _secureCodeValidator;

		public SecureCodeServiceValidator(
			ILogger<SecureCodeServiceValidator> logger,
			IEmailMethodValidator emailMethodValidator,
			IUserPhoneValidator userPhoneValidator,
			IIpValidator ipValidator,
			ISecureCodeValidator secureCodeValidator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_emailMethodValidator = emailMethodValidator ?? throw new ArgumentNullException(nameof(emailMethodValidator));
			_userPhoneValidator = userPhoneValidator ?? throw new ArgumentNullException(nameof(userPhoneValidator));
			_ipValidator = ipValidator ?? throw new ArgumentNullException(nameof(ipValidator));
			_secureCodeValidator = secureCodeValidator ?? throw new ArgumentNullException(nameof(secureCodeValidator));
		}
		
		public string Validate(object data)
		{
			var sb = new StringBuilder();
			
			var validationResults = data switch
			{
				SendSecureCodeDto sendCodeDto => Validate(sendCodeDto),
				CheckSecureCodeDto checkCodeDto => Validate(checkCodeDto),
				_ => Enumerable.Empty<ValidationResult>()
			};

			foreach(var validationResult in validationResults)
			{
				sb.AppendLine(validationResult.ErrorMessage);
			}

			if(sb.Length <= 0)
			{
				return null;
			}

			_logger.LogWarning("Не прошли валидацию: {ValidationResult}", sb.ToString());
			return sb.ToString().TrimEnd('\r', '\n');
		}
		
		private IEnumerable<ValidationResult> Validate(SendSecureCodeDto sendCodeDto)
		{
			foreach (var validationResult in _emailMethodValidator.Validate(sendCodeDto.Target, sendCodeDto.Method))
			{
				yield return validationResult;
			}

			foreach (var validationResult in _userPhoneValidator.Validate(sendCodeDto.UserPhone))
			{
				yield return validationResult;
			}

			foreach (var validationResult in _ipValidator.Validate(sendCodeDto.Ip))
			{
				yield return validationResult;
			}
		}
		
		private IEnumerable<ValidationResult> Validate(CheckSecureCodeDto checkCodeDto)
		{
			foreach (var validationResult in _secureCodeValidator.Validate(checkCodeDto.Code))
			{
				yield return validationResult;
			}
			
			foreach (var validationResult in _emailMethodValidator.Validate(checkCodeDto.Target))
			{
				yield return validationResult;
			}

			foreach (var validationResult in _userPhoneValidator.Validate(checkCodeDto.UserPhone))
			{
				yield return validationResult;
			}

			foreach (var validationResult in _ipValidator.Validate(checkCodeDto.Ip))
			{
				yield return validationResult;
			}
		}
	}
}
