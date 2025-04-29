﻿using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны документов",
		Nominative = "шаблон документа")]
	[EntityPermission]
	public class DocTemplate : PropertyChangedBase, IDomainObject, IDocTemplate
	{
		#region Свойства
		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[StringLength(45)]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		TemplateType templateType;

		[Display (Name = "Тип шаблона")]
		public virtual TemplateType TemplateType {
			get { return templateType; }
			set { 
				bool needUpdateName = String.IsNullOrWhiteSpace(Name) || Name == templateType.GetEnumTitle();
				if (SetField(ref templateType, value, () => TemplateType))
				{
					docParser = null;
				if(needUpdateName)
					Name = templateType.GetEnumTitle();
				}					
			}
		}

		Organization organization;

		[Display (Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		ContractType contractType;

		[Display(Name = "Тип договора")]
		public virtual ContractType ContractType {
			get { return contractType; }
			set { SetField(ref contractType, value, () => ContractType); }
		}

		byte[] templateFile;

		[Display (Name = "Файл шаблона")]
		[PropertyChangedAlso("FileSize")]
		[Required]
		public virtual byte[] TempalteFile {
			get { return templateFile; }
			set { SetField (ref templateFile, value, () => TempalteFile); }
		}
			
		#endregion

		#region Вычисляемые

		public virtual long FileSize{
			get{
				return TempalteFile != null ? TempalteFile.LongLength : 0;
			}
		}

		public virtual byte[] File{
			get{
				return ChangedDocFile ?? TempalteFile;
			}
		}
			
		#endregion

		#region Не сохраняемые

		byte[] changedDocFile;

		[Display (Name = "Измененный Файл шаблона")]
		[PropertyChangedAlso("FileSize")]
		public virtual byte[] ChangedDocFile {
			get { return changedDocFile; }
			set { SetField (ref changedDocFile, value, () => ChangedDocFile); }
		}

		IDocParser docParser;

		public virtual IDocParser DocParser
		{
			get
			{
				if (docParser == null)
					docParser = CreateParser(TemplateType);
				return docParser;
			}
		}

		#endregion

		public DocTemplate()
		{
			Name = templateType.GetEnumTitle();
		}

		#region Функции


		#endregion

		#region Статические

		public static IDocParser CreateParser(TemplateType type)
		{
			switch (type)
			{
				case TemplateType.Contract:
					return new ContractParser();
				case TemplateType.AgWater:
					return new WaterAgreementParser();
				case TemplateType.AgEquip:
					return new EquipmentAgreementParser();
				case TemplateType.AgLongRent:
					return new NonFreeRentAgreementParser();
				case TemplateType.AgFreeRent:
					return new FreeRentAgreementParser();
				case TemplateType.AgShortRent:
					return new DailyRentAgreementParser();
				case TemplateType.AgRepair:
					return new RepairAgreementParser();
				case TemplateType.CarProxy:
					return new CarProxyDocumentParser();
				case TemplateType.M2Proxy:
					return new M2ProxyDocumentParser();
				case TemplateType.EmployeeContract:
					return new EmployeeContractParser();
				case TemplateType.WayBill:
					return new WayBillDocumentParser();
				case TemplateType.CarRentalContract:
					return new CarRentalContractParser();
				default:
					throw new NotImplementedException(String.Format("Тип шаблона {0}, не реализован.", type));
			}
		}

		#endregion
	}
}
