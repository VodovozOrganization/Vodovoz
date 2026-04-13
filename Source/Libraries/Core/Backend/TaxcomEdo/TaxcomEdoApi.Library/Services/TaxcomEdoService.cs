using System;
using System.Security.Cryptography.X509Certificates;
using Core.Infrastructure;
using DateTimeHelpers;
using Edo.Contracts.Messages.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Documents.Events;
using TaxcomEdoApi.Library.Builders;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Factories.Format5_03;
using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class TaxcomEdoService : ITaxcomEdoService
	{
		private readonly ILogger<TaxcomEdoService> _logger;
		private readonly X509Certificate2 _certificate;
		private readonly TaxcomEdoApiOptions _apiOptions;
		private readonly WarrantOptions _warrantOptions;
		private readonly IEdoTaxcomDocumentsFactory5_03 _edoTaxcomDocumentsFactory503;
		private readonly IEdoBillFactory _edoBillFactory;
		
		public TaxcomEdoService(
			ILogger<TaxcomEdoService> logger,
			IOptionsSnapshot<TaxcomEdoApiOptions> apiOptions,
			IOptionsSnapshot<WarrantOptions> warrantOptions,
			IEdoTaxcomDocumentsFactory5_03 edoTaxcomDocumentsFactory503,
			IEdoBillFactory edoBillFactory,
			X509Certificate2 certificate
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTaxcomDocumentsFactory503 =
				edoTaxcomDocumentsFactory503 ?? throw new ArgumentNullException(nameof(edoTaxcomDocumentsFactory503));
			_edoBillFactory = edoBillFactory ?? throw new ArgumentNullException(nameof(edoBillFactory));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			_apiOptions = (apiOptions ?? throw new ArgumentNullException(nameof(apiOptions))).Value;
			_warrantOptions = (warrantOptions ?? throw new ArgumentNullException(nameof(warrantOptions))).Value;
		}
		
		public NewContainer CreateContainerWithUpd(UniversalTransferDocumentInfo updInfo)
		{
			var edoAccountId = _apiOptions.EdxClientId;
			var organizationEdoId = updInfo.Seller.Organization.EdoAccountId;

			if(edoAccountId != organizationEdoId)
			{
				_logger.LogError(
					"edxClientId {EdoAccountId} отличается от указанного в организации {OrganizationEdoId}",
					edoAccountId,
					organizationEdoId);
				
				throw new InvalidOperationException("Кабинет ЭДО организации отличается от указанной для отправки документов в конфиге");
			}
			
			var updXml = _edoTaxcomDocumentsFactory503.CreateUpdXml5_03(
				updInfo,
				_warrantOptions,
				edoAccountId,
				_certificate.Subject);

			var container = NewContainer.Create(SignMode.UseSpecifiedCertificate);
			var upd = UniversalInvoiceDocumentBuilder5_03
				.Create()
				.ExternalIdentifier(updInfo.DocumentId.ToString())
				.WrapperXml(updXml)
				.Sender(edoAccountId)
				.Recipient(updInfo.Customer.Organization.EdoAccountId)
				.AddCertificateForSign(_certificate.Thumbprint)
				.Build();
			
			container.AddDocument(upd);
			
			//На случай, если МЧД будет не готова, просто проставляем пустые строки в конфиге
			//чтобы отправка шла без прикрепления доверки
			if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber))
			{
				container.SetWarrantParameters(
					_warrantOptions.WarrantNumber,
					updInfo.Seller.Organization.Inn,
					_warrantOptions.RepresentativeInn,
					_warrantOptions.StartDate,
					_warrantOptions.EndDate);
			}

			return container;
		}

		public NewContainer CreateContainerWithBill(InfoForCreatingEdoBill data)
		{
			var container = NewContainer.Create(SignMode.UseSpecifiedCertificate);
			var document = _edoBillFactory.CreateBillDocument(data);

			container.AddDocument(document);
			document.AddCertificateForSign(_certificate.Thumbprint);

			if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber) && _apiOptions.SendWarrantWithBills)
			{
				container.SetWarrantParameters(
					_warrantOptions.WarrantNumber,
					data.OrderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.Inn,
					_warrantOptions.RepresentativeInn,
					_warrantOptions.StartDate,
					_warrantOptions.EndDate);
			}

			return container;
		}
		
		public NewContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data)
		{
			var container = NewContainer.Create(SignMode.UseSpecifiedCertificate);
			var document = _edoBillFactory.CreateBillWithoutShipment(data);

			container.AddDocument(document);
			document.AddCertificateForSign(_certificate.Thumbprint);

			if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber) && _apiOptions.SendWarrantWithBillsWithoutShipment)
			{
				container.SetWarrantParameters(
					_warrantOptions.WarrantNumber,
					data.OrderWithoutShipmentInfo.OrganizationInfoForEdo.Inn,
					_warrantOptions.RepresentativeInn,
					_warrantOptions.StartDate,
					_warrantOptions.EndDate);
			}

			return container;
		}

		public string GetSendCustomerInformationEvent(string docflowId, string organization, string updFormat)
		{
			if(string.IsNullOrWhiteSpace(organization))
			{
				throw new ArgumentNullException(nameof(organization));
			}
			
			var jobPosition = _warrantOptions.JobPosition;

			if(string.IsNullOrWhiteSpace(jobPosition))
			{
				throw new InvalidOperationException("В конфиге не найдена должность подписанта!");
			}

			switch(updFormat)
			{
				case "5.01":
					return CreateSendCustomerInformation5_01(docflowId, organization, jobPosition).ToXmlString();
				default:
					return CreateSendCustomerInformation5_03(docflowId, organization, jobPosition).ToXmlString();
			}
		}

		private SendCustomerInformationEvent CreateSendCustomerInformation5_01(string docflowId, string organization, string jobPosition)
		{
			return new SendCustomerInformationEvent
			{
				InternalId = docflowId,
				Signers = new[]
				{
					new TaxcomEdo.Contracts.Documents.Events.Signer
					{
						Item = new SignerCertificate
						{
							Thumbprint = _certificate.Thumbprint,
							SerialNumber = _certificate.SerialNumber
						}
					}
				},
				AdditionalData =  new []
				{
					new AdditionalParameter
					{
						Name = "Покупатель.НаименованиеЭкономическогоСубъектаСоставителя",
						Value = organization
					},
					new AdditionalParameter
					{
						Name = "СодержаниеФактаХозЖизни.СодержаниеОперации",
						Value = "Товары/Услуги получены, претензий нет"
					},
					new AdditionalParameter
					{
						Name = "Подписант.ОблПолн",
						Value = "3"
					},
					new AdditionalParameter
					{
						Name = "Подписант.Статус",
						Value = "5"
					},
					new AdditionalParameter
					{
						Name = "Подписант.Должн",
						Value = jobPosition
					},
					new AdditionalParameter
					{
						Name = "Подписант.ОснПолн",
						Value = "Должностные обязанности"
					}
				}
			};
		}
		
		private SendCustomerInformationEvent CreateSendCustomerInformation5_03(string docflowId, string organization, string jobPosition)
		{
			var document = new SendCustomerInformationEvent
			{
				InternalId = docflowId,
				Signers = new[]
				{
					new TaxcomEdo.Contracts.Documents.Events.Signer
					{
						Item = new SignerCertificate
						{
							Thumbprint = _certificate.Thumbprint,
							SerialNumber = _certificate.SerialNumber
						}
					}
				},
				AdditionalData =  new []
				{
					new AdditionalParameter
					{
						Name = "Покупатель.НаименованиеЭкономическогоСубъектаСоставителя",
						Value = organization
					},
					new AdditionalParameter
					{
						Name = "СодержаниеФактаХозЖизни.СодержаниеОперации",
						Value = "Товары/Услуги получены, претензий нет"
					},
					new AdditionalParameter
					{
						Name = "СодержаниеФактаХозЖизни.ДатаПринятия",
						Value = DateTime.Today.ToEdoShortDateString()
					},
					new AdditionalParameter
					{
						Name = "Подписант.СпосПодтПолном",
						Value = "1"
					},
					new AdditionalParameter
					{
						Name = "СодержаниеФактаХозЖизни.СведенияОПринятии.КодСодержанияОперации.КодИтога",
						Value = "1"
					}
				}
			};
			
			return document;
		}

		public SendOfferCancellationEvent CreateOfferCancellation(string docflowId, string comment)
		{
			var document = new SendOfferCancellationEvent
			{
				InternalId = docflowId,
				Signers = new[]
				{
					new TaxcomEdo.Contracts.Documents.Events.Signer
					{
						Item = new SignerCertificate
						{
							Thumbprint = _certificate.Thumbprint,
							SerialNumber = _certificate.SerialNumber
						}
					}
				},
				Comment = comment
			};

			return document;
		}

		public SendAcceptCancellationOfferEvent AcceptOfferCancellation(string docflowId)
		{
			var document = new SendAcceptCancellationOfferEvent
			{
				InternalId = docflowId,
				Signers = new[]
				{
					new TaxcomEdo.Contracts.Documents.Events.Signer
					{
						Item = new SignerCertificate
						{
							Thumbprint = _certificate.Thumbprint,
							SerialNumber = _certificate.SerialNumber
						}
					}
				}
			};

			return document;
		}

		public SendRejectCancellationOfferEvent RejectOfferCancellation(string docflowId, string comment)
		{
			var document = new SendRejectCancellationOfferEvent
			{
				InternalId = docflowId,
				Signers = new[]
				{
					new TaxcomEdo.Contracts.Documents.Events.Signer
					{
						Item = new SignerCertificate
						{
							Thumbprint = _certificate.Thumbprint,
							SerialNumber = _certificate.SerialNumber
						}
					}
				},
				Comment = comment
			};

			return document;
		}
	}
}
