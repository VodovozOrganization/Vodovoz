using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Employees
{
	public class PersonnelMap : ClassMap<Personnel>
	{
		public PersonnelMap()
		{
			Table("employees");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("employee_type");
			Map(x => x.EmployeeType).Column("employee_type").CustomType<EmployeeTypeStringType>().Update().Not.Insert();

			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.Name).Column("name");
			Map(x => x.LastName).Column("last_name");
			Map(x => x.Patronymic).Column("patronymic");
			Map(x => x.DrivingNumber).Column("driving_number");
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
		}

		public class EmployeeMap : SubclassMap<Employee>
		{
			public EmployeeMap()
			{
				DiscriminatorValue(EmployeeType.Employee.ToString());

				Map(x => x.Category).Column("category").CustomType<EmployeeCategoryStringType>();
				Map(x => x.IsFired).Column("is_fired");
				Map(x => x.AndroidLogin).Column("android_login");
				Map(x => x.AndroidPassword).Column("android_password");
				Map(x => x.AndroidSessionKey).Column("android_session_key");
				Map(x => x.AndroidToken).Column("android_token");
				Map(x => x.FirstWorkDay).Column("first_work_day");
				Map(x => x.TripPriority).Column("priority_for_trip");
				Map(x => x.DriverSpeed).Column("driver_speed");
				Map(x => x.WageCalcType).Column("wage_calc_type");
				Map(x => x.WageCalcRate).Column("wage_calc_rate");
				Map(x => x.VisitingMaster).Column("visiting_master");
				Map(x => x.DriverOf).Column("driver_of").CustomType<CarTypeOfUseStringType>();
				Map(x => x.Registration).Column("registration_type").CustomType<RegistrationTypeStringType>();

				References(x => x.Subdivision).Column("subdivision_id");
				References(x => x.User).Column("user_id");
				References(x => x.DefaultDaySheldule).Column("default_delivery_day_schedule_id");
				References(x => x.DefaultForwarder).Column("default_forwarder_id");

				HasMany(x => x.Districts).Cascade.AllDeleteOrphan().Inverse()
										 .KeyColumn("driver_id")
										 .AsList(x => x.Column("priority"));

				HasMany(x => x.Contracts).Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
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
