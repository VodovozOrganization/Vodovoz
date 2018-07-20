using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using QSDocTemplates;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderContract : OrderDocument, ITemplatePrntDoc
	{
		#region implemented abstract members of OrderDocument
					
		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.Contract;
			}
		}
			
		#endregion

		CounterpartyContract contract;

		[Display (Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get { return contract; }
			set { SetField (ref contract, value, () => Contract); }
		}

		public override string Name {
			get { return String.Format ("Договор №{0}", contract.Number); }
		}

		public override string DocumentDate {
			get { return String.Format ("от {0}", Contract.IssueDate.ToShortDateString ()); }
		}

		public virtual int CopiesToPrint { get; set; }

		public virtual IDocTemplate GetTemplate()
		{
			return Contract.ContractTemplate;
		}
	}

}

