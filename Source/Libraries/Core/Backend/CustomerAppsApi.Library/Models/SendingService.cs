using System;
using System.Collections.Generic;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Errors;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Employee;

namespace CustomerAppsApi.Library.Models
{
	public class SendingService : ISendingService
	{
		private const string _authorizationCodeFor = "Код авторизации для";
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IEmailSettings _emailSettings;
		//private readonly ISourceConverter _sourceConverter;
		//private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly IPublishEndpoint _publishEndpoint;

		public SendingService(
			IUnitOfWork unitOfWork,
			IEmployeeSettings employeeSettings,
			IEmailSettings emailSettings,
			//ISourceConverter sourceConverter,
			//IExternalCounterpartyRepository externalCounterpartyRepository,
			IPublishEndpoint publishEndpoint)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			//_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			//_externalCounterpartyRepository =
			//	externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto)
		{
			var createdStoredEmailResult = TryCreateStoredEmail(codeToEmailDto, out var mailSubject);

			if(createdStoredEmailResult.IsFailure)
			{
				return createdStoredEmailResult;
			}

			//var counterpartyFrom = _sourceConverter.ConvertToCounterpartyFrom(codeToEmailDto.Source);
			var counterparty = _unitOfWork.GetById<Counterparty>(codeToEmailDto.CounterpartyId);

			if(counterparty is null)
			{
				return Result.Failure(new Error("UnknownClient", "Неизвестный клиент"));
			}
			
			var sendEmailMessage = new SendEmailMessage()
			{
				From = new EmailContact
				{
					Name = _emailSettings.DocumentEmailSenderName,
					Email = _emailSettings.DocumentEmailSenderAddress
				},

				To = new List<EmailContact>
				{
					new EmailContact
					{
						Name = counterparty.FullName,
						Email = codeToEmailDto.EmailAddress
					}
				},

				Subject = mailSubject,

				TextPart = codeToEmailDto.Message,
				HTMLPart = codeToEmailDto.Message,
				Payload = new EmailPayload
				{
					Id = 0,
					Trackable = false,
					InstanceId = 0
				}
			};

			_publishEndpoint.Publish<SendEmailMessage>(sendEmailMessage);
			return Result.Success();
		}

		private Result TryCreateStoredEmail(SendingCodeToEmailDto codeToEmailDto, out string mailSubject)
		{
			Employee employee = null;
			mailSubject = string.Empty;
			
			switch(codeToEmailDto.Source)
			{
				case Source.MobileApp:
					employee = _unitOfWork.GetById<Employee>(_employeeSettings.MobileAppEmployee);
					break;
				case Source.VodovozWebSite:
					employee = _unitOfWork.GetById<Employee>(_employeeSettings.VodovozWebSiteEmployee);
					break;
				case Source.KulerSaleWebSite:
					employee = _unitOfWork.GetById<Employee>(_employeeSettings.KulerSaleWebSiteEmployee);
					break;
			}

			if(employee is null)
			{
				return Result.Failure(new Error("UnsupportedSource", "Неизвестный источник запроса"));
			}
			
			mailSubject = $"{_authorizationCodeFor} мобильного приложения Веселый водовоз";
			
			var storedEmail = new StoredEmail
			{
				State = StoredEmailStates.WaitingToSend,
				Author = employee,
				ManualSending = false,
				SendDate = DateTime.Now,
				StateChangeDate = DateTime.Now,
				Subject = mailSubject,
				RecipientAddress = codeToEmailDto.EmailAddress,
				Guid = Guid.NewGuid()
			};
			
			_unitOfWork.Save(storedEmail);
			_unitOfWork.Commit();
			
			return Result.Success();
		}
	}
}
