using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Factories;

namespace TaxcomEdoApi.Library.Services
{
	public class TaxcomEdoService : ITaxcomEdoService
	{
		private readonly ILogger<TaxcomEdoService> _logger;
		private readonly X509Certificate2 _certificate;
		private readonly TaxcomEdoApiOptions _apiOptions;
		private readonly WarrantOptions _warrantOptions;
		private readonly IEdoUpdFactory _edoUpdFactory;
		private readonly IEdoBillFactory _edoBillFactory;
		
		public TaxcomEdoService(
			ILogger<TaxcomEdoService> logger,
			IOptions<TaxcomEdoApiOptions> apiOptions,
			IOptions<WarrantOptions> warrantOptions,
			IEdoUpdFactory edoUpdFactory,
			IEdoBillFactory edoBillFactory,
			X509Certificate2 certificate
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoUpdFactory = edoUpdFactory ?? throw new ArgumentNullException(nameof(edoUpdFactory));
			_edoBillFactory = edoBillFactory ?? throw new ArgumentNullException(nameof(edoBillFactory));
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
			
			var updXml = _edoUpdFactory.CreateNewUpdXml(
				infoForCreatingEdoUpd,
				_warrantOptions,
				edoAccountId,
				_certificate.Subject);
			
			var container = new TaxcomContainer
			{
				SignMode = DocumentSignMode.UseSpecifiedCertificate
			};

			var upd = new UniversalInvoiceDocument();
			UniversalInvoiceConverter.Convert(upd, updXml);

			if(!upd.Validate(out var errors))
			{
				var errorsString = string.Join(", ", errors);
				_logger.LogError(
					"УПД {OrderId} не прошла валидацию\nОшибки: {ErrorsString}",
					infoForCreatingEdoUpd.OrderInfoForEdo.Id,
					errorsString);
				
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
					_warrantOptions.StartDate,
					_warrantOptions.EndDate);
			}
			
			//отсылаем сообщение обработчику, что контейнер создан
			//либо переносим создание контейнера в момент отправки сообщения об создании УПД для апи
			/*var edoContainer = new EdoContainer
			{
				Type = Type.Upd,
				Created = DateTime.Now,
				Container = new byte[64],
				Order = order,
				Counterparty = order.Client,
				MainDocumentId = $"{upd.FileIdentifier}.xml",
				EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
			};

			var actions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
				.Where(x => x.Order.Id == edoContainer.Order.Id)
				.FirstOrDefault();

			if(actions != null && actions.IsNeedToResendEdoUpd)
			{
				actions.IsNeedToResendEdoUpd = false;
				uow.Save(actions);
			}

			_logger.LogInformation("Сохраняем контейнер по заказу №{OrderId}", order.Id);
			uow.Save(edoContainer);
			uow.Commit();
			*/

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
				
			/*
			var containerRawData = container.ExportToZip();

			edoContainer.Container = containerRawData;
			edoContainer.MainDocumentId = document.ExternalIdentifier;
			edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

			_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по заказу №{OrderId}",
				edoContainer.Id,
				edoContainer.Order.Id);

			unitOfWork.Save(edoContainer);
			unitOfWork.Commit();
			*/

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
				
			/*
			var containerRawData = container.ExportToZip();

			edoContainer.Container = containerRawData;
			edoContainer.MainDocumentId = document.ExternalIdentifier;
			edoContainer.EdoDocFlowStatus = EdoDocFlowStatus.NotStarted;

			_logger.LogInformation("Сохраняем контейнер №{EdoContainerId} по заказу №{OrderId}",
				edoContainer.Id,
				edoContainer.Order.Id);

			unitOfWork.Save(edoContainer);
			unitOfWork.Commit();
			*/

			return container;
		}
	}
}
