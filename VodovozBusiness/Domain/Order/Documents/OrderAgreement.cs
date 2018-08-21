using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Print;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Repositories.Client;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderAgreement : OrderDocument, IPrintableOdtDocument, ITemplateOdtDocument
	{
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type {
			get {
				return OrderDocumentType.AdditionalAgreement;	
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

		public override DateTime? DocumentDate {
			get { return AdditionalAgreement?.IssueDate;}
		}

		public virtual void PrepareTemplate(IUnitOfWork uow)
		{
			if (AdditionalAgreement.DocumentTemplate == null)
				AdditionalAgreement.UpdateContractTemplate(uow);

			if(AdditionalAgreement.DocumentTemplate != null) {
				AdditionalAgreement.DocumentTemplate.DocParser.SetDocObject(AdditionalAgreement.Self);

				switch(AdditionalAgreement.Type) {
					case AgreementType.NonfreeRent:
						var nonFreeRentAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as NonFreeRentAgreementParser);
						nonFreeRentAgreementParser.AddTableNomenclatures((AdditionalAgreement.Self as NonfreeRentAgreement).PaidRentEquipments.ToList());
						nonFreeRentAgreementParser.AddTableEquipmentTypes((AdditionalAgreement.Self as NonfreeRentAgreement).PaidRentEquipments.ToList());
						break;
					case AgreementType.DailyRent:
						var dailyRentAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as DailyRentAgreementParser);
						dailyRentAgreementParser.AddTableNomenclatures((AdditionalAgreement.Self as DailyRentAgreement).Equipment.ToList());
						dailyRentAgreementParser.AddTableEquipmentTypes((AdditionalAgreement.Self as DailyRentAgreement).Equipment.ToList());
						break;
					case AgreementType.FreeRent:
						break;
					case AgreementType.WaterSales:
						var waterAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as WaterAgreementParser);
						waterAgreementParser.AddPricesTable(WaterPricesRepository.GetCompleteWaterPriceTable(uow));
						break;
					case AgreementType.EquipmentSales:
						var equipmentAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as EquipmentAgreementParser);
						equipmentAgreementParser.AddPricesTable((AdditionalAgreement.Self as SalesEquipmentAgreement).SalesEqipments.ToList());
					break;
					case AgreementType.Repair:
						break;
					default:
						break;
				}
			}
		}
		public virtual IDocTemplate GetTemplate() => AdditionalAgreement.DocumentTemplate;

		public override PrinterType PrintType => PrinterType.ODT;
	}
}

