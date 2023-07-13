 

namespace WorkItemQueryMover
{
    public class ADOQueryObject
    {
        public int count { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public bool isFolder { get; set; }
        public bool hasChildren { get; set; }
        public Child[] children { get; set; }
        public bool isPublic { get; set; }
    }

    public class Child
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public bool isFolder { get; set; }
        public bool hasChildren { get; set; }
        public bool isPublic { get; set; }
        public string queryType { get; set; }
        public string wiql { get; set; }
    }

    public class UserToken
    {
        public string UserName { get;set; }
        public string SourceToken { get; set; } 
        public string DestinationToken { get; set; } 

    }


}
