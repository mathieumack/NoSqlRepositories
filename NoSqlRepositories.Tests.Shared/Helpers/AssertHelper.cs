using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace NoSqlRepositories.Tests.Shared.Helpers
{
    public static class AssertHelper
    {
        public static void LookLikeEachOther(object a, object b)
        {
            Type typeA = a.GetType();
            Type typeB = b.GetType();

            Assert.AreEqual(typeA, typeB, "The types of instances a and b are not the same.");

            PropertyInfo[] myProperties = typeA.GetProperties(BindingFlags.DeclaredOnly
                                | BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo myPropertyA in myProperties)
            {
                PropertyInfo myPropertyB = typeB.GetProperty(myPropertyA.Name);
                Assert.IsNotNull(myPropertyB, string.Format(@"The property {0} from instance a 
           was not found on instance b.", myPropertyA.Name));

                Assert.AreEqual<Type>(myPropertyA.PropertyType, myPropertyB.PropertyType,
                       string.Format(@"The type of property {0} on instance a is different from 
           the one on instance b.", myPropertyA.Name));

                Assert.AreEqual(myPropertyA.GetValue(a, null), myPropertyB.GetValue(b, null),
                       string.Format(@"The value of the property {0} on instance a is different from 
           the value on instance b.", myPropertyA.Name));
            }
        }

        public static void AreJsonEqual(object expected, object actual, bool checkType = true, string ErrorMsg=null)
        {
            if (expected == null) Assert.Fail("expected object is null");
            if (actual == null) Assert.Fail("actual object is null");

            if(checkType)
            {
                Type typeA = expected.GetType();
                Type typeB = actual.GetType();

                Assert.AreEqual(typeA, typeB, 
                    (ErrorMsg != null ? ErrorMsg : string.Format("Type mismatch : expecting '{0}' and found '{1}'", typeA.Name, typeB.Name)));
            }

            // Check field to field
            foreach(var field in expected.GetType().GetProperties().Where( p => p.PropertyType.IsValueType))
            {
                Assert.AreEqual(field.GetValue(expected), field.GetValue(actual)
                    , (ErrorMsg != null ? ErrorMsg : string.Format("Field '{0}' is different", field.Name)));
            }

            // Global check with json comparison
            var jsonCompare = JToken.DeepEquals(
                JToken.FromObject(expected),
                JToken.FromObject(actual)
            );
        
            Assert.IsTrue(jsonCompare, "Objects are not equals");
        }
    }
}
