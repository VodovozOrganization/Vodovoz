﻿using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
    public class EmployeePostMap : ClassMap<EmployeePost>
    {
        public EmployeePostMap()
        {
            Table("employees_posts");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Name).Column("name");
        }
    }
}