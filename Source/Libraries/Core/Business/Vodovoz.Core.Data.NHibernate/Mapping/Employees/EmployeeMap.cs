using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Employees
{
	public class EmployeeMap : ClassMap<EmployeeEntity>
	{
		public EmployeeMap()
		{
			Table("employees");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.Name).Column("name");
			Map(x => x.LastName).Column("last_name");
			Map(x => x.Patronymic).Column("patronymic");
			Map(x => x.DrivingLicense).Column("driving_number");
			Map(x => x.Photo).Column("photo").CustomSqlType("BinaryBlob").LazyLoad();
			Map(x => x.AddressRegistration).Column("address_registration");
			Map(x => x.BirthdayDate).Column("birthday_date");
			Map(x => x.AddressCurrent).Column("address_current");
			Map(x => x.INN).Column("inn");
			Map(x => x.IsRussianCitizen).Column("is_russian_citizen");
			Map(x => x.InnerPhone).Column("inner_phone");
			Map(x => x.Email).Column("email_address");
			Map(x => x.Category).Column("category");
			Map(x => x.Status).Column("status");
			Map(x => x.AndroidLogin).Column("android_login");
			Map(x => x.AndroidPassword).Column("android_password");
			Map(x => x.AndroidSessionKey).Column("android_session_key");
			Map(x => x.AndroidToken).Column("android_token");
			Map(x => x.FirstWorkDay).Column("first_work_day");
			Map(x => x.DateHired).Column("date_hired");
			Map(x => x.DateCalculated).Column("date_calculated");
			Map(x => x.DateFired).Column("date_fired");
			Map(x => x.TripPriority).Column("priority_for_trip");
			Map(x => x.DriverSpeed).Column("driver_speed");
			Map(x => x.VisitingMaster).Column("visiting_master");
			Map(x => x.IsChainStoreDriver).Column("is_chain_store_driver");
			Map(x => x.IsDriverForOneDay).Column("is_driver_for_one_day");
			Map(x => x.Gender).Column("gender");
			Map(x => x.MinRouteAddresses).Column("min_route_addresses");
			Map(x => x.MaxRouteAddresses).Column("max_route_addresses");
			Map(x => x.DriverType).Column("driver_type");
			Map(x => x.SkillLevel).Column("skill_level");
			Map(x => x.Comment).Column("comment");

			References(x => x.DefaultAccount).Column("default_account_id");
		}
	}
}
