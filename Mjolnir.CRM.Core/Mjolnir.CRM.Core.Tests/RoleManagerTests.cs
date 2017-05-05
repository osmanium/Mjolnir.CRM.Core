﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mjolnir.CRM.Core.EntityManagers;
using Mjolnir.CRM.Sdk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mjolnir.CRM.Sdk.Extensions;

namespace Mjolnir.CRM.Core.Tests
{
    [TestClass]
    public class RoleManagerTests : CrmUnitTestBase
    {
        [TestMethod]
        public void should_get_role_by_name()
        {
            var roleEntityManager = new RoleManager(SourceCrmContext);

            var task = roleEntityManager.GetRoleByNameAsync("System Administrator");
            task.Wait();

            var adminRole = task.Result;

            Assert.IsNotNull(adminRole);
        }

        [TestMethod]
        public void should_get_all_roles()
        {
            var roleEntityManager = new RoleManager(SourceCrmContext);

            var task = roleEntityManager.GetAllRootLevelRolesAsync();
            task.Wait();

            var roles = task.Result;

            Assert.IsNotNull(roles);
            Assert.IsTrue(roles.Any());

        }


        [TestMethod]
        public void should_compare_entity_records_same()
        {
            var roleEntityManager = new RoleManager(SourceCrmContext);

            var task = roleEntityManager.GetRoleByNameAsync("System Administrator");
            var task2 = roleEntityManager.GetRoleByNameAsync("System Administrator");

            task.Wait();
            task2.Wait();
            
            var adminRole = task.Result;
            var adminRole2 = task2.Result;

            Assert.IsTrue(adminRole.CompareValues(adminRole2).IsEqual);
        }

        [TestMethod]
        public void should_compare_entity_records_different()
        {
            var roleEntityManager = new RoleManager(SourceCrmContext);

            var task = roleEntityManager.GetRoleByNameAsync("System Administrator");
            var task2 = roleEntityManager.GetRoleByNameAsync("System Customizer");

            task.Wait();
            task2.Wait();

            var adminRole = task.Result;
            var adminRole2 = task2.Result;

            Assert.IsFalse(adminRole.CompareValues(adminRole2).IsEqual);
        }
    }
}
