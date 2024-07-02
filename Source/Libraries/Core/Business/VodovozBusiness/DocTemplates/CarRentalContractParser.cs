using QS.DocTemplates;
using System.Globalization;
using Vodovoz.Domain.Documents;

namespace Vodovoz.DocTemplates
{
	public class CarRentalContractParser : DocParserBase<CarRentalContract>
	{
		public override void UpdateFields()
		{
			fieldsList.Clear();

			var rus = CultureInfo.GetCultureInfo("ru-RU");

			// Car

			AddField(x => x.CarChasisColor, nameof(CarRentalContract.CarChasisColor), PatternFieldType.FString);
			AddField(x => x.CarModel, nameof(CarRentalContract.CarModel), PatternFieldType.FString);
			AddField(x => x.CarModelReleaseYear, nameof(CarRentalContract.CarModelReleaseYear), PatternFieldType.FString);
			AddField(x => x.CarRegistrationNumber, nameof(CarRentalContract.CarRegistrationNumber), PatternFieldType.FString);
			AddField(x => x.CarVINNumber, nameof(CarRentalContract.CarVINNumber), PatternFieldType.FString);

			// Ceo

			AddField(x => x.CeoFio, nameof(CarRentalContract.CeoFio), PatternFieldType.FString);
			AddField(x => x.CeoFioShort, nameof(CarRentalContract.CeoFioShort), PatternFieldType.FString);

			// Date

			AddField(x => x.CreatedAtDay, nameof(CarRentalContract.CreatedAtDay), PatternFieldType.FNumber);
			AddField(x => x.CreatedAtMonth, nameof(CarRentalContract.CreatedAtMonth), PatternFieldType.FString);
			AddField(x => x.CreatedAtYear, nameof(CarRentalContract.CreatedAtYear), PatternFieldType.FNumber);

			// Driver

			AddField(x => x.DriverBirthDate, nameof(CarRentalContract.DriverBirthDate), PatternFieldType.FString);
			AddField(x => x.DriverFio, nameof(CarRentalContract.DriverFio), PatternFieldType.FString);

			// Driver passport

			AddField(x => x.DriverPassportSerie, nameof(CarRentalContract.DriverPassportSerie), PatternFieldType.FString);
			AddField(x => x.DriverPassportNumber, nameof(CarRentalContract.DriverPassportNumber), PatternFieldType.FString);
			AddField(x => x.DriverRegistrationAddress, nameof(CarRentalContract.DriverRegistrationAddress), PatternFieldType.FString);
			AddField(x => x.DriverResidentialAddress, nameof(CarRentalContract.DriverResidentialAddress), PatternFieldType.FString);

			// Organization

			AddField(x => x.OrganizationFullName, nameof(CarRentalContract.OrganizationFullName), PatternFieldType.FString);
			AddField(x => x.OrganizationAddress, nameof(CarRentalContract.OrganizationAddress), PatternFieldType.FString);


			AddField(x => x.OrganizationCheckingAccount, nameof(CarRentalContract.OrganizationCheckingAccount), PatternFieldType.FString);
			AddField(x => x.OrganizationInn, nameof(CarRentalContract.OrganizationInn), PatternFieldType.FString);
			AddField(x => x.OrganizationKpp, nameof(CarRentalContract.OrganizationKpp), PatternFieldType.FString);

			// Organization Bank Account

			AddField(x => x.OrganizationBankCorrespondentAccount, nameof(CarRentalContract.OrganizationBankCorrespondentAccount), PatternFieldType.FString);

			// Organization Bank

			AddField(x => x.OrganizationBankName, nameof(CarRentalContract.OrganizationBankName), PatternFieldType.FString);
			AddField(x => x.OrganizationBankCity, nameof(CarRentalContract.OrganizationBankCity), PatternFieldType.FString);
			AddField(x => x.OrganizationBankBik, nameof(CarRentalContract.OrganizationBankBik), PatternFieldType.FString);

			SortFields();
		}
	}
}
