using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.StoredEmails
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Электронные почты для отправки",
		Nominative = "Электронная почта для отправки"
	)]
	public class StoredEmail : BusinessObjectBase<StoredEmail>, IDomainObject
	{
		public const int DescriptionMaxLength = 1000;
		private string _subject;
		private Guid? _guid;
		private DateTime _sendDate;
		private StoredEmailStates _state;
		private DateTime _stateChangeDate;
		private string _description;
		private string _recipientAddress;
		private bool? _manualSending;
		private Employee _author;

		public virtual int Id { get; set; }

		public virtual string ExternalId { get; set; }

		[Display(Name = "Дата действия")]
		public virtual DateTime SendDate
		{
			get => _sendDate;
			set => SetField(ref _sendDate, value);
		}

		[Display(Name = "Состояние")]
		public virtual StoredEmailStates State
		{
			get => _state;
			set => SetField(ref _state, value);
		}

		[Display(Name = "Дата действия")]
		public virtual DateTime StateChangeDate
		{
			get => _stateChangeDate;
			set => SetField(ref _stateChangeDate, value);
		}

		[Display(Name = "Описание")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		[Display(Name = "Почта получателя")]
		public virtual string RecipientAddress
		{
			get => _recipientAddress;
			set => SetField(ref _recipientAddress, value);
		}

		[Display(Name = "Отправлено вручную")]
		public virtual bool? ManualSending
		{
			get => _manualSending;
			set => SetField(ref _manualSending, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Тема")]
		public virtual string Subject
		{
			get => _subject;
			set => SetField(ref _subject, value);
		}

		[Display(Name = "Guid")]
		public virtual Guid? Guid
		{
			get => _guid;
			set => SetField(ref _guid, value);
		}

		public virtual void AddDescription(string description)
		{
			if(!string.IsNullOrWhiteSpace(Description))
			{
				Description += "\n";
			}

			Description += description;

			if(Description.Length > DescriptionMaxLength)
			{
				Description = Description.Substring(0, DescriptionMaxLength);
			}
		}
	}
}
