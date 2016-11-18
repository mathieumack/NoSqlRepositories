using NoSqlRepositories.Test.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlRepositories.Test.Shared.Helpers
{
    public class TestHelper
    {

        internal static TestEntity GetEntity1()
        {
            return new TestEntity
            {
                Birthday = new DateTime(1985, 08, 12, 0, 0, 0, DateTimeKind.Utc),
                IsAMan = true,
                Name = "Balan",
                PoidsFloat = 70.15F,
                PoidsDouble = 70.15,
                NumberOfChildenInt = 5,
                NumberOfChildenLong = 9999999999999999,
                Cities = new List<string> { "Andernos", "Grenoble" }
            };
        }

        internal static TestExtraEltEntity GetEntity2()
        {
            return new TestExtraEltEntity
            {
                Birthday = new DateTime(2000, 06, 12, 23, 59, 50, DateTimeKind.Utc),
                IsAMan = false,
                Name = "Mack",
                PoidsFloat = 10.15F,
                PoidsDouble = 10.15,
                NumberOfChildenInt = 0,
                NumberOfChildenLong = 0,
                ExtraElements = new Dictionary<string, object> {
                    { "subfield1", "test" },
                    { "subfield2", 1 }
                },
                Cities = new List<string> { "Grenoble", "Bordeaux" }
            };
        }

        internal static TestEntity GetEntity3()
        {
            return new TestEntity
            {
                Birthday = new DateTime(2010, 06, 12, 23, 59, 50, DateTimeKind.Utc),
                IsAMan = true,
                Name = "Mareuil",
                PoidsFloat = 200F,
                PoidsDouble = 200,
                NumberOfChildenInt = 0,
                NumberOfChildenLong = 0,
                Cities = new List<string> { "Grenoble"}
            };
        }

        internal static TestEntity GetEntity4()
        {
            return new TestEntity
            {
                Birthday = new DateTime(2010, 06, 12, 12, 59, 50, DateTimeKind.Utc),
                IsAMan = true,
                Name = "Daude",
                PoidsFloat = 190F,
                PoidsDouble = 40,
                NumberOfChildenInt = 10,
                NumberOfChildenLong = 1
            };
        }
    }
}
