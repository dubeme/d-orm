using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DamnORM.Helpers;
using DamnORM.Model.Exceptions;
using DamnORM.Services;
using DamnORM.unit.tests.Model.POCOs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DamnORM.Model;

namespace DamnORM.unit.tests.Services
{
    [TestClass]
    public class SqlServerServiceTest
    {
        private const int NUMBER_OF_RECORDS_ON_SQL_FILE = 9;
        private const int NUMBER_OF_PEOPLE_UNDER_21 = 5;
        private const string TEST_SELECT = "Perform T-SQL SELECT";
        private const string TEST_INSERT = "Perform T-SQL INSERT";
        private const string TEST_UPDATE = "Perform T-SQL UPDATE";
        private const string TEST_RAW_SQL = "Execute raw T-SQL";
        private const string TEST_STORED_PROC = "Call SQL server stored procedure";
        private const string STORE_PROC_NAME = "select_all";

        private readonly string LONG_NAME = new string('-', short.MaxValue);

        private const string CONNECTION_STRING = @"
            Server=(localdb)\MSSQLLocalDB;
            Integrated Security=True;
            Database=test_db;
            Connect Timeout=15;
            Encrypt=False;
            TrustServerCertificate=False;";

        [TestInitialize]
        public void PerTestStartUp()
        {
            var script = File.ReadAllText(@"..\..\test.sql");
            var conn = new SqlConnection(CONNECTION_STRING);
            var server = new Server(new ServerConnection(conn));

            server.ConnectionContext.ExecuteNonQuery(script);
        }

        [TestMethod]
        [TestCategory(TEST_SELECT)]
        public void TestSelect_GetTopRecord()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.Select<Person>();

            Assert.AreEqual("Paul", res.FirstName);
            Assert.AreEqual("Jacobs", res.LastName);
            Assert.AreEqual("M", res.Gender);
            Assert.AreEqual(3, res.Age);
        }

        [TestMethod]
        [TestCategory(TEST_SELECT)]
        public void TestSelect_WithWhere()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.Select<Person>(p => p.Age == 44);

            Assert.AreEqual("John", res.FirstName);
            Assert.AreEqual("Doe", res.LastName);
            Assert.AreEqual("F", res.Gender);
            Assert.AreEqual(44, res.Age);
        }

        [TestMethod]
        [TestCategory(TEST_SELECT)]
        public void TestSelect_WithWhere_2Expressions()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.Select<Person>(p => p.Age == 2 || p.Age == 59);

            Assert.AreEqual("Jenna", res.FirstName);
            Assert.AreEqual("Pink", res.LastName);
            Assert.AreEqual("F", res.Gender);
            Assert.AreEqual(59, res.Age);
        }

        [TestMethod]
        [TestCategory(TEST_SELECT)]
        public void TestSelectMany_WithWhere_Under21()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.SelectMany<Person>(int.MaxValue, p => p.Age >= 0 && p.Age < 21).ToArray();

            Assert.AreEqual(NUMBER_OF_PEOPLE_UNDER_21, res.Length);
        }

        [TestMethod]
        [TestCategory(TEST_SELECT)]
        public void TestSelectMany_WithWhereGroup_Under21()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.SelectMany<Person>(int.MaxValue, p => p.Age >= 0 && p.Age < 21).ToArray();

            Assert.AreEqual(NUMBER_OF_PEOPLE_UNDER_21, res.Length);
        }

        [TestMethod]
        [TestCategory(TEST_INSERT)]
        public void TestInsert_OneRecord()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.Insert(new Person
            {
                ID = 100,
                Age = 23,
                FirstName = "Didier",
                LastName = "Drogba",
                Gender = "M"
            });

            Assert.AreEqual(NUMBER_OF_RECORDS_ON_SQL_FILE + 1, res.ID);

            res = sqlService.Select<Person>(p => p.ID == 100);

            Assert.AreEqual(res, default(Person));
        }

        [TestMethod]
        [TestCategory(TEST_INSERT)]
        [ExpectedException(typeof(MaxLengthException))]
        public void TestInsert_OneRecord_FirstNameTooLong()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.Insert(new Person
            {
                ID = 100,
                Age = 23,
                FirstName = "Didier" + LONG_NAME,
                LastName = "Drogba",
                Gender = "M"
            });
        }

        [TestMethod]
        [TestCategory(TEST_INSERT)]
        public void TestInsert_ManyRecords()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var rand = new Random();

            var people = Enumerable.Range(0, 10).Select(index => new Person
            {
                ID = index,
                Age = rand.Next() % 100,
                FirstName = "Didier_" + index,
                LastName = "Drogba" + index,
                Gender = rand.Next() % 2 == 0 ? "M" : "F"
            }).ToArray();

            var inserted = sqlService.InsertMany(people);
            Assert.AreEqual(people.Length, inserted.Count());

            var selectResult = sqlService.SelectMany<Person>(int.MaxValue, p => p.FirstName.Contains("Didier_"));
            Assert.AreEqual(people.Length, selectResult.Count());
        }

        [TestMethod]
        [TestCategory(TEST_STORED_PROC)]
        public void TestStoredProcedure()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var res = sqlService.InvokeStoredProcedure<Person>(STORE_PROC_NAME, null).ToArray();

            Assert.AreEqual(NUMBER_OF_RECORDS_ON_SQL_FILE, res.Length);

            Assert.AreEqual("Paul", res[0].FirstName);
            Assert.AreEqual("Jacobs", res[0].LastName);
            Assert.AreEqual("M", res[0].Gender);
            Assert.AreEqual(3, res[0].Age);
        }

        [TestMethod]
        [TestCategory(TEST_UPDATE)]
        public void TestUpdate_OneRecord()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var person = new Person
            {
                ID = 100,
                Age = 23,
                FirstName = "Paula",
                LastName = "Podolski",
                Gender = "G"
            };

            var affectedRowCount = sqlService.Update(person, p => p.FirstName.Contains("Paula"));

            Assert.AreEqual(1, affectedRowCount);

            person = sqlService.Select<Person>(p => p.FirstName.Contains("Paula"));

            Assert.AreEqual("Paula", person.FirstName);
            Assert.AreEqual("Podolski", person.LastName);
            Assert.AreEqual("G", person.Gender);
            Assert.AreEqual(23, person.Age);
            Assert.AreEqual(4, person.ID);
        }

        [TestMethod]
        [TestCategory(TEST_UPDATE)]
        [ExpectedException(typeof(MaxLengthException))]
        public void TestUpdate_OneRecord_LastNameTooLong()
        {
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var person = new Person
            {
                ID = 100,
                Age = 23,
                FirstName = "Paula",
                LastName = "Podolski" + LONG_NAME,
                Gender = "G"
            };

            var affectedRowCount = sqlService.Update(person, p => p.FirstName.Contains("Paula"));
        }

        [TestMethod]
        [TestCategory(TEST_UPDATE)]
        public void TestUpdate_ManyRecords_Under21()
        {
            var noobs = "Noobs";
            var sqlService = new SqlServerService
            {
                ConnectionString = CONNECTION_STRING
            };

            var cols = new Dictionary<string, object>
            {
                { SqlHelper.GetColumnName<Person>(p => p.FirstName), noobs}
            };

            var affectedRowCount = sqlService.UpdateColumns<Person>(cols, p => p.Age >= 0 && p.Age < 21);
            Assert.AreEqual(NUMBER_OF_PEOPLE_UNDER_21, affectedRowCount);

            var peeps = sqlService.SelectMany<Person>(int.MaxValue, p => p.FirstName.Contains(noobs)).ToArray();

            Assert.AreEqual(NUMBER_OF_PEOPLE_UNDER_21, peeps.Length);
        }
    }
}