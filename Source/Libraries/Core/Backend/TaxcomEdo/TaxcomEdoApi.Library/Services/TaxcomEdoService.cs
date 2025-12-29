using System;
using System.Security.Cryptography.X509Certificates;
using Core.Infrastructure;
using DateTimeHelpers;
using Edo.Contracts.Messages.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Documents.Events;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Factories.Format5_03;

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
		private readonly IEdoInformalOrderDocumentFactory _edoInformalOrderDocumentFactory;

		public TaxcomEdoService(
			ILogger<TaxcomEdoService> logger,
			IOptionsSnapshot<TaxcomEdoApiOptions> apiOptions,
			IOptionsSnapshot<WarrantOptions> warrantOptions,
			IEdoTaxcomDocumentsFactory5_03 edoTaxcomDocumentsFactory503,
			IEdoBillFactory edoBillFactory,
			IEdoInformalOrderDocumentFactory edoInformalOrderDocumentFactory,
			X509Certificate2 certificate
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTaxcomDocumentsFactory503 =
				edoTaxcomDocumentsFactory503 ?? throw new ArgumentNullException(nameof(edoTaxcomDocumentsFactory503));
			_edoBillFactory = edoBillFactory ?? throw new ArgumentNullException(nameof(edoBillFactory));
			_edoInformalOrderDocumentFactory = edoInformalOrderDocumentFactory ?? throw new ArgumentNullException(nameof(edoInformalOrderDocumentFactory));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			_apiOptions = (apiOptions ?? throw new ArgumentNullException(nameof(apiOptions))).Value;
			_warrantOptions = (warrantOptions ?? throw new ArgumentNullException(nameof(warrantOptions))).Value;
		}
		
		public TaxcomContainer CreateContainerWithUpd(InfoForCreatingEdoUpd infoForCreatingEdoUpd)
		{
			var edoAccountId = _apiOptions.EdxClientId;
			var organizationEdoId = infoForCreatingEdoUpd.OrderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.TaxcomEdoAccountId;

			if(edoAccountId != organizationEdoId)
			{
				_logger.LogError(
					"edxClientId {EdoAccountId} отличается от указанного в организации из договора заказа {OrganizationEdoId}",
					edoAccountId,
					organizationEdoId);
				
				throw new InvalidOperationException("Организация заказа отличается от указанной для отправки документов в конфиге");
			}
			
			var updXml = _edoTaxcomDocumentsFactory503.CreateUpdXml5_03(
				infoForCreatingEdoUpd,
				_warrantOptions,
				edoAccountId,
				_certificate.Subject);
			
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};

			var orderId = infoForCreatingEdoUpd.OrderInfoForEdo.Id;
			var upd = new UniversalInvoiceDocument();
			UniversalInvoiceConverter_5_03.Convert(upd, updXml);

			if(!upd.Validate(out var errors))
			{
				var errorsString = string.Join(", ", errors);
				_logger.LogError(
					"УПД {OrderId} не прошла валидацию\nОшибки: {ErrorsString}",
					orderId,
					errorsString);

				throw new InvalidOperationException($"УПД {orderId} не прошла валидацию, отправка не возможна");
				//подумать, что делаем в таких случаях
			}
			
			upd.ExternalIdentifier = infoForCreatingEdoUpd.MainDocumentId.ToString();
			container.Documents.Add(upd);
			upd.AddCertificateForSign(_certificate.Thumbprint);
			
			//На случай, если МЧД будет не готова, просто проставляем пустые строки в конфиге
			//чтобы отправка шла без прикрепления доверки
			if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber))
			{
				container.SetWarrantParameters(
					_warrantOptions.WarrantNumber,
					infoForCreatingEdoUpd.OrderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.Inn,
					_warrantOptions.RepresentativeInn,
					_warrantOptions.StartDate,
					_warrantOptions.EndDate);
			}

			return container;
		}
		
		public TaxcomContainer CreateContainerWithUpd(UniversalTransferDocumentInfo updInfo)
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
			
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};
			
			var upd = new UniversalInvoiceDocument();
			UniversalInvoiceConverter_5_03.Convert(upd, updXml);

			if(!upd.Validate(out var errors))
			{
				var errorsString = string.Join(", ", errors);
				_logger.LogError(
					"УПД {UpdNumber} {DocumentId} не прошла валидацию\nОшибки: {ErrorsString}",
					updInfo.StringNumber,
					updInfo.DocumentId,
					errorsString);

				throw new InvalidOperationException($"УПД {updInfo.StringNumber} {updInfo.DocumentId} не прошла валидацию, отправка не возможна");
				//подумать, что делаем в таких случаях
			}
			
			upd.ExternalIdentifier = updInfo.DocumentId.ToString();
			container.Documents.Add(upd);
			upd.AddCertificateForSign(_certificate.Thumbprint);
			
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

		public TaxcomContainer CreateContainerWithBill(InfoForCreatingEdoBill data)
		{
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};
				
			var document = _edoBillFactory.CreateBillDocument(data);

			container.Documents.Add(document);
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
		
		public TaxcomContainer CreateContainerWithBillWithoutShipment(InfoForCreatingBillWithoutShipmentEdo data)
		{
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};

			var document = _edoBillFactory.CreateBillWithoutShipment(data);

			container.Documents.Add(document);
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

		public TaxcomContainer CreateContainerWithInformalOrderDocument(InfoForCreatingEdoInformalOrderDocument data)
		{
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};

			var document = _edoInformalOrderDocumentFactory.CreateInformalOrderDocument(data);
			container.Documents.Add(document);
			document.AddCertificateForSign(_certificate.Thumbprint);
			if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber))
			{
				container.SetWarrantParameters(
					_warrantOptions.WarrantNumber,
					data.OrganizationInfoForEdo.Inn,
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
