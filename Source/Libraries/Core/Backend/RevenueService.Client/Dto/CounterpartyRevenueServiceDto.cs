using System.ComponentModel.DataAnnotations;
using Dadata.Model;
using Gamma.Utilities;
using RevenueService.Client.Enums;
using RevenueService.Client.Extensions;
using VodovozInfrastructure.Attributes;
using CounterpartyType = RevenueService.Client.Enums.CounterpartyType;

namespace RevenueService.Client.Dto
{
	public class CounterpartyRevenueServiceDto
	{
		[Display(Name = "ИНН")]
		public string Inn { get; set; }

		[Display(Name = "КПП")]
		public string Kpp { get; set; }

		[Display(Name = "Короткое название")]
		public string ShortName { get; set; }

		[Display(Name = "Полное название")]
		public string FullName { get; set; }

		[ReportExportIgnore]
		[Display(Name = "Название")]
		public string Name => string.IsNullOrEmpty(ShortName) ? FullName : ShortName;

		[Display(Name = "Адрес")]
		public string Address { get; set; }

		[Display(Name = "Фамилия")]
		public string PersonSurname { get; set; }

		[Display(Name = "Имя")]
		public string PersonName { get; set; }

		[Display(Name = "Отчество")]
		public string PersonPatronymic { get; set; }

		[ReportExportIgnore]
		[Display(Name = "Полное имя")]
		public string PersonFullName
		{
			get
			{
				if(string.IsNullOrWhiteSpace(PersonSurname) && string.IsNullOrWhiteSpace(PersonName) && string.IsNullOrWhiteSpace(PersonPatronymic))
				{
					return null;
				}

				return string.Join(" ", PersonSurname, PersonName, PersonPatronymic);
			}
		}

		[ReportExportIgnore]
		[Display(Name = "ФИО")]
		public string TitlePersonFullName => PersonFullName ?? ManagerFullName ?? string.Empty;

		[Display(Name = "Телефоны")]
		public string[] Phones { get; set; }

		[Display(Name = "Почтовые ящики")]
		public string[] Emails { get; set; }

		[ReportExportIgnore]
		[Display(Name = "Головная/филиал")]
		public BranchType BranchType { get; set; }

		[Display(Name = "Головная/филиал (строка)")]
		public string BranchTypeString => BranchType.GetEnumTitle();

		[ReportExportIgnore]
		[Display(Name = "Тип контрагента")]
		public CounterpartyType CounterpartyType { get; set; }

		[Display(Name = "Тип контрагента (строка)")]
		public string CounterpartyTypeString => CounterpartyType.GetEnumTitle();

		[Display(Name = "Организационно-правовая форма (аббревиатура)")]
		public string Opf { get; set; }

		[Display(Name = "Полное название организационно-правовой формы")]
		public string OpfFull { get; set; }

		[Display(Name = "ФИО менеджера")]
		public string ManagerFullName { get; set; }

		[Display(Name = "Статус")]
		public PartyStatus State { get; set; }

		[Display(Name = "Название статуса в налоговой")]
		public string RevenueStatusName => State.ConvertToRevenueStatus().GetEnumTitle();

		[ReportExportIgnore]
		[Display(Name = "Активен")]
		public bool IsActive => State == PartyStatus.ACTIVE;
	}
}
