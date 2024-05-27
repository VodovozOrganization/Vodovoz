using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using TaxcomEdoApi.Config;
using TaxcomEdoApi.Factories;
using TaxcomEdoApi.Services;
using Vodovoz.Core.Data.Documents;
using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Settings.Organizations;
using Order = Vodovoz.Core.Data.Orders.Order;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class TaxcomEdoController : ControllerBase
	{
		private readonly ILogger<TaxcomEdoController> _logger;
		private readonly TaxcomApi _taxcomApi;
		private readonly X509Certificate2 _certificate;
		private readonly DocumentFlowService _documentFlowService;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISettingsController _settingController;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly TaxcomEdoApiOptions _apiOptions;
		private readonly WarrantOptions _warrantOptions;
		private readonly IEdoUpdFactory _edoUpdFactory;
		private readonly IEdoBillFactory _edoBillFactory;
		
		public TaxcomEdoController(
			ILogger<TaxcomEdoController> logger,
			TaxcomApi taxcomApi,
			IOptions<TaxcomEdoApiOptions> apiOptions,
			IOptions<WarrantOptions> warrantOptions,
			ISettingsController settingController,
			IOrderRepository orderRepository,
			IOrganizationRepository organizationRepository,
			IOrganizationSettings organizationSettings,
			IEdoUpdFactory edoUpdFactory,
			IEdoBillFactory edoBillFactory,
			X509Certificate2 certificate,
			DocumentFlowService documentFlowService)
		{
			_logger = logger;
			_taxcomApi = taxcomApi ?? throw new ArgumentNullException(nameof(taxcomApi));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			_documentFlowService = documentFlowService ?? throw new ArgumentNullException(nameof(documentFlowService));
			_apiOptions = (apiOptions ?? throw new ArgumentNullException(nameof(apiOptions))).Value;
			_warrantOptions = (warrantOptions ?? throw new ArgumentNullException(nameof(warrantOptions))).Value;
		}

		[HttpPost]
		public void CreateAndSendUpd(InfoForCreatingUpd infoForCreatingUpd)
		{
			try
			{
				var edoAccountId = _apiOptions.EdxClientId;
				var organizationEdoId = infoForCreatingUpd.Order.Contract.Organization.TaxcomEdoAccountId;

				if(edoAccountId != organizationEdoId)
				{
					_logger.LogError(
						"edxClientId {EdoAccountId} отличается от указанного в организации из договора заказа {OrganizationEdoId}",
						edoAccountId,
						organizationEdoId);
					
					throw new InvalidOperationException("Организация заказа отличается от указанной для отправки документов в конфиге");
				}
				
				var updXml = _edoUpdFactory.CreateNewUpdXml(infoForCreatingUpd.Order, _warrantOptions, edoAccountId, _certificate.Subject);
				
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
						infoForCreatingUpd.Order.Id,
						errorsString);
					
					//подумать, что делаем в таких случаях
				}

				container.Documents.Add(upd);
				upd.AddCertificateForSign(_certificate.Thumbprint);
				
				//На случай, если МЧД будет не готова, просто проставляем пустые строки в конфиге
				//чтобы отправка шла без прикрепления доверки
				if(!string.IsNullOrWhiteSpace(_warrantOptions.WarrantNumber))
				{
					container.SetWarrantParameters(
						_warrantOptions.WarrantNumber,
						infoForCreatingUpd.Order.Contract.Organization.INN,
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
				
				_logger.LogInformation("Отправляем контейнер по заказу №{OrderId}", infoForCreatingUpd.Order.Id);
				_taxcomApi.Send(container);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка в процессе формирования УПД №{OrderId} и ее отправки", infoForCreatingUpd.Order.Id);
			}
		}
		
		[HttpPost]
		public void CreateAndSendBill(Order order, byte[] attachment)
		{
			/*var edoContainer = new EdoContainer
			{
				Type = Type.Bill,
				Created = DateTime.Now,
				Container = new byte[64],
				Order = order,
				Counterparty = order.Client,
				MainDocumentId = string.Empty,
				EdoDocFlowStatus = EdoDocFlowStatus.NotStarted
			};

			var action = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
				.Where(x => x.Order.Id == edoContainer.Order.Id)
				.FirstOrDefault();
			*/
			
			_documentFlowService.SendBill(order, attachment);
		}
		
		[HttpPost]
		public void CreateAndSendBillWithoutShipmentForDebt(OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachment)
		{
			_documentFlowService.SendBillWithoutShipment(orderWithoutShipmentInfo, attachment);
		}
		
		[HttpPost]
		public void CreateAndSendBillWithoutShipmentForPayment(OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachment)
		{
			_documentFlowService.SendBillWithoutShipment(orderWithoutShipmentInfo, attachment);
		}
		
		[HttpPost]
		public void CreateAndSendBillWithoutShipmentForAdvancePayment(OrderWithoutShipmentInfo orderWithoutShipmentInfo, byte[] attachment)
		{
			_documentFlowService.SendBillWithoutShipment(orderWithoutShipmentInfo, attachment);
		}
	}
}
