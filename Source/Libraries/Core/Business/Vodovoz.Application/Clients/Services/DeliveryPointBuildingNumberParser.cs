using Core.Infrastructure;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using VodovozBusiness.Services.Clients.DeliveryPoints;

namespace Vodovoz.Application.Clients.Services
{
	public class DeliveryPointBuildingNumberParser : IDeliveryPointBuildingNumberParser
	{
		public BuildingNumberDetails ParseBuildingNumber(string building)
		{
			var buildingDetails = new BuildingNumberDetails();

			var i = 0;

			if(!TryParseBuildingNumber(building, ref i, buildingDetails))
			{
				return buildingDetails;
			}

			while(i < building.Length)
			{
				if(building[i].IsDotOrComma() || char.IsWhiteSpace(building[i]))
				{
					i++;
					continue;
				}

				if(char.IsLetter(building[i]) && char.IsUpper(building[i]))
				{
					TryParseLiter(building, ref i, buildingDetails);
				}

				if(i >= building.Length)
				{
					break;
				}

				if(building[i] == 'к')
				{
					i++;
					if(!TryParseCorpus(building, ref i, buildingDetails))
					{
						return buildingDetails;
					}
				}
				
				if(i >= building.Length)
				{
					break;
				}
				
				//Проверяем и русскую и английскую буквы 
				if(building[i] == 'с' || building[i] == 'c')
				{
					i++;
					TryParseStructure(building, ref i, buildingDetails);
				}
				
				if(i >= building.Length)
				{
					break;
				}

				i++;
			}
			
			return buildingDetails;
		}

		private void TryParseStructure(string building, ref int i, BuildingNumberDetails buildingNumberDetails)
		{
			while(i < building.Length)
			{
				if(string.IsNullOrWhiteSpace(buildingNumberDetails.Structure)
				   && (building[i].IsDotOrComma()
				       || char.IsWhiteSpace(building[i])
				       || building[i] == 'т'
				       || building[i] == 'р'
				       || building[i] == 'о'
				       || building[i] == 'е'
				       || building[i] == 'н'
				       || building[i] == 'и'))
				{
					i++;
					continue;
				}

				if(!string.IsNullOrWhiteSpace(buildingNumberDetails.Structure)
				   && (building[i].IsDotOrComma() || char.IsWhiteSpace(building[i])))
				{
					break;
				}

				if(char.IsDigit(building[i]))
				{
					buildingNumberDetails.Structure += building[i];
				}

				i++;
			}
		}

		private void TryParseLiter(string building, ref int i, BuildingNumberDetails buildingNumberDetails)
		{
			//Счетчик количества цифр в литере, например АИ1, АБ2
			var numberCount = 0;
			
			while(i < building.Length)
			{
				if(building[i].IsDotOrComma()
				   || char.IsWhiteSpace(building[i])
				   || (char.IsLetter(building[i]) && char.IsLower(building[i])))
				{
					break;
				}

				if(char.IsDigit(building[i]) && numberCount < 2)
				{
					buildingNumberDetails.Liter += building[i];
					numberCount++;
					i++;
					continue;
				}
				
				if(char.IsLetter(building[i]) && char.IsUpper(building[i]))
				{
					buildingNumberDetails.Liter += building[i];
				}
				
				if(building[i] == '/')
				{
					buildingNumberDetails.Liter += building[i];
				}
				
				if(building[i] == '-')
				{
					buildingNumberDetails.Liter += building[i];
				}
				
				i++;
			}
			
			buildingNumberDetails.BuildingNumber = buildingNumberDetails.BuildingNumber.TrimEnd('-', '/');
		}

		private bool TryParseBuildingNumber(string building, ref int i, BuildingNumberDetails buildingNumberDetails)
		{
			if(i == default && !char.IsDigit(building[i]))
			{
				return false;
			}
			
			var tempBuilding = building;
			var hasSlash = false;
			
			while(i < tempBuilding.Length)
			{
				if(char.IsWhiteSpace(tempBuilding[i])
				   || char.IsLetter(tempBuilding[i]))
				{
					break;
				}

				if(tempBuilding[i].IsDotOrComma())
				{
					tempBuilding = tempBuilding.Remove(i, 1);
					continue;
				}
				
				if(char.IsDigit(tempBuilding[i]))
				{
					buildingNumberDetails.BuildingNumber += tempBuilding[i];
				}

				if(tempBuilding[i] == '-')
				{
					buildingNumberDetails.BuildingNumber += tempBuilding[i];
				}

				if(tempBuilding[i] == '/')
				{
					if(hasSlash)
					{
						tempBuilding = tempBuilding.Remove(i, 1);
						continue;
					}
					
					buildingNumberDetails.BuildingNumber += tempBuilding[i];
					hasSlash = true;
				}
				
				i++;
			}

			buildingNumberDetails.BuildingNumber = buildingNumberDetails.BuildingNumber.TrimEnd('-', '/');

			return true;
		}
		
		private bool TryParseCorpus(string building, ref int i, BuildingNumberDetails buildingNumberDetails)
		{
			var needParseLiter = buildingNumberDetails.Liter is null;
			
			while(i < building.Length)
			{
				if(string.IsNullOrWhiteSpace(buildingNumberDetails.Corpus)
				   && (building[i].IsDotOrComma()
				       || char.IsWhiteSpace(building[i])
				       || building[i] == 'о'
				       || building[i] == 'р'
				       || building[i] == 'п'
				       || building[i] == 'у'
				       || building[i] == 'с'))
				{
					i++;
					continue;
				}

				if(!string.IsNullOrWhiteSpace(buildingNumberDetails.Corpus))
				{
					if(building[i].IsDotOrComma() || char.IsWhiteSpace(building[i]))
					{
						break;
					}
					
					if(char.IsLetter(building[i]) && char.IsUpper(building[i]))
					{
						if(!needParseLiter)
						{
							return false;
						}
						
						break;
					}
				}
				
				if(char.IsDigit(building[i]))
				{
					buildingNumberDetails.Corpus += building[i];
				}
				
				i++;
			}

			if(needParseLiter)
			{
				TryParseLiter(building, ref i, buildingNumberDetails);
			}

			return true;
		}
	}
}
