using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Employees
{
	public class PersonnelMap : ClassMap<Personnel>
	{
		public PersonnelMap()
		{
			Table("employees");
			DiscriminateSubClassesOnColumn("employee_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.EmployeeType).Column("employee_type").CustomType<EmployeeTypeStringType>().Update().Not.Insert();
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

			References(x => x.Nationality).Column("nationality_id");
			References(x => x.Citizenship).Column("citizenship_id");

			HasMany(x => x.Accounts).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Phones).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Documents).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Attachments).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
		}

		public class EmployeeMap : SubclassMap<Employee>
		{
			public EmployeeMap()
			{
				DiscriminatorValue(EmployeeType.Employee.ToString());
				Map(x => x.InnerPhone).Column("inner_phone");
				Map(x => x.Email).Column("email_address");
				Map(x => x.Category).Column("category").CustomType<EmployeeCategoryStringType>();
				Map(x => x.Status).Column("status").CustomType<EmployeeStatusStringType>();
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
				Map(x => x.DriverOfCarTypeOfUse).Column("driver_of_car_type_of_use").CustomType<CarTypeOfUseStringType>();
				Map(x => x.DriverOfCarOwnType).Column("driver_of_car_own_type").CustomType<CarOwnTypeStringType>();
				Map(x => x.Gender).Column("gender").CustomType<GenderStringType>();
				Map(x => x.MinRouteAddresses).Column("min_route_addresses");
				Map(x => x.MaxRouteAddresses).Column("max_route_addresses");
				Map(x => x.DriverType).Column("driver_type").CustomType<DriverTypeStringType>();
				Map(x => x.SkillLevel).Column("skill_level");
				Map(x => x.Comment).Column("comment");

				References(x => x.Subdivision).Column("subdivision_id");
				References(x => x.User).Column("user_id");
				References(x => x.DefaultForwarder).Column("default_forwarder_id");
				References(x => x.OrganisationForSalary).Column("organisation_for_salary_id");
				References(x => x.Post).Column("employees_posts_id");

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
			}
		}

		public class TraineeMap : SubclassMap<Trainee>
		{
			public TraineeMap()
			{
				DiscriminatorValue(EmployeeType.Trainee.ToString());
			}
		}
	}
}
