using Newtonsoft.Json.Linq;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Errors;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Nominative = "Договор аренды автомобиля")]
	public class CarRentalContract : IPrintableOdtDocument
	{
		private readonly IDocTemplate _template;

		private CarRentalContract(
			IUnitOfWork unitOfWork,
			IDocTemplate template,
			Car car,
			Organization organization,
			Employee driver)
		{
			_template = template;

			CarChasisColor = car.Color;
			CarModel = car.CarModel.Name;
			CarModelReleaseYear = car.ManufactureYear;
			CarRegistrationNumber = car.RegistrationNumber;
			CarVINNumber = car.VIN;

			var activeOrganizationVersion = organization.ActiveOrganizationVersion;

			CeoFio = activeOrganizationVersion.Leader.FullName;
			CeoFioShort = activeOrganizationVersion.LeaderShortName;

			var today = DateTime.Today;

			CreatedAtDay = today.Day;
			CreatedAtMonth = $"{today:M}";
			CreatedAtYear = today.Year;

			DriverBirthDate = $"{driver.BirthdayDate:dd M yyyy}";
			DriverFio = driver.FullName;

			var passport = driver.Documents
				.FirstOrDefault(x => x.Document == EmployeeDocumentType.Passport);

			DriverPassportSerie = passport?.PassportSeria;
			DriverPassportNumber = passport?.PassportNumber;
			DriverRegistrationAddress = driver?.AddressRegistration;
			DriverResidentialAddress = driver?.AddressCurrent;

			OrganizationFullName = organization.FullName;
			OrganizationAddress = activeOrganizationVersion.Address;
			OrganizationBankName = organization.DefaultAccount.InBank.Name;
			OrganizationBankCity = organization.DefaultAccount.InBank.City;
			OrganizationBankBik = organization.DefaultAccount.InBank.Bik;
			OrganizationBankCorrespondentAccount = organization.DefaultAccount.InBank.DefaultCorAccount.CorAccountNumber;
			OrganizationCheckingAccount = organization.DefaultAccount.Number;
			OrganizationInn = organization.INN;
			OrganizationKpp = organization.KPP;
		}

		#region Template variables

		public string CarChasisColor { get; }
		public string CarModel { get; }
		public string CarModelReleaseYear { get; }
		public string CarRegistrationNumber { get; }
		public string CarVINNumber { get; }

		public string CeoFio { get; }
		public string CeoFioShort { get; }

		public int CreatedAtDay { get; }
		public string CreatedAtMonth { get; }
		public int CreatedAtYear { get; }

		public string DriverBirthDate { get; }
		public string DriverFio { get; }
		public string DriverPassportSerie { get; }
		public string DriverPassportNumber { get; }
		public string DriverRegistrationAddress { get; }
		public string DriverResidentialAddress { get; }

		public string OrganizationFullName { get; }
		public string OrganizationAddress { get; }
		public string OrganizationBankName { get; }
		public string OrganizationBankCity { get; }
		public string OrganizationBankBik { get; }
		public string OrganizationBankCorrespondentAccount { get; }
		public string OrganizationCheckingAccount { get; }
		public string OrganizationInn { get; }
		public string OrganizationKpp { get; }

		#endregion Template variables

		#region Printing

		public PrinterType PrintType => PrinterType.ODT;

		public DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public int CopiesToPrint { get; set; }

		public string Name => typeof(CarRentalContract).GetClassUserFriendlyName().Nominative;

		public IDocTemplate GetTemplate() => _template;

		#endregion Printing

		public static Result<CarRentalContract> Create(
			IUnitOfWork unitOfWork,
			IDocTemplateRepository docTemplateRepository,
			Car car,
			Organization organization,
			Employee driver)
		{
			var docTemplateResult = docTemplateRepository
				.GetMatchingTemplate(
					unitOfWork,
					TemplateType.CarRentalContract,
					organization);

			if(docTemplateResult.IsFailure)
			{
				return Result.Failure<CarRentalContract>(Errors.Documents.DocumentTemplate.NotFound);
			}

			var validationResults = ValidateParameters(car, organization, driver);

			if(validationResults.Any())
			{
				return Result.Failure<CarRentalContract>(validationResults.Select(ve => new Error("ValidationError", ve.ErrorMessage)));
			}

			return new CarRentalContract(unitOfWork, docTemplateResult.Value, car, organization, driver);
		}

		private static IEnumerable<ValidationResult> ValidateParameters(
			Car car,
			Organization organization,
			Employee driver)
		{
			if(car is null)
			{
				yield return new ValidationResult("Не указан автомобиль", new string[] { nameof(car) });
			}

			if(car.CarModel is null)
			{
				yield return new ValidationResult("Не указана модель автомобиля", new string[] { nameof(car.CarModel) });
			}

			if(organization is null)
			{
				yield return new ValidationResult("Не указана организация", new string[] { nameof(organization) });
			}

			if(organization.ActiveOrganizationVersion is null)
			{
				yield return new ValidationResult("В организации отсутствует активная версия", new string[] { nameof(organization.ActiveOrganizationVersion) });
			}

			if(organization.ActiveOrganizationVersion.Leader is null)
			{
				yield return new ValidationResult("В организации не указан руководитель", new string[] { nameof(organization.ActiveOrganizationVersion.Leader) });
			}

			if(organization.DefaultAccount is null)
			{
				yield return new ValidationResult("В организации не указан основной банковский счет", new string[] { nameof(organization.DefaultAccount) });
			}

			if(organization.DefaultAccount.InBank is null)
			{
				yield return new ValidationResult("В организации не указан банк в основном банковском счету", new string[] { nameof(organization.DefaultAccount.InBank) });
			}

			if(organization.DefaultAccount.InBank.DefaultCorAccount is null)
			{
				yield return new ValidationResult("В организации не указан основной корреспондентский счет в банке в основном банковском счету", new string[] { nameof(organization.DefaultAccount.InBank.DefaultCorAccount) });
			}

			if(driver is null)
			{
				yield return new ValidationResult("Не указан водитель", new string[] { nameof(driver) });
			}
		}
	}
}
