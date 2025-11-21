using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class OrderAgreementEntity : PrintableOrderDocumentEntity
	{
		private AdditionalAgreementEntity _additionalAgreement;
		private int _copiesToPrint = 2;

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type => OrderDocumentType.AdditionalAgreement;

		#endregion

		/// <summary>
		/// Дополнительное соглашение
		/// </summary>
		[Display(Name = "Доп. соглашение")]
		public virtual AdditionalAgreementEntity AdditionalAgreement
		{
			get => _additionalAgreement;
			set => SetField(ref _additionalAgreement, value, () => AdditionalAgreement);
		}

		public override string Name => $"Доп. соглашение {_additionalAgreement.AgreementTypeTitle} №{_additionalAgreement.FullNumberText}";

		public override DateTime? DocumentDate => AdditionalAgreement?.IssueDate;

		public override PrinterType PrintType => PrinterType.ODT;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
