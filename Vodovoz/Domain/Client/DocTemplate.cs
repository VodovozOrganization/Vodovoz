using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.DocTemplates;

namespace Vodovoz.Domain.Client
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "шаблоны документов",
		Nominative = "шаблон документа")]
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

		byte[] file;

		[Display (Name = "Файл шаблона")]
		[PropertyChangedAlso("FileSize")]
		[Required]
		public virtual byte[] File {
			get { return file; }
			set { SetField (ref file, value, () => File); }
		}

		#endregion

		#region Вычисляемые

		public virtual long FileSize{
			get{
				return File != null ? File.LongLength : 0;
			}
		}
			
		#endregion

		#region Не сохраняемые

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

		public static IDocParser CreateParser(TemplateType type)
		{
			switch (type)
			{
				case TemplateType.Contract:
					return new ContractParser();
				default:
					throw new NotImplementedException(String.Format("Тип шаблона {0}, не реализован.", type));
			}
		}
	}

	public enum TemplateType
	{
		[Display (Name = "Основной договор")]
		Contract,
		[Display (Name = "Доп. соглашение на воду")]
		AgWater,
		[Display (Name = "Доп. соглашение бесплатной аренды")]
		AgFreeRent,
		[Display (Name = "Доп. соглашение короткосрочной аренды")]
		AgShortRent,
		[Display (Name = "Доп. соглашение долгосрочной аренды")]
		AgLongRent,
		[Display (Name = "Доп. соглашение на обслуживание")]
		AgRepair
	}

	public class TemplateTypeStringType : NHibernate.Type.EnumStringType
	{
		public TemplateTypeStringType () : base (typeof(TemplateType))
		{
		}
	}
}

