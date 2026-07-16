using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class SpecialBillDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 1;
		private bool _hideSignature = true;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.SpecialBill;
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Особый счет №{DocumentOrganizationCounter?.DocumentNumber ?? Order?.Id.ToString()}"
			:  $"Особый счет №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.BillDate;

		public virtual CounterpartyEntity Counterparty => Order?.Client;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		#region Свои свойства

		/// <summary>
		/// Без подписей и печати
		/// </summary>
		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature
		{
			get => _hideSignature;
			set => SetField(ref _hideSignature, value);
		}

		#endregion
	}
}
