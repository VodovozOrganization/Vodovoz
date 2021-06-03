using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Dialogs
{
	public partial class AddExistingDocumentsDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private readonly Order _order;
		private readonly IUnitOfWork _uow;

		public AddExistingDocumentsDlg(IUnitOfWork uow, Order order)
		{
			this.Build();
			_uow = uow;
			_order = order;
			var client = order.Client;
			counterpartydocumentsview1.Config(uow, client, true);
			orderselectedview1.Config(uow, client);
			orderselectedview1.OrderActivated += Orderselectedview1_OrderActivated;
			TabName = "Добавление документов";
		}

		protected void OnButtonAddSelectedDocumentsClicked(object sender, EventArgs e)
		{
			var counterpartyDocuments = counterpartydocumentsview1.GetSelectedDocuments();
			var orderDocuments = orderselectedview1.GetSelectedDocuments();

			List<OrderDocument> resultList = new List<OrderDocument>();

			//Контракты
			var documentsContract = 
				_uow.Session.QueryOver<OrderContract>()
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

			//Документы заказа
			var documentsOrder = _uow.Session.QueryOver<OrderDocument>()
			   .WhereRestrictionOn(x => x.Id).IsIn(orderDocuments.Select(y => y.DocumentId).ToList())
			   .List();
			resultList.AddRange(documentsOrder);

			_order.AddAdditionalDocuments(resultList);

			this.OnCloseTab(false);
		}

		void Orderselectedview1_OrderActivated(object sender, int e)
		{
			var doc = _uow.GetById<OrderDocument>(e) as IPrintableRDLDocument;
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