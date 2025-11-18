using System;
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
	public class DocTemplate : DocTemplateEntity, IDocTemplate
	{
		#region Свойства

		private TemplateType _templateType;
		private Organization _organization;
		private ContractType _contractType;

		/// <summary>
		/// Тип шаблона
		/// </summary>
		[Display (Name = "Тип шаблона")]
		public virtual TemplateType TemplateType {
			get => _templateType;
			set { 
				bool needUpdateName = String.IsNullOrWhiteSpace(Name) || Name == _templateType.GetEnumTitle();
				if (SetField(ref _templateType, value, () => TemplateType))
				{
					_docParser = null;
					if(needUpdateName)
					{
						Name = _templateType.GetEnumTitle();
					}
				}					
			}
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display (Name = "Организация")]
		public virtual new Organization Organization 
		{
			get => _organization;
			set => SetField (ref _organization, value, () => Organization);
		}

		/// <summary>
		/// Тип договора
		/// </summary>
		[Display(Name = "Тип договора")]
		public virtual ContractType ContractType
		{
			get => _contractType;
			set => SetField(ref _contractType, value, () => ContractType);
		}

		#endregion

		#region Вычисляемые

		public virtual long FileSize
		{
			get => TempalteFile != null ? TempalteFile.LongLength : 0;
		}

		public virtual byte[] File
		{
			get => ChangedDocFile ?? TempalteFile;
		}
			
		#endregion

		#region Не сохраняемые

		byte[] _changedDocFile;

		[Display (Name = "Измененный Файл шаблона")]
		[PropertyChangedAlso("FileSize")]
		public virtual byte[] ChangedDocFile 
		{
			get => _changedDocFile;
			set => SetField (ref _changedDocFile, value, () => ChangedDocFile);
		}

		IDocParser _docParser;

		public virtual IDocParser DocParser
		{
			get
			{
				if(_docParser == null)
				{
					_docParser = CreateParser(TemplateType);
				}

				return _docParser;
			}
		}

		#endregion

		public DocTemplate()
		{
			Name = _templateType.GetEnumTitle();
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
