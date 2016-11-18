using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlRepositories.Test.Shared.Entities
{
    public class TestExtraEltEntity : TestEntity
    {
        [JsonExtensionData] // work for JSON.Net and Elasticsearch
        public Dictionary<string, object> ExtraElements { get; set; } = new Dictionary<string, object>();

        public TestEntity CloneToTestEntity()
        {
            return new TestEntity()
            {
                Birthday = this.Birthday,
                Childs = this.Childs.ToList(),
                Deleted = this.Deleted,
                Id = this.Id,
                IsAMan = this.IsAMan,
                Name = this.Name,
                NumberOfChildenInt = this.NumberOfChildenInt,
                NumberOfChildenLong = this.NumberOfChildenLong,
                PoidsDouble = this.PoidsDouble,
                PoidsFloat = this.PoidsFloat,
                SystemCreationDate = this.SystemCreationDate,
                SystemLastUpdateDate = this.SystemLastUpdateDate
            };
        }
    }
}
