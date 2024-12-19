using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Clients
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "контрагенты",
			Nominative = "контрагент",
			Accusative = "контрагента",
			Genitive = "контрагента"
		)
	]
	[HistoryTrace]
	[EntityPermission]
	public class CounterpartyEntity : AccountOwnerBase, IDomainObject, IHasAttachedFilesInformations<CounterpartyFileInformation>
	{
		private int _id;
		private OrderStatusForSendingUpd _orderStatusForSendingUpd;
		private ConsentForEdoStatus _consentForEdoStatus;
		private OrganizationEntity _worksThroughOrganization;
		private bool _isNewEdoProcessing;
		private IObservableList<CounterpartyFileInformation> _attachedFileInformations = new ObservableList<CounterpartyFileInformation>();

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		[Display(Name = "Согласие клиента на ЭДО")]
		public virtual ConsentForEdoStatus ConsentForEdoStatus
		{
			get => _consentForEdoStatus;
			set => SetField(ref _consentForEdoStatus, value);
		}

		[Display(Name = "Статус заказа для отправки УПД")]
		public virtual OrderStatusForSendingUpd OrderStatusForSendingUpd
		{
			get => _orderStatusForSendingUpd;
			set => SetField(ref _orderStatusForSendingUpd, value);
		}

		[Display(Name = "Работает через организацию")]
		public virtual OrganizationEntity WorksThroughOrganization
		{
			get => _worksThroughOrganization;
			set => SetField(ref _worksThroughOrganization, value);
		}

		/// <summary>
		/// Документооборот по ЭДО с клиентом осуществляется по новой схеме
		/// </summary>
		[Display(Name = "Работа с ЭДО по новой схеме")]
		public virtual bool IsNewEdoProcessing
		{
			get => _isNewEdoProcessing;
			set => SetField(ref _isNewEdoProcessing, value);
		}

		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<CounterpartyFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
		}

		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(afi => afi.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new CounterpartyFileInformation
			{
				FileName = fileName,
				CounterpartyId = Id
			});
		}

		public virtual void RemoveFileInformation(string fileName)
		{
			AttachedFileInformations.Remove(AttachedFileInformations.FirstOrDefault(afi => afi.FileName == fileName));
		}

		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.CounterpartyId = Id;
			}
		}
	}
}
