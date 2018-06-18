using System;
using System.ComponentModel.DataAnnotations;
using QSDocTemplates;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderContract : OrderDocument, ITemplatePrntDoc, IPrintableDocument, ITemplateOdtDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Contract;
			}
		}

		#endregion

		CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get { return contract; }
			set { SetField(ref contract, value, () => Contract); }
		}

		public override string Name {
			get { return String.Format("Договор №{0}", contract.ContractFullNumber); }
		}

		public override DateTime? DocumentDate {
			get { return Contract?.IssueDate; }
		}

		public virtual void PrepareTemplate(IUnitOfWork uow)
		{
			if(Contract.DocumentTemplate == null)
				Contract.UpdateContractTemplate(uow);

			if(Contract.DocumentTemplate != null)
				Contract.DocumentTemplate.DocParser.SetDocObject(Contract);
		}

		public virtual IDocTemplate GetTemplate()
		{
			return Contract.DocumentTemplate;
		}
	}

}

