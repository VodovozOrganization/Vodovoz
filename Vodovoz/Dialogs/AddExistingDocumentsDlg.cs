using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSReport;
using QSTDI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Dialogs
{
	public partial class AddExistingDocumentsDlg : TdiTabBase
	{
		public IUnitOfWorkGeneric<Order> UoW { get; private set; }

		public AddExistingDocumentsDlg(IUnitOfWorkGeneric<Order> uow, Counterparty client)
		{
			this.Build();
			UoW = uow;
			counterpartydocumentsview1.Config(uow, client, true);
			orderselectedview1.Config(uow, client);
			orderselectedview1.OrderActivated += Orderselectedview1_OrderActivated;
			TabName = "Добавление документов";
		}

		protected void OnButtonAddSelectedDocumentsClicked(object sender, EventArgs e)
		{
			Order currentOrder = UoW.Root;
			var counterpartyDocuments = counterpartydocumentsview1.GetSelectedDocuments();
			var orderDocuments = orderselectedview1.GetSelectedDocuments();

			List<OrderDocument> resultList = new List<OrderDocument>();

			//Контракты
			var documentsContract = 
				UoW.Session.QueryOver<OrderContract>()
                   .WhereRestrictionOn(x => x.Contract.Id)
                   .IsIn(counterpartyDocuments
						.Select(y => y.Document)
						.OfType<CounterpartyContract>()
						.Select(x => x.Id)
						.ToList()
                        )
                   .List()
                   .Distinct();
			resultList.AddRange(documentsContract);

			//Доп. соглашения
			var documentsAgreement = 
				UoW.Session.QueryOver<OrderAgreement>()			                            
				   .WhereRestrictionOn(x => x.AdditionalAgreement.Id)
				   .IsIn(counterpartyDocuments
				         .Select(y => y.Document)
				         .OfType<AdditionalAgreement>()			
				         .Select(x => x.Id)						
				         .ToList()
						 )
				   .List()
				   .Distinct();
			resultList.AddRange(documentsAgreement);

			//Гарантийные талоны на кулера
			var documentsCoolerWarranty = 
				UoW.Session.QueryOver<CoolerWarrantyDocument>()
				   .WhereRestrictionOn(x => x.Id)
				   .IsIn(counterpartyDocuments
				         .Select(y => y.Document)						   
				         .OfType<CoolerWarrantyDocument>()					
				         .Select(x => x.Id)					 
				         .ToList()			 	 	
				        )					
				   .List()
				   .Distinct();
			resultList.AddRange(documentsCoolerWarranty);

			//Гарантийные талоны на помпы
			var documentsPumpWarranty = 
				UoW.Session.QueryOver<PumpWarrantyDocument>()
				   .WhereRestrictionOn(x => x.Id)
				   .IsIn(counterpartyDocuments							
				         .Select(y => y.Document)
				         .OfType<PumpWarrantyDocument>()							
				         .Select(x => x.Id)							
				         .ToList()									
				        )								
				   .List()			                               
				   .Distinct();
			resultList.AddRange(documentsPumpWarranty);

			//Документы заказа
			var documentsOrder = UoW.Session.QueryOver<OrderDocument>()
			   .WhereRestrictionOn(x => x.Id).IsIn(orderDocuments.Select(y => y.DocumentId).ToList())
			   .List();
			resultList.AddRange(documentsOrder);

			UoW.Root.AddAdditionalDocuments(resultList);

			this.OnCloseTab(false);
		}

		void Orderselectedview1_OrderActivated(object sender, int e)
		{
			var doc = UoW.GetById<OrderDocument>(e) as IPrintableRDLDocument;
			if(doc == null) {
				return;
			}
			TabParent.AddTab(DocumentPrinter.GetPreviewTab(doc), this, false);
		}

		private void OpenOrder()
		{
			
		}
	}
}