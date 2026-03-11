using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Converters;
using CustomerAppsApi.Library.Dto;
using Gamma.Utilities;
using Mailjet.Api.Abstractions;
using MassTransit;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories.Counterparties;
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
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly IPublishEndpoint _publishEndpoint;

		public SendingService(
			IUnitOfWork unitOfWork,
			IEmployeeSettings employeeSettings,
			IEmailSettings emailSettings,
			IExternalCounterpartyRepository externalCounterpartyRepository,
			ISourceConverter sourceConverter,
			IPublishEndpoint publishEndpoint)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_externalCounterpartyRepository =
				externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto, bool isDryRun = false)
		{
			var instanceId = Convert.ToInt32(_unitOfWork.Session
				.CreateSQLQuery("SELECT GET_CURRENT_DATABASE_ID()")
				.List<object>()
				.FirstOrDefault());

			//TODO сделать отдельный обработчик для проверок
			var counterparty = _unitOfWork.GetById<Counterparty>(codeToEmailDto.CounterpartyId);

			if(counterparty is null)
			{
				return Result.Failure(Vodovoz.Errors.Common.CustomerAppsApiClientErrors.UnknownCounterparty);
			}

			var externalCounterparty = _externalCounterpartyRepository.GetExternalCounterparty(
				_unitOfWork, codeToEmailDto.ExternalUserId, _sourceConverter.ConvertToCounterpartyFrom(codeToEmailDto.Source));
			
			if(externalCounterparty is null)
			{
				return Result.Failure(Vodovoz.Errors.Common.CustomerAppsApiClientErrors.UnknownUser);
			}
			
			Employee employee = null;
			
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
				case Source.AiBot:
					employee = _unitOfWork.GetById<Employee>(_employeeSettings.AiBotEmployee);
					break;
			}

			if(employee is null)
			{
				return Result.Failure(Vodovoz.Errors.Common.CustomerAppsApiClientErrors.UnsupportedSource);
			}
			
			CreateAuthorizationCodeEmail(
				employee,
				counterparty,
				codeToEmailDto.Source,
				externalCounterparty.Id,
				codeToEmailDto.EmailAddress,
				out var mailSubject,
				isDryRun);

			var sendEmailMessage = new AuthorizationCodesSendEmailMessage
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
					InstanceId = instanceId
				}
			};

			if(!isDryRun)
			{
				_publishEndpoint.Publish(sendEmailMessage);
			}

			return Result.Success();
		}

		private void CreateAuthorizationCodeEmail(Employee employee,
			Counterparty counterparty,
			Source source,
			int externalCounterpartyId,
			string emailAddress,
			out string mailSubject, bool isDryRun)
		{
			mailSubject = $"{_authorizationCodeFor} {source.GetAttribute<AppellativeAttribute>()?.Genitive}";
			
			var authorizationCodeEmail = AuthorizationCodeEmail.Create(
				employee,
				counterparty,
				externalCounterpartyId,
				mailSubject,
				emailAddress);

			if(!isDryRun)
			{
				_unitOfWork.Save(authorizationCodeEmail.StoredEmail);
				_unitOfWork.Save(authorizationCodeEmail);
				_unitOfWork.Commit();
			}
		}
	}
}
