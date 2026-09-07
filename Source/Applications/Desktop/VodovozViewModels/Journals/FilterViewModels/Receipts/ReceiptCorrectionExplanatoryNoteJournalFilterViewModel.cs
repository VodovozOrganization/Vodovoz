using QS.Project.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Receipts
{
	public class ReceiptCorrectionExplanatoryNoteJournalFilterViewModel : FilterViewModelBase<ReceiptCorrectionExplanatoryNoteJournalFilterViewModel>
	{
		private int? _orderId;
		private Organization _organization;
		private DateTime _dateFrom;
		private DateTime _dateTo;
		private IEnumerable<Organization> _organizations;

		public ReceiptCorrectionExplanatoryNoteJournalFilterViewModel()
		{
			_dateFrom = DateTime.Today.AddMonths(-1);
			_dateTo = DateTime.Today;
		}

		// Только Id/Name — без DefaultAccount/банков (полная загрузка Organization валит фильтр на accounts).
		public IEnumerable<Organization> Organizations =>
			_organizations ?? (_organizations = UoW.Session.Query<Organization>()
				.OrderBy(x => x.Name)
				.Select(x => new { x.Id, x.Name })
				.ToList()
				.Select(x => new Organization
				{
					Id = x.Id,
					Name = x.Name
				})
				.ToList());

		public virtual int? OrderId
		{
			get => _orderId;
			set => UpdateFilterField(ref _orderId, value);
		}

		public virtual Organization Organization
		{
			get => _organization;
			set => UpdateFilterField(ref _organization, value);
		}

		public virtual DateTime DateFrom
		{
			get => _dateFrom;
			set => UpdateFilterField(ref _dateFrom, value);
		}

		public virtual DateTime DateTo
		{
			get => _dateTo;
			set => UpdateFilterField(ref _dateTo, value);
		}
	}
}
