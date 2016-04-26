using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "дополнительные соглашения",
		Nominative = "дополнительное соглашение",
		Accusative = "дополнительное соглашение",
		Genitive = "дополнительного соглашения"
	)]
	public class AdditionalAgreement : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		int agreementNumber;

		[Display (Name = "Номер")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual int AgreementNumber { 
			get { return agreementNumber; } 
			set { SetField (ref agreementNumber, value, () => AgreementNumber); }
		}

		AgreementType type;

		[Display (Name = "Тип доп. соглашения")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual AgreementType Type{
			get { return type; } 
			protected set { SetField (ref type, value, () => Type); }
		}

		[Required (ErrorMessage = "Договор должен быть указан.")]
		[Display (Name = "Договор")]
		[PropertyChangedAlso("FullNumberText")]
		public virtual CounterpartyContract Contract { get; set; }

		[Required (ErrorMessage = "Дата создания должна быть указана.")]
		[Display (Name = "Дата подписания")]
		public virtual DateTime IssueDate { get; set; }

		[Required (ErrorMessage = "Дата начала действия должна быть указана.")]
		[Display (Name = "Дата начала")]
		public virtual DateTime StartDate { get; set; }

		[Display (Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint { get; set; }

		[Display (Name = "Закрыто")]
		public virtual bool IsCancelled { get; set; }

		#region Вычисляемые

		public virtual string AgreementDeliveryPoint { get { return DeliveryPoint != null ? DeliveryPoint.CompiledAddress : "Не указана"; } }

		public virtual string AgreementTypeTitle { get { return Type.GetEnumTitle (); } }

		public virtual string DocumentDate { get { return String.Format ("От {0}", StartDate.ToShortDateString ()); } }

		public virtual string Title { get { return String.Format ("Доп. соглашение №{0} от {1}", FullNumberText, StartDate.ToShortDateString ()); } }

		public virtual string FullNumberText {
			get{
				return String.Format("{0}-{1}{2}", Contract.Id, GetTypePrefix(Type), AgreementNumber);
			}
		}
	
		#endregion

		public AdditionalAgreement ()
		{
			IssueDate = StartDate = DateTime.Now;
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			int count = 0;
			foreach (AdditionalAgreement agreement in Contract.AdditionalAgreements)
				if (agreement.AgreementNumber == this.AgreementNumber)
					count++;
			if (count > 1)
				yield return new ValidationResult ("Доп. соглашение с таким номером уже существует.", new[] { "AgreementNumber" });
		}

		#region Статические

		public static int GetNumber (CounterpartyContract contract)
		{
			//Вычисляем номер для нового соглашения.
			var additionalAgreements = contract.AdditionalAgreements;
			var numbers = additionalAgreements.Select(x => x.AgreementNumber).ToList();
			numbers.Sort ();

			if (numbers.Count > 0) {
				return numbers.Last() + 1;
			} else
				return 1;
		}

		public static string GetTypePrefix(AgreementType type)
		{
			switch (type)
			{
				case AgreementType.DailyRent:
					return "АС";
				case AgreementType.NonfreeRent:
					return "А";
				case AgreementType.FreeRent:
					return "Б";
				case AgreementType.Repair:
					return "Т";
				case AgreementType.WaterSales:
					return "В";
				default:
					throw new InvalidOperationException(String.Format("Тип {0} не поддерживается.", type));
			}
		}

		#endregion
	}

	public enum AgreementType
	{
		[Display (Name = "Долгосрочая аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent,
		[Display (Name = "Продажа воды")]
		WaterSales,
		[Display (Name = "Ремонт")]
		Repair
	}

	public class AgreementTypeStringType : NHibernate.Type.EnumStringType
	{
		public AgreementTypeStringType () : base (typeof(AgreementType))
		{
		}
	}

	public enum OrderAgreementType
	{
		[Display (Name = "Долгосрочная аренда")]
		NonfreeRent,
		[Display (Name = "Посуточная аренда")]
		DailyRent,
		[Display (Name = "Бесплатная аренда")]
		FreeRent
	}

	public interface IAgreementSaved
	{
		event EventHandler<AgreementSavedEventArgs> AgreementSaved;
	}

	public class AgreementSavedEventArgs : EventArgs
	{
		public AdditionalAgreement Agreement { get; private set; }

		public AgreementSavedEventArgs (AdditionalAgreement agreement)
		{
			Agreement = agreement;
		}
	}
}

