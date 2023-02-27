﻿using Gamma.Utilities;
using RevenueService.Client.Enums;
using System.ComponentModel.DataAnnotations;
using VodovozInfrastructure.Attributes;

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
				if(PersonSurname == null && PersonName == null && PersonPatronymic == null)
				{
					return null;
				}

				return string.Join(" ", PersonSurname, PersonName, PersonPatronymic);
			}
		}

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
		public TypeOfOwnership TypeOfOwnerShip { get; set; }

		[Display(Name = "Тип контрагента (строка)")]
		public string TypeOfOwnershipString => TypeOfOwnerShip.GetEnumTitle();

		[Display(Name = "ФИО менеджера")]
		public string ManagerFullName { get; set; }

		[Display(Name = "Статус")]
		public string State { get; set; }

		[ReportExportIgnore]
		[Display(Name = "Активен")]
		public bool IsActive => State == "ACTIVE";
	}
}
