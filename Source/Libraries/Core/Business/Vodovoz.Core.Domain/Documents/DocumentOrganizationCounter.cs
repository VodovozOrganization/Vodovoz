using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Documents
{
	public class DocumentOrganizationCounter: PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrganizationEntity _organization;
		private OrderDocumentEntity _orderDocument;
		private int _counter;
		private DateTime _counterDate;
		private DocumentType _documentType;
		private string _documentNumber;

		/// <summary>
		/// ID
		/// </summary>
		[Display(Name = "ID")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
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
		/// Документ из заказа
		/// </summary>
		[Display(Name = "Документ из заказа")]
		public virtual OrderDocumentEntity OrderDocument
		{
			get => _orderDocument;
			set => SetField(ref _orderDocument, value);
		}

		/// <summary>
		/// Счетчик документов УПД
		/// </summary>
		[Display(Name = "Счетчик документов УПД")]
		public virtual int Counter
		{
			get => _counter;
			set => SetField(ref _counter, value);
		}

		/// <summary>
		/// Дата создания счетчика
		/// </summary>
		[Display(Name = "Дата создания счетчика")]
		public virtual DateTime CounterDate
		{
			get => _counterDate;
			set => SetField(ref _counterDate, value);
		}
		
		/// <summary>
		/// Номер документа подготовленный
		/// </summary>
		[Display(Name = "Номер документа подготовленный")]
		public virtual string DocumentNumber
		{
			get => _documentNumber;
			set => SetField(ref _documentNumber, value);
		}
	}
}
