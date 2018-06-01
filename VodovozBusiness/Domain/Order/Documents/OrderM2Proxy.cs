using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using QSReport;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderM2Proxy : OrderDocument, ITemplatePrntDoc, ITemplateOdtDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.M2Proxy;
			}
		}

		#endregion

		M2ProxyDocument m2Proxy;

		[Display(Name = "Доверенность М-2")]
		public virtual M2ProxyDocument M2Proxy {
			get { return m2Proxy; }
			set { SetField(ref m2Proxy, value, () => M2Proxy); }
		}

		public override string Name {
			get {
				return String.Format("Доверенность М-2 №{0}", M2Proxy.Id);
			}
		}

		public override DateTime? DocumentDate {
			get { return M2Proxy?.Date; }
		}

		public virtual void PrepareTemplate(IUnitOfWork uow)
		{
			//if(M2Proxy.ProxyDocumentTemplate == null)
				//M2Proxy.UpdateContractTemplate(uow);

			if(M2Proxy.ProxyDocumentTemplate != null)
				M2Proxy.ProxyDocumentTemplate.DocParser.SetDocObject(M2Proxy);
		}

		public virtual IDocTemplate GetTemplate()
		{
			return M2Proxy.ProxyDocumentTemplate;
		}

	}
}
