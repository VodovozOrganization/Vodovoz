﻿using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFromEntity : PropertyChangedBase, IDomainObject, INamed, IArchivable
	{
		private int _id;
		private string _name;
		private bool _isArchive;
		private bool _receiptRequired;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Архивный")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Требуется чек")]
		public virtual bool ReceiptRequired
		{
			get => _receiptRequired;
			set => SetField(ref _receiptRequired, value);
		}
	}
}
