using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "дополнительные соглашения",
		Nominative = "дополнительное соглашение",
		Accusative = "дополнительное соглашение",
		Genitive = "дополнительного соглашения"
	)]
	public class AdditionalAgreement : PropertyChangedBase, IDomainObject
	{
		/// <summary>
		/// Используется для возможности приведения общего типа к конкретному, если
		/// напрямую привести не удается. 
		/// AdditionalAgreement a = entity.self;
		/// (a as WaterSalesAgreement).IsFixedPrice
		/// где IsFixedPrice доступно только для WaterSalesAgreement
		/// </summary> 
		public virtual AdditionalAgreement Self => this;

		public virtual int Id { get; set; }

		int agreementNumber;

		[Display (Name = "Номер")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual int AgreementNumber {
			get => agreementNumber;
			set => SetField(ref agreementNumber, value, () => AgreementNumber);
		}

		DocTemplate agreeemntTemplate;

		[Display (Name = "Шаблон договора")]
		public virtual DocTemplate DocumentTemplate {
			get => agreeemntTemplate;
			protected set => SetField(ref agreeemntTemplate, value, () => DocumentTemplate);
		}

		byte[] changedTemplateFile;

		[Display (Name = "Измененное соглашение")]
		//[PropertyChangedAlso("FileSize")]
		public virtual byte[] ChangedTemplateFile {
			get => changedTemplateFile;
			set => SetField(ref changedTemplateFile, value, () => ChangedTemplateFile);
		}

		[Display (Name = "Тип доп. соглашения")]
		public virtual AgreementType Type {
			get {	 
				if (this is DailyRentAgreement)
					return AgreementType.DailyRent;
				if (this is NonfreeRentAgreement)
					return AgreementType.NonfreeRent;
				if (this is FreeRentAgreement)
					return AgreementType.FreeRent;
				if (this is WaterSalesAgreement)
					return AgreementType.WaterSales;
				if(this is SalesEquipmentAgreement)
					return AgreementType.EquipmentSales;
				return AgreementType.Repair;
			}		
		}

		[Display (Name = "Договор")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual CounterpartyContract Contract { get; set; }

		[Display (Name = "Дата подписания")]
		[HistoryDateOnly]
		public virtual DateTime IssueDate { get; set; }

		[Display (Name = "Дата начала")]
		[HistoryDateOnly]
		public virtual DateTime StartDate { get; set; }

		[Display (Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint { get; set; }

		[Display (Name = "Закрыто")]
		public virtual bool IsCancelled { get; set; }

		#region Вычисляемые
		
		public virtual string AgreementTypeTitle => Type.GetEnumTitle();

		public virtual string Title => $"Доп. соглашение №{FullNumberText} от {StartDate.ToShortDateString()}";

		[Display(Name = "Полный номер")]
		public virtual string FullNumberText => $"{Contract.Number}/{GetTypePrefix(Type)}{AgreementNumber}";

		#endregion

		/// <summary>
		/// Updates template for the additional agreement.
		/// </summary>
		/// <returns><c>true</c>, in case of successful update, <c>false</c> if template for the additional agreement was not found.</returns>
		/// <param name="uow">Unit of Work.</param>
		public virtual bool UpdateContractTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
		{
			if (Contract == null)
			{
				DocumentTemplate = null;
				ChangedTemplateFile = null;
			}
			else
			{
				var newTemplate = docTemplateRepository.GetTemplate(uow, GetTemplateType(Type), Contract.Organization, Contract.ContractType);
				if(newTemplate == null) {
					DocumentTemplate = null;
					ChangedTemplateFile = null;
					return false;
				}
				if (!DomainHelper.EqualDomainObjects(newTemplate, DocumentTemplate))
				{
					DocumentTemplate = newTemplate;
					ChangedTemplateFile = null;
					return true;
				}
			}
			return false;
		}
		#region Статические

		public static string GetTypePrefix(AgreementType type)
		{
			switch (type)
			{
				case AgreementType.DailyRent:
					return "АС";
				case AgreementType.NonfreeRent:
					return "АМ";
				case AgreementType.FreeRent:
					return "Б";
				case AgreementType.Repair:
					return "Т";
				case AgreementType.WaterSales:
					return "В";
				case AgreementType.EquipmentSales:
					return "П";
				default:
					throw new InvalidOperationException(string.Format("Тип {0} не поддерживается.", type));
			}
		}
			

		public static TemplateType GetTemplateType(AgreementType type)
		{
			switch (type)
			{
				case AgreementType.DailyRent:
					return TemplateType.AgShortRent;
				case AgreementType.NonfreeRent:
					return TemplateType.AgLongRent;
				case AgreementType.FreeRent:
					return TemplateType.AgFreeRent;
				case AgreementType.Repair:
					return TemplateType.AgRepair;
				case AgreementType.WaterSales:
					return TemplateType.AgWater;
				case AgreementType.EquipmentSales:
					return TemplateType.AgEquip;
				default:
					throw new InvalidOperationException(string.Format("Тип {0} не поддерживается.", type));
			}
		}

		#endregion
	}
}
