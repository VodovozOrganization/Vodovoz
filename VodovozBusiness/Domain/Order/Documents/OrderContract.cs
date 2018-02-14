using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using QSDocTemplates;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderContract : OrderDocument, ITemplatePrntDoc
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "order_contract"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];
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

		public override DateTime? DocumentDate {
			get { return Contract?.IssueDate; }
		}
			
		public virtual IDocTemplate GetTemplate()
		{
			return Contract.ContractTemplate;
		}
	}

}

