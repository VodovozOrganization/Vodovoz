using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using QS.Print;
using QSDocTemplates;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderContract : OrderDocument, IPrintableOdtDocument, ITemplateOdtDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type => OrderDocumentType.Contract;

		#endregion

		CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get => contract;
			set => SetField(ref contract, value, () => Contract);
		}

		public override string Name => String.Format("Договор №{0}", contract.ContractFullNumber);

		public override DateTime? DocumentDate => Contract?.IssueDate;

		public virtual void PrepareTemplate(IUnitOfWork uow)
		{
			if(Contract.DocumentTemplate == null)
				Contract.UpdateContractTemplate(uow);

			if(Contract.DocumentTemplate != null)
				Contract.DocumentTemplate.DocParser.SetDocObject(Contract);
		}

		public virtual IDocTemplate GetTemplate() => Contract.DocumentTemplate;

		public override PrinterType PrintType => PrinterType.ODT;

		int copiesToPrint = 2;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
	}
}

