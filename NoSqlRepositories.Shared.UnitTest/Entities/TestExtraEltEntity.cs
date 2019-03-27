using System.Linq;

namespace NoSqlRepositories.Test.Shared.Entities
{
    public class TestExtraEltEntity : TestEntity
    {
        // TODO : Add management of extra elements in a next release
        //[JsonExtensionData] // work for JSON.Net and Elasticsearch
        //public Dictionary<string, object> ExtraElements { get; set; } = new Dictionary<string, object>();

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
