using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "дополнительные соглашения",
		Nominative = "дополнительное соглашение",
		Accusative = "дополнительное соглашение",
		Genitive = "дополнительного соглашения"
	)]
	public class AdditionalAgreementEntity: PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _agreementNumber;
		private DocTemplateEntity _agreeemntTemplate;
		private byte[] _changedTemplateFile;
		private CounterpartyContractEntity _contract;
		private DateTime _issueDate;
		private DateTime _startDate;
		private DeliveryPointEntity _deliveryPoint;
		private bool _isCancelled;

		/// <summary>
		/// Идентификатор дополнительного соглашения
		/// </summary>
		[Display(Name = "Идентификатор дополнительного соглашения")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Дополнительное соглашение
		/// </summary>
		public virtual AdditionalAgreementEntity Self => this;

		/// <summary>
		/// Номер
		/// </summary>
		[Display(Name = "Номер")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual int AgreementNumber
		{
			get => _agreementNumber;
			set => SetField(ref _agreementNumber, value);
		}

		/// <summary>
		/// Шаблон договора
		/// </summary>
		[Display(Name = "Шаблон договора")]
		public virtual DocTemplateEntity DocumentTemplate
		{
			get => _agreeemntTemplate;
			protected set => SetField(ref _agreeemntTemplate, value);
		}

		/// <summary>
		/// Измененное соглашение
		/// </summary>
		[Display(Name = "Измененное соглашение")]
		[PropertyChangedAlso("FileSize")]
		public virtual byte[] ChangedTemplateFile
		{
			get => _changedTemplateFile;
			set => SetField(ref _changedTemplateFile, value);
		}

		/// <summary>
		/// Тип доп. соглашения
		/// </summary>
		[Display(Name = "Тип доп. соглашения")]
		public virtual AgreementType Type
		{
			get
			{
				if(this is DailyRentAgreementEntity)
				{
					return AgreementType.DailyRent;
				}

				if(this is NonfreeRentAgreementEntity)
				{
					return AgreementType.NonfreeRent;
				}

				if(this is FreeRentAgreementEntity)
				{
					return AgreementType.FreeRent;
				}

				if(this is WaterSalesAgreementEntity)
				{
					return AgreementType.WaterSales;
				}

				if(this is SalesEquipmentAgreementEntity)
				{
					return AgreementType.EquipmentSales;
				}

				return AgreementType.Repair;
			}
		}

		/// <summary>
		/// Договор
		/// </summary>
		[Display(Name = "Договор")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual CounterpartyContractEntity Contract
		{
			get => _contract;
			set => SetField(ref _contract, value);
		}

		/// <summary>
		/// Дата подписания
		/// </summary>
		[Display(Name = "Дата подписания")]
		[HistoryDateOnly]
		public virtual DateTime IssueDate
		{
			get => _issueDate;
			set => SetField(ref _issueDate, value);
		}

		/// <summary>
		/// Дата начала
		/// </summary>
		[Display(Name = "Дата начала")]
		[HistoryDateOnly]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		/// <summary>
		/// Точка доставки
		/// </summary>
		[Display(Name = "Точка доставки")]
		public virtual DeliveryPointEntity DeliveryPoint 
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		/// <summary>
		/// Закрыто
		/// </summary>
		[Display(Name = "Закрыто")]
		public virtual bool IsCancelled
		{
			get => _isCancelled;
			set => SetField(ref _isCancelled, value);
		}

		#region Вычисляемые

		/// <summary>
		/// Тип соглашения в виде заголовка
		/// </summary>
		public virtual string AgreementTypeTitle => Type.GetEnumTitle();

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title => $"Доп. соглашение №{FullNumberText} от {StartDate.ToShortDateString()}";

		/// <summary>
		/// Полный номер
		/// </summary>
		[Display(Name = "Полный номер")]
		public virtual string FullNumberText => $"{Contract.Number}/{GetTypePrefix(Type)}{AgreementNumber}";

		#endregion

		#region Статические

		/// <summary>
		/// Получить префикс типа соглашения  по типу соглашения
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static string GetTypePrefix(AgreementType type)
		{
			switch(type)
			{
				case AgreementType.DailyRent:
					return "АС";
				case AgreementType.NonfreeRent:
					return "АМ";
				case AgreementType.FreeRent:
					return "Б";
				case AgreementType.Repair:
					return "Т";
				case AgreementType.WaterSales:
					return "В";
				case AgreementType.EquipmentSales:
					return "П";
				default:
					throw new InvalidOperationException(string.Format("Тип {0} не поддерживается.", type));
			}
		}

		/// <summary>
		/// Получить тип шаблона по типу соглашения
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static TemplateType GetTemplateType(AgreementType type)
		{
			switch(type)
			{
				case AgreementType.DailyRent:
					return TemplateType.AgShortRent;
				case AgreementType.NonfreeRent:
					return TemplateType.AgLongRent;
				case AgreementType.FreeRent:
					return TemplateType.AgFreeRent;
				case AgreementType.Repair:
					return TemplateType.AgRepair;
				case AgreementType.WaterSales:
					return TemplateType.AgWater;
				case AgreementType.EquipmentSales:
					return TemplateType.AgEquip;
				default:
					throw new InvalidOperationException(string.Format("Тип {0} не поддерживается.", type));
			}
		}

		#endregion
	}
}
