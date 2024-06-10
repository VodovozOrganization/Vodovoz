using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CustomerAppsApi.Library.Dto;
using Gamma.Utilities;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Validators
{
	public class DeliveryPointModelValidator : IDeliveryPointModelValidator
	{
		private const int _cityLimit = 45;
		private const int _streetLimit = 500;
		private const int _buildingLimit = 20;
		private const int _floorLimit = 20;
		private const int _roomLimit = 20;
		private const int _entranceLimit = 50;
		
		private const string _invalidParameter = "Поле {0} заполнено некорректно";
		private const string _parameterEmpty = "Поле {0} не заполнено";
		private const string _parameterOutLimit = "Поле {0} не должно превышать {1} символов";
		
		public string NewDeliveryPointInfoDtoValidate(NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			var sb = new StringBuilder();

			ValidateParameters(sb, newDeliveryPointInfoDto);
			ValidateEmptyParameters(sb, newDeliveryPointInfoDto);
			ValidateOutLimit(sb, newDeliveryPointInfoDto);

			return sb.ToString();
		}

		private void ValidateParameters(StringBuilder sb, NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			if(newDeliveryPointInfoDto.DeliveryPointCategoryId <= 0)
			{
				AppendLineInvalidParameter(sb, newDeliveryPointInfoDto, x => x.DeliveryPointCategoryId);
			}
			
			if(newDeliveryPointInfoDto.CounterpartyErpId <= 0)
			{
				AppendLineInvalidParameter(sb, newDeliveryPointInfoDto, x => x.CounterpartyErpId);
			}
		}

		private void ValidateEmptyParameters(StringBuilder sb, NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.City))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.City);
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Street))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Street);
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Building))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Building);
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Floor))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Floor);
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Entrance))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Entrance);
			}
			
			if(string.IsNullOrWhiteSpace(newDeliveryPointInfoDto.Room))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Room);
			}

			if(newDeliveryPointInfoDto.Longitude == default(decimal))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Longitude);
			}
			
			if(newDeliveryPointInfoDto.Latitude == default(decimal))
			{
				AppendLineParameterEmpty(sb, newDeliveryPointInfoDto, x => x.Latitude);
			}
		}
		
		private void ValidateOutLimit(StringBuilder sb, NewDeliveryPointInfoDto newDeliveryPointInfoDto)
		{
			if(newDeliveryPointInfoDto.City?.Length > _cityLimit)
			{
				AppendLineParameterOutLimit(sb, _cityLimit, newDeliveryPointInfoDto, x => x.City);
			}

			if(newDeliveryPointInfoDto.Street?.Length > _streetLimit)
			{
				AppendLineParameterOutLimit(sb, _streetLimit, newDeliveryPointInfoDto, x => x.Street);
			}

			if(newDeliveryPointInfoDto.Building?.Length > _buildingLimit)
			{
				AppendLineParameterOutLimit(sb, _buildingLimit, newDeliveryPointInfoDto, x => x.Building);
			}

			if(newDeliveryPointInfoDto.Floor?.Length > _floorLimit)
			{
				AppendLineParameterOutLimit(sb, _floorLimit, newDeliveryPointInfoDto, x => x.Floor);
			}
			
			if(newDeliveryPointInfoDto.Entrance?.Length > _entranceLimit)
			{
				AppendLineParameterOutLimit(sb, _entranceLimit, newDeliveryPointInfoDto, x => x.Entrance);
			}

			if(newDeliveryPointInfoDto.Room?.Length > _roomLimit)
			{
				AppendLineParameterOutLimit(sb, _roomLimit, newDeliveryPointInfoDto, x => x.Room);
			}
			
			if(newDeliveryPointInfoDto.Intercom?.Length > DeliveryPoint.IntercomMaxLength)
			{
				AppendLineParameterOutLimit(sb, DeliveryPoint.IntercomMaxLength, newDeliveryPointInfoDto, x => x.Intercom);
			}
		}
		
		private void AppendLineInvalidParameter(StringBuilder sb, NewDeliveryPointInfoDto newDeliveryPointInfoDto,
			Expression<Func<NewDeliveryPointInfoDto, object>> propertyExpression)
		{
			sb.AppendLine(
				string.Format(_invalidParameter, GetPropertyNameFromDisplayAttribute(newDeliveryPointInfoDto, propertyExpression)));
		}

		private void AppendLineParameterEmpty(StringBuilder sb, NewDeliveryPointInfoDto newDeliveryPointInfoDto,
			Expression<Func<NewDeliveryPointInfoDto, object>> propertyExpression)
		{
			sb.AppendLine(string.Format(_parameterEmpty, GetPropertyNameFromDisplayAttribute(newDeliveryPointInfoDto, propertyExpression)));
		}
		
		private void AppendLineParameterOutLimit(StringBuilder sb, int limit, NewDeliveryPointInfoDto newDeliveryPointInfoDto,
			Expression<Func<NewDeliveryPointInfoDto, object>> propertyExpression)
		{
			sb.AppendLine(string.Format(
				_parameterOutLimit,
				GetPropertyNameFromDisplayAttribute(newDeliveryPointInfoDto, propertyExpression),
				limit));
		}

		private string GetPropertyNameFromDisplayAttribute(
			NewDeliveryPointInfoDto newDeliveryPointInfoDto, Expression<Func<NewDeliveryPointInfoDto, object>> propertyExpression)
		{
			return newDeliveryPointInfoDto.GetPropertyInfo(propertyExpression)
				.GetCustomAttribute<DisplayAttribute>(true)?
				.Name;
		}
	}
}
