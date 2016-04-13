using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace QSDocTemplates.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "шаблоны документов",
		Nominative = "шаблон документа")]
	public class DocTemplate : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		TemplateType category;

		[Display (Name = "Тип шаблона")]
		public virtual TemplateType Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}

		byte[] file;

		[Display (Name = "Файл шаблона")]
		public virtual byte[] File {
			get { return file; }
			set { SetField (ref file, value, () => File); }
		}

		#endregion

		public DocTemplate()
		{
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

	public class EmployeeCategoryStringType : NHibernate.Type.EnumStringType
	{
		public EmployeeCategoryStringType () : base (typeof(TemplateType))
		{
		}
	}
}

