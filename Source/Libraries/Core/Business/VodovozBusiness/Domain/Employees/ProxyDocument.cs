using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "доверенности",
		Nominative = "доверенность")]
	public abstract class ProxyDocument : PropertyChangedBase, IDomainObject, IBusinessObject
	{
		private Organization _organization;
		private EmployeeDocument _employeeDocument;
		private DocTemplate _proxyDocumentTemplate;
		private byte[] _changedTemplateFile;
		private ProxyDocumentType _type;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id { get; set; }

		public virtual IUnitOfWork UoW { set; get; }

		/// <summary>
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date { get; set; }

		/// <summary>
		/// Дата окончания
		/// </summary>
		[Display(Name = "Дата окончания")]
		public virtual DateTime ExpirationDate { get; set; }

		/// <summary>
		/// Текстовое представление даты
		/// </summary>
		public virtual string DateText => Date.ToShortDateString() ?? "не указана";

		/// <summary>
		/// Тип принтера для печати документа
		/// </summary>
		[Display(Name = "Тип принтера для печати документа")]
		public virtual PrinterType PrintType => PrinterType.None;

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization; 
			set => SetField(ref _organization, value, () => Organization);
		}

		/// <summary>
		/// Документ сотрудника
		/// </summary>
		[Display(Name = "Документ сотрудника")]
		public virtual EmployeeDocument EmployeeDocument
		{
			get => _employeeDocument;
			set => SetField(ref _employeeDocument, value, () => EmployeeDocument);
		}

		/// <summary>
		/// Шаблон доверенности
		/// </summary>
		[Display(Name = "Шаблон доверенности")]
		public virtual DocTemplate DocumentTemplate {
			get => _proxyDocumentTemplate;
			protected set { SetField(ref _proxyDocumentTemplate, value, () => DocumentTemplate); }
		}

		/// <summary>
		/// Измененная доверенность
		/// </summary>
		[Display(Name = "Измененная доверенность")]
		public virtual byte[] ChangedTemplateFile {
			get => _changedTemplateFile;
			set => SetField(ref _changedTemplateFile, value, () => ChangedTemplateFile);
		}

		/// <summary>
		/// Тип доверенности
		/// </summary>
		[Display(Name = "Тип доверенности")]
		public virtual ProxyDocumentType Type {
			get => _type; 
			set => SetField(ref _type, value, () => Type);
		}

		#region static

		/// <summary>
		/// Получить класс документа по его типу
		/// </summary>
		/// <param name="docType"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
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
}
