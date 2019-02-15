using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Print;
using QS.Report;
using QSSupportLib;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Orders.Documents
{
	public class CoolerWarrantyDocument : OrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = String.Format("Гарантийный талон на кулера №{0}", WarrantyFullNumber),
				Identifier = "Documents.CoolerWarranty",
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "agreement_id",  AdditionalAgreement?.Id != null ? AdditionalAgreement.Id : -1 },
					{ "warranty_full_number", WarrantyFullNumber },
					{ "organization_id", int.Parse (MainSupport.BaseParameters.All [OrganizationRepository.CashlessOrganization])}
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override OrderDocumentType Type => OrderDocumentType.CoolerWarranty;

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override string Name => String.Format("Гарантийный талон на кулера №{0}", WarrantyFullNumber);

		int copiesToPrint = 1;
		public override int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}

		public override PrinterType PrintType => PrinterType.RDL;

		CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get => contract;
			set => SetField(ref contract, value, () => Contract);
		}

		AdditionalAgreement additionalAgreement;

		[Display(Name = "Доп. соглашение")]
		public virtual AdditionalAgreement AdditionalAgreement {
			get => additionalAgreement;
			set => SetField(ref additionalAgreement, value, () => AdditionalAgreement);
		}

		int warrantyNumber;

		[Display(Name = "Номер гарантийного талона")]
		public virtual int WarrantyNumber {
			get => warrantyNumber;
			set => SetField(ref warrantyNumber, value, () => WarrantyNumber);
		}

		public virtual string WarrantyFullNumber {
			get {
				if(contract == null)
					return "";
				if(additionalAgreement == null)
					return String.Format("{0}/Г-{1}", contract.Id, warrantyNumber);
				return String.Format("{0}/{1}-{2}/Г-{3}", contract.Id, AdditionalAgreement.GetTypePrefix(additionalAgreement.Type), additionalAgreement.AgreementNumber, warrantyNumber);
			}
		}

		/// <summary>
		/// Возвращает уникальный номер для нового гарантийного талона в пределах договора и доп. соглашения, 
		/// или только договора (продажа, если нет доп. соглашения)
		/// </summary>
		/// <returns>The number.</returns>
		/// <param name="order">Заказ</param>
		/// <param name="agreement">Дополнительное соглашение</param>
		public static int GetNumber(Order order, AdditionalAgreement agreement = null)
		{
			//Вычисляем номер для нового гарантийного талона.
			var orderDocuments = order.OrderDocuments;
			var coolerWarrantyNumbers = orderDocuments.Where(x => x.Type == OrderDocumentType.CoolerWarranty)
													  .OfType<CoolerWarrantyDocument>()
													  .Where(x => x.AdditionalAgreement == agreement)
													  .Select(x => x.WarrantyNumber)
													  .ToList();


			coolerWarrantyNumbers.Sort();

			return coolerWarrantyNumbers.Any() ? coolerWarrantyNumbers.Last() + 1 : 1;
		}
	}
}

