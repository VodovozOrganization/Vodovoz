using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	/// <summary>
	/// Объяснительная записка по корректировке чека.
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "объяснительная записка",
		NominativePlural = "объяснительные записки")]
	public class ReceiptCorrectionExplanatoryNote : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private ReceiptCorrectionProcess _process;
		private ReceiptCorrectionExplanatoryNoteTemplateType _templateType;
		private int? _organizationId;
		private string _signerTitle;
		private string _signerName;
		private int? _signerSignatureId;
		private string _content;
		private DateTime _createdDate;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Процесс")]
		public virtual ReceiptCorrectionProcess Process
		{
			get => _process;
			set => SetField(ref _process, value);
		}

		[Display(Name = "Шаблон")]
		public virtual ReceiptCorrectionExplanatoryNoteTemplateType TemplateType
		{
			get => _templateType;
			set => SetField(ref _templateType, value);
		}

		[Display(Name = "Организация")]
		public virtual int? OrganizationId
		{
			get => _organizationId;
			set => SetField(ref _organizationId, value);
		}

		[Display(Name = "Должность подписанта")]
		public virtual string SignerTitle
		{
			get => _signerTitle;
			set => SetField(ref _signerTitle, value);
		}

		[Display(Name = "ФИО подписанта")]
		public virtual string SignerName
		{
			get => _signerName;
			set => SetField(ref _signerName, value);
		}

		[Display(Name = "Подпись подписанта")]
		public virtual int? SignerSignatureId
		{
			get => _signerSignatureId;
			set => SetField(ref _signerSignatureId, value);
		}

		[Display(Name = "Текст")]
		public virtual string Content
		{
			get => _content;
			set => SetField(ref _content, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedDate
		{
			get => _createdDate;
			set => SetField(ref _createdDate, value);
		}
	}
}
