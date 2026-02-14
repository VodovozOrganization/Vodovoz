using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class EmployeeMap : ClassMap<Employee>
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
			Map(x => x.PhotoFileName).Column("photo_file_name");
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
			Map(x => x.CanRecieveCounterpartyCalls).Column("can_recieve_counterparty_calls");

			Map(x => x.DriverOfCarTypeOfUse).Column("driver_of_car_type_of_use");
			Map(x => x.DriverOfCarOwnType).Column("driver_of_car_own_type");
			Map(x => x.HasAccessToWarehouseApp).Column("has_access_to_warehouse_app");

			References(x => x.Nationality).Column("nationality_id");
			References(x => x.Citizenship).Column("citizenship_id");
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.User).Column("user_id");
			References(x => x.DefaultForwarder).Column("default_forwarder_id");
			References(x => x.OrganisationForSalary).Column("organisation_for_salary_id");
			References(x => x.Post).Column("employees_posts_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.PhoneForCounterpartyCalls).Column("phone_for_counterparty_calls_id");
			References(x => x.District).Column("district_id");

			HasMany(x => x.Accounts).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Phones).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Documents).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("employee_id");
			HasMany(x => x.Contracts).Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
			HasMany(x => x.WageParameters).Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
			HasMany(x => x.EmployeeRegistrationVersions)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id")
				.OrderBy("start_date DESC");

			HasMany(x => x.DriverWorkScheduleSets)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("driver_id")
				.OrderBy("date_activated DESC");

			HasMany(x => x.DriverDistrictPrioritySets)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("driver_id")
				.OrderBy("date_created DESC");

			HasMany(x => x.ExternalApplicationsUsers)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
		}
	}
}
