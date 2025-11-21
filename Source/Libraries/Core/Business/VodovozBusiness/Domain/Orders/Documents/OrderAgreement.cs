using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Domain.Orders.Documents
{
	public class OrderAgreement : PrintableOrderDocument, IPrintableOdtDocument, ITemplateOdtDocument
	{
		private IWaterPricesRepository _waterPricesRepository => ScopeProvider.Scope.Resolve<IWaterPricesRepository>();
		
		#region implemented abstract members of OrderDocument

		public override OrderDocumentType Type => OrderDocumentType.AdditionalAgreement;

		#endregion

		AdditionalAgreement additionalAgreement;

		[Display(Name = "Доп. соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get => additionalAgreement;
			set => SetField(ref additionalAgreement, value, () => AdditionalAgreement);
		}

		public override string Name => String.Format("Доп. соглашение {0} №{1}", additionalAgreement.AgreementTypeTitle, additionalAgreement.FullNumberText);

		public override DateTime? DocumentDate => AdditionalAgreement?.IssueDate;

		public virtual void PrepareTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
		{
			if(AdditionalAgreement.DocumentTemplate == null)
			{
				AdditionalAgreement.UpdateContractTemplate(uow, docTemplateRepository);
			}

			if(AdditionalAgreement.DocumentTemplate != null) {
				AdditionalAgreement.DocumentTemplate.DocParser.SetDocObject(AdditionalAgreement.Self);

				switch(AdditionalAgreement.Type) {
					case AgreementType.NonfreeRent:
						var nonFreeRentAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as NonFreeRentAgreementParser);
						nonFreeRentAgreementParser.AddTableNomenclatures((AdditionalAgreement.Self as NonfreeRentAgreement).PaidRentEquipments.ToList());
						nonFreeRentAgreementParser.AddTableEquipmentKinds((AdditionalAgreement.Self as NonfreeRentAgreement).PaidRentEquipments.ToList());
						break;
					case AgreementType.DailyRent:
						var dailyRentAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as DailyRentAgreementParser);
						dailyRentAgreementParser.AddTableNomenclatures((AdditionalAgreement.Self as DailyRentAgreement).Equipment.ToList());
						dailyRentAgreementParser.AddTableEquipmentKinds((AdditionalAgreement.Self as DailyRentAgreement).Equipment.ToList());
						break;
					case AgreementType.FreeRent:
						break;
					case AgreementType.WaterSales:
						var waterAgreementParser = (AdditionalAgreement.DocumentTemplate.DocParser as WaterAgreementParser);
						waterAgreementParser.AddPricesTable(_waterPricesRepository.GetCompleteWaterPriceTable(uow));
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

		int copiesToPrint = 2;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
	}
}

