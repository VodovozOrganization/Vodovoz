using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSReport;
using Vodovoz.Domain.Client;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "доверенности",
		Nominative = "доверенность")]
	public abstract class ProxyDocument : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		[UseForSearch]
		public virtual int Id { get; set; }

		public virtual IUnitOfWork UoW { set; get; }

		public virtual DateTime Date { get; set; }

		[Display(Name = "Дата окончания")]
		public virtual DateTime ExpirationDate { get; set; }

		public virtual string DateText => Date.ToShortDateString() ?? "не указана";

		public virtual PrinterType PrintType {
			get {
				return PrinterType.None;
			}
		}

		Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
		}

		DocTemplate proxyDocumentTemplate;
		[Display(Name = "Шаблон доверенности")]
		public virtual DocTemplate DocumentTemplate {
			get { return proxyDocumentTemplate; }
			protected set { SetField(ref proxyDocumentTemplate, value, () => DocumentTemplate); }
		}

		byte[] changedTemplateFile;
		[Display(Name = "Измененная доверенность")]
		public virtual byte[] ChangedTemplateFile {
			get { return changedTemplateFile; }
			set { SetField(ref changedTemplateFile, value, () => ChangedTemplateFile); }
		}

		ProxyDocumentType type;
		[Display(Name = "Тип доверенности")]
		[UseForSearch]
		public virtual ProxyDocumentType Type {
			get { return type; }
			set { SetField(ref type, value, () => Type); }
		}

		#region static
		public static Type GetProxyDocumentClass(ProxyDocumentType docType)
		{
			switch(docType) {
				case ProxyDocumentType.CarProxy:
					return typeof(CarProxyDocument);
				case ProxyDocumentType.M2Proxy:
					return typeof(M2ProxyDocument);
			}
			throw new NotSupportedException();
		}
		#endregion
	}

	public enum ProxyDocumentType
	{
		[Display(Name = "Доверенность на ТС")]
		CarProxy,
		[Display(Name = "Доверенность M-2")]
		M2Proxy
	}

	public class ProxyDocumentStringType : NHibernate.Type.EnumStringType
	{
		public ProxyDocumentStringType() : base(typeof(ProxyDocumentType))
		{
		}
	}
}