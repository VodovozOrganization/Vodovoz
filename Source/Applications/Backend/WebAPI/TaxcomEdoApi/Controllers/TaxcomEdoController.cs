using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Taxcom.Client.Api;
using Taxcom.Client.Api.Converters;
using Taxcom.Client.Api.Entity;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using EconomicLifeParticipiantInfo = Taxcom.Client.Api.Entity.Knd1115131.EconomicLifeParticipiantInfo;
using Product = Taxcom.Client.Api.Entity.Knd1115131.Product;
using TaxcomContainer = Taxcom.Client.Api.Entity.TaxcomContainer;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TaxcomEdoController : ControllerBase
	{
		private TaxcomApi _api;
			
		private readonly ILogger<TaxcomEdoController> _logger;
		private readonly IUnitOfWork _uow;

		public TaxcomEdoController(
			ILogger<TaxcomEdoController> logger,
			IUnitOfWork uow)
		{
			_logger = logger;
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		[HttpGet]
		public void Get()
		{
			var certificates = CertificateLogic.GetAvailableCertificates();
			var cert = certificates.FirstOrDefault();
			var issuer = cert.Issuer;
			var friendlyName = cert.FriendlyName;
			var subject = cert.Subject;
		}

		private DateTime? _lastCheckContactsUpdates;
		
		[HttpGet]
		[Route("/UpdateContacts")]
		public void UpdateContacts()
		{
			if(_api is null)
			{
				CreateNewSessionForSovCode();
			}
			
			_api.Login();

			var list = _api.GetContactListUpdates(null, null);
			var contactUpdates = ContactListSerializer.DeserializeContactList(list);
			/*while(true)
			{
				Task.Delay(60000);
				ContactList contactUpdates;

				do
				{
					try
					{
						var response = api1.GetContactListUpdates(_lastCheckContactsUpdates ?? DateTime.Now, null);
						contactUpdates = ContactListSerializer.DeserializeContactList(response);
					}
					catch(TaxcomApiException e)
					{
						e.ApiErrorCode //Если ошибка связана с устареванием маркера перелогиниваемся и запускаем цикл по новой
						Console.WriteLine(e);
						throw;
					}

					foreach(var contact in contactUpdates.Contacts)
					{
						Counterparty counterparty;
						
						switch(contact.State.Code)
						{
							case ContactStateCode.Incoming:
								api1.AcceptContact(contact.EdxClientId); //Перепроверить какой надо Id
								
								counterparty = _uow.Session.QueryOver<Counterparty>()
									.Where(x => x.INN == contact.Inn && x.KPP == contact.Kpp)
									.SingleOrDefault();
								
								if(counterparty == null)
								{
									break;
								}

								counterparty.EdoOperator = _uow.Session.QueryOver<EdoOperator>()
									.Where(x => x.Code == contact.EdxClientId.Substring(0, 3))
									.SingleOrDefault();
								counterparty.PersonalAccountIdInEdo = contact.EdxClientId;
								counterparty.ConsentForEdoStatus = ConsentForEdoStatus.Agree;
								
								_uow.Save(counterparty);
								_uow.Commit();
								break;
							case ContactStateCode.Sent:
							case ContactStateCode.Accepted:
							case ContactStateCode.Rejected:
							case ContactStateCode.Error:
								counterparty = _uow.Session.QueryOver<Counterparty>()
									.Where(x => x.INN == contact.Inn && x.KPP == contact.Kpp)
									.SingleOrDefault();

								if(counterparty == null)
								{
									break;
								}

								if(counterparty.ConsentForEdoStatus != contact.State.Code)
								{
									counterparty.ConsentForEdoStatus = contact.State.Code;
									_uow.Save(counterparty);
									_uow.Commit();
								}

								break;
						}
					}

					var lastItem = contactUpdates.Contacts?.LastOrDefault();
					_lastCheckContactsUpdates = lastItem != null ? lastItem.State.Changed : DateTime.Now;

				} while(contactUpdates.Contacts != null && contactUpdates.Contacts.Length >= 100);
			}*/
		}

		[HttpGet]
		[Route("/DocFlow1")]
		public void DocFlow1()
		{
			if(_api is null)
			{
				CreateNewSessionForSovCode();
			}

			_api.Login();
			_api.AutoSendReceive();

			//var list = DocFlowConverter.DeserializeDocflow(_api.GetDocflowsList(), null);
			var list = _api.GetDocflowsUpdates(null, 637765920000000000, null, null, true, true);

			/*using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var edoContainer = uow.GetById<EdoContainer>(20);
				
				var container = new TaxcomContainer();
				container.ImportFromZip(edoContainer.Container);
				
				_api.Send(container);
			}*/
			//var containerRawData =  container.ExportToZip();
			//System.IO.File.WriteAllBytes(@"E:\test5.zip", containerRawData);
		}

		[HttpGet]
		[Route("/DocFlow2")]
		public void DocFlow2()
		{
			if(_api is null)
			{
				CreateNewSessionForFinSoft();
			}
			
			_api.Login();
			_api.AutoSendReceive(); //8a96fb41-2dce-472a-a065-eda65ddcf617

			var list = _api.GetDocflowsUpdates(null, null, DocFlowDirection.Ingoing, null, true, true);
		}
		
		[HttpGet]
		[Route("/Accept")]
		public void Accept()
		{
			if(_api is null)
			{
				CreateNewSessionForFinSoft();
			}
			
			_api.Login();
			_api.Accept("8a96fb41-2dce-472a-a065-eda65ddcf617");
			_api.AutoSendReceive();
		}
		

		[HttpGet]
		[Route("/SendFile")]
		public void SendFile()
		{
			if(_api is null)
			{
				CreateNewSessionForSovCode();
			}
			
			_api.Login();
			_api.AutoSendReceive();

			var rawContainer = System.IO.File.ReadAllBytes(@"E:\test4.zip");
			var container = new TaxcomContainer();
			
			container.ImportFromZip(rawContainer);
			
			_api.Send(container);
		}

		[HttpGet]
		[Route("/CreateUpd")]
		public void CreateUpd()
		{
			//var flag = true;

			
			/*var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			x509Store.Open(OpenFlags.ReadOnly);
			//var x509Certificates = x509Store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
			x509Store.Close();*/
			
			var certificates = CertificateLogic.GetAvailableCertificates();
			var cert = certificates.SingleOrDefault(x => x.SubjectName.Name != null && x.SubjectName.Name.Contains("SN=Шевалье"));
			
			//Шевалье, G=Ефрем Филимонович

			//1872898 заказ на юр лицо
			//1873000 заказ на ип
			//1871773 закза на ИП со спец грузополучателем
			
			var order = _uow.GetById<Order>(1871773);

			var container = new TaxcomContainer();
			container.SignMode = DocumentSignMode.UseSpecifiedCertificate;
			
			var updXml = new EdoUpdFactory(new ParticipantDocFlowConverter(), new UpdProductConverter()).CreateNewUpdXml(order);

			var upd = new UniversalInvoiceDocument();
			UniversalInvoiceConverter.Convert(upd, updXml);

			upd.Validate(out var errors);
			
			container.Documents.Add(upd);
			upd.AddCertificateForSign(certificates[1].Thumbprint);
			
			var containerRawData = container.ExportToZip();
			System.IO.File.WriteAllBytes(@"E:\test4.zip", containerRawData);

			//api1.Send(container);
			var f = true;
		}

		private void CreateNewSessionForSovCode()
		{
			_api = new Factory().CreateApi(
				"https://api-invoice.taxcom.ru/v1.3/",
				true,
				"TaxcomFiler_F9244126-2184-4E43-8F62-F5BB1BEE4418",
				System.IO.File.ReadAllBytes(@"E:\2AL-F892A660-6A3C-4380-86C2-3C4F0090C1C8-00000.cer"),
				"2AL-F892A660-6A3C-4380-86C2-3C4F0090C1C8-00000");
		}
		
		private void CreateNewSessionForFinSoft()
		{
			_api = new Factory().CreateApi(
				"https://api-invoice.taxcom.ru/v1.3/",
				true,
				"TaxcomFiler_F9244126-2184-4E43-8F62-F5BB1BEE4418",
				System.IO.File.ReadAllBytes(@"E:\test.cer"),
				"2AL-3978EE3E-C84E-49F7-A214-4D533028AAD9-00000");
		}
	}
}
