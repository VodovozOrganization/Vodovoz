using System;
using System.ComponentModel.DataAnnotations;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderContract : PrintableOrderDocument, IPrintableOdtDocument, ITemplateOdtDocument
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

		public override string Name {
			get {
				if(contract == null) {
					return "Нет договора";
				}
				return $"Договор №{contract.Number}";
			}
		}

		public override DateTime? DocumentDate => Contract?.IssueDate;

		public virtual void PrepareTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
		{
			if(Contract.DocumentTemplate == null)
			{
				Contract.UpdateContractTemplate(uow, docTemplateRepository);
			}

			Contract.DocumentTemplate?.DocParser.SetDocObject(Contract);
		}

		public virtual IDocTemplate GetTemplate() => Contract?.DocumentTemplate;

		public override PrinterType PrintType => PrinterType.ODT;

		int copiesToPrint = 2;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
	}
}

