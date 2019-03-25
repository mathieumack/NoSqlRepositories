using Couchbase.Lite;

namespace NoSqlRepositories.CouchBaseLite
{
    internal class NoSqlEntityDocument
    {
        private bool isMutable = false;
        private readonly Document document;
        private MutableDocument mutableDocument;

        public Document Document { get { return document; } }

        public MutableDocument MutableDocument { get { return mutableDocument; } }

        public NoSqlEntityDocument(Document document)
        {
            this.document = document;
            isMutable = false;
        }

        public NoSqlEntityDocument(MutableDocument mutableDocument)
        {
            this.mutableDocument = mutableDocument;
            isMutable = true;
        }

        public bool CheckMutable()
        {
            if (!isMutable)
            {
                mutableDocument = document.ToMutable();
                isMutable = true;
            }
            return isMutable;
        }
    }
}
