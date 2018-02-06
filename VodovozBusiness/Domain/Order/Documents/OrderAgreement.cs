using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using QSReport;
using QSDocTemplates;
using QSOrmProject;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderAgreement : OrderDocument, ITemplatePrntDoc, ITemplateOdtDocument
	{
		/// <summary>
		/// Тип документа используемый для маппинга для DiscriminatorValue
		/// а также для определния типа в нумераторе типов документов.
		/// Создано для определения этого значения только в одном месте.
		/// </summary>
		public static new string OrderDocumentTypeValue { get => "order_agreement"; }

		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentTypeValues[OrderDocumentTypeValue];	
			}
		}

		#endregion

		AdditionalAgreement additionalAgreement;

		[Display (Name = "Доп. соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get { return additionalAgreement; }
			set { SetField (ref additionalAgreement, value, () => AdditionalAgreement); }
		}			

		public override string Name {
			get {
				return String.Format ("Доп. соглашение {0} №{1}",
				                      additionalAgreement.AgreementTypeTitle,
				                      additionalAgreement.FullNumberText);
			}
		}

		public override string DocumentDate {
			get { return AdditionalAgreement.DocumentDate; }
		}

		public virtual void PrepareTemplate(IUnitOfWork uow)
		{
			if (AdditionalAgreement.AgreementTemplate == null && AdditionalAgreement.AgreementTemplate != null)
				AdditionalAgreement.UpdateContractTemplate(uow);

			if (AdditionalAgreement.AgreementTemplate != null)
				AdditionalAgreement.AgreementTemplate.DocParser.SetDocObject(AdditionalAgreement);
		}

		public virtual IDocTemplate GetTemplate()
		{
			return AdditionalAgreement.AgreementTemplate;
		}
	}
}

