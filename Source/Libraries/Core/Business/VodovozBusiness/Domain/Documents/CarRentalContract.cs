using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Nominative = "Договор аренды автомобиля")]
	public class CarRentalContract : IPrintableOdtDocument
	{
		private IDocTemplate _template;

		private CarRentalContract(
			IUnitOfWork unitOfWork,
			IDocTemplateRepository docTemplateRepository,
			Car car,
			Organization organization,
			Employee driver)
		{
			docTemplateRepository
				.GetMatchingTemplate(
					unitOfWork,
					TemplateType.CarRentalContract,
					organization)
				.Match(
					template => _template = template,
					errors => throw new InvalidOperationException(string.Join(", ", errors.Select(x => x.Message))));

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

			DriverPassportSerie = passport.PassportSeria;
			DriverPassportNumber = passport.PassportNumber;
			DriverRegistrationAddress = driver.AddressRegistration;
			DriverResidentialAddress = driver.AddressCurrent;

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

		public static CarRentalContract Create(
			IUnitOfWork unitOfWork,
			IDocTemplateRepository docTemplateRepository,
			Car car,
			Organization organization,
			Employee driver)
		{
			return new CarRentalContract(unitOfWork, docTemplateRepository, car, organization, driver);
		}
	}
}
