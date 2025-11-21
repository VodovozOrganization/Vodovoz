using Gamma.Utilities;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны документов",
		Nominative = "шаблон документа")]
	[EntityPermission]
	public class DocTemplateEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _name;
		private OrganizationEntity _organization;
		private TemplateType _templateType;
		private ContractType _contractType;
		private byte[] _templateFile;
		private IDocParser _docParser;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value, () => Id);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		[StringLength(45)]
		public virtual string Name
		{
			get { return _name; }
			set { SetField(ref _name, value, () => Name); }
		}

		/// <summary>
		/// Тип шаблона
		/// </summary>
		[Display(Name = "Тип шаблона")]
		public virtual TemplateType TemplateType
		{
			get => _templateType;
			set
			{
				bool needUpdateName = String.IsNullOrWhiteSpace(Name) || Name == _templateType.GetEnumTitle();
				if(SetField(ref _templateType, value, () => TemplateType))
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
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value, () => Organization);
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

		/// <summary>
		/// Файл шаблона
		/// </summary>
		[Display(Name = "Файл шаблона")]
		[PropertyChangedAlso("FileSize")]
		[Required]
		public virtual byte[] TempalteFile
		{
			get => _templateFile;
			set => SetField(ref _templateFile, value, () => TempalteFile);
		}
	}
}
