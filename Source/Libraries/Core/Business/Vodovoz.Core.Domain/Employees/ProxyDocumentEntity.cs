using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "доверенности",
		Nominative = "доверенность")]
	public class ProxyDocumentEntity : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		private int _id;
		private byte[] _changedTemplateFile;
		private ProxyDocumentType _type;
		private OrganizationEntity _organization;
		private EmployeeDocument _employeeDocument;
		private DocTemplateEntity _proxyDocumentTemplate;

		/// <summary>
		/// Unit of work для работы с сущностью
		/// </summary>
		public virtual IUnitOfWork UoW { set; get; }

		/// <summary>
		/// Дата создания доверенности
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime Date { get; set; }

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дата окончания
		/// </summary>
		[Display(Name = "Дата окончания")]
		public virtual DateTime ExpirationDate { get; set; }

		/// <summary>
		/// Тип печати
		/// </summary>
		[Display(Name = "Тип печати")]
		public virtual PrinterType PrintType
		{
			get => PrinterType.None;
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Документ сотрудника
		/// </summary>
		[Display(Name = "Документ сотрудника")]
		public virtual EmployeeDocument EmployeeDocument
		{
			get => _employeeDocument;
			set => SetField(ref _employeeDocument, value);
		}

		/// <summary>
		/// Шаблон доверенности
		/// </summary>
		[Display(Name = "Шаблон доверенности")]
		public virtual DocTemplateEntity DocumentTemplate
		{
			get => _proxyDocumentTemplate;
			protected set => SetField(ref _proxyDocumentTemplate, value);
		}

		/// <summary>
		/// Измененная доверенность
		/// </summary>
		[Display(Name = "Измененная доверенность")]
		public virtual byte[] ChangedTemplateFile
		{
			get => _changedTemplateFile; 
			set => SetField(ref _changedTemplateFile, value); 
		}

		/// <summary>
		/// Тип доверенности
		/// </summary>
		[Display(Name = "Тип доверенности")]
		public virtual ProxyDocumentType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}
	}
}
