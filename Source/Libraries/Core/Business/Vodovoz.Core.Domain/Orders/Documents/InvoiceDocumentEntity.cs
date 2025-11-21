using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class InvoiceDocumentEntity : PrintableOrderDocumentEntity, IAdvertisable, ISignableDocument
	{
		private int _copiesToPrint = 1;
		private bool _withoutAdvertising;
		private bool _hideSignature = true;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Invoice;
		#endregion
		public override string Name => $"Накладная №{Order.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		/// <summary>
		/// Без рекламы
		/// </summary>
		[Display(Name = "Без рекламы")]
		public virtual bool WithoutAdvertising
		{
			get => _withoutAdvertising;
			set => SetField(ref _withoutAdvertising, value);
		}

		/// <summary>
		/// Без подписей и печати
		/// </summary>
		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature
		{
			get => _hideSignature;
			set => SetField(ref _hideSignature, value);
		}
	}
}
