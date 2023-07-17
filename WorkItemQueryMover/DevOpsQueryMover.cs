
 
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace WorkItemQueryMover
{
    internal class DevOpsQueryMover
    {
        private HttpClient _client = new HttpClient();
        private IConfiguration _config;
        private string _sourceUrl;
        private string _destinationUrl;
        private string _sourceToken;
        private string _destinationToken;
        private string _sourceProjectName;
        private string _destinationProjectName;
        private string _apiVersion;


        public DevOpsQueryMover(IConfiguration config, string sourceToken, string destinationToken)
        {
            _config = config;
            _sourceUrl = _config["SourceUrl"];
            _sourceToken = sourceToken;

            _destinationUrl = _config["DestinationUrl"];
            _destinationToken = destinationToken;

            _sourceProjectName = _config["SourceProjectName"];  
            _destinationProjectName = _config["DestinationProjectName"];
            _apiVersion = _config["ApiVersion"];
        }

        private async Task<bool>  CreateIndividualQueryFolder(string parentFolderPath, string folderName)
        {

            if (folderName == "My Queries")
            {
                return true;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _destinationUrl +  @"/_apis/wit/queries/" + parentFolderPath + _apiVersion);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + _destinationToken)));

            string s = "{\r\n  \"name\" : \"" + folderName + "\",\r\n  \"isFolder\": true \r\n}";

            var content = new StringContent(s, null, "application/json");
            request.Content = content;
            var response = await _client.SendAsync(request);

               if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    await Console.Out.WriteLineAsync("Folder created: " + parentFolderPath + "/" + folderName);
                    return true;
                }
                else
                {
                    await Console.Out.WriteLineAsync("Error created: " + parentFolderPath + "/" + folderName );
                    await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
                return false;
                 }
      
 
        }

        private async Task<bool> CreateFolders(string path)
        {
            var pathArray = path.Split("/");
          
            string parentFolderPath = "";

            foreach (var p in pathArray)
            {                
                CreateIndividualQueryFolder(parentFolderPath, p);
                parentFolderPath = parentFolderPath + "/" + p;  
            }

            return true;

        }
        private async Task<bool> CreateQueryOnTarget(string queryName, string queryPath, string queryWIQL)
        {


            await Console.Out.WriteLineAsync("Processing query: " + queryName);
            var request = new HttpRequestMessage(HttpMethod.Post, _destinationUrl +  "/_apis/wit/queries/" + queryPath + _apiVersion);
        
           _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + _destinationToken)));
 
            QueryCreate queryCreate = new QueryCreate();
            queryCreate.name = queryName;
            queryCreate.wiql = queryWIQL.Replace("'" + _sourceProjectName + "'", "'" + _destinationProjectName + "'");

            string s = JsonSerializer.Serialize(queryCreate);
          //  string s = "{\r\n  \"name\" : \"" + queryName + "\",\r\n  \"wiql\":  \"" + queryWIQL.Replace("'" + _sourceProjectName + "'", "'" + _destinationProjectName + "'") + "\" \r\n}";

            var content = new StringContent(s, null, "application/json");
            request.Content = content;
            var response = await _client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                await Console.Out.WriteLineAsync("Query created: " + queryName + " in path " + queryPath);
                return true;
            }
            else
            {
                await Console.Out.WriteLineAsync("Error creating Query: " + queryName + " in path " + queryPath);
                await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
                return false;
            }
  
        }

        public async Task<bool> ProcessQueries(string id)
        {

 
            var request = new HttpRequestMessage(HttpMethod.Get, _sourceUrl + "/_apis/wit/queries/" + id + "?$depth=1&$expand=minimal");
 
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + _sourceToken)));

            var response = await _client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                
                var responseBody = await response.Content.ReadAsStringAsync();
                //if (!responseBody.Contains(@"""count"""))
                //    responseBody =   "{\"count\":1,\"value\": [ {" + responseBody.Substring(1, (responseBody.Length) - 1) + "] }";

               


                ADOQueryObject? queries =  new ADOQueryObject();
                queries = JsonSerializer.Deserialize<ADOQueryObject>(responseBody);

                if (queries != null)
                {
                    if (queries.count == 0)
                    {
                        queries.count = 1;
                        queries.value = new Value[1];
                        queries.value[0] =   JsonSerializer.Deserialize<Value>(responseBody);
                    }

                }


                foreach (var item in queries.value)
                {
                    await CreateFolders(item.path);
          
                    foreach(var child in item.children)
                    {
                        
                        if (!child.isPublic && !child.hasChildren && child.isFolder)
                             await CreateIndividualQueryFolder(item.path, child.name);
                        else if (!child.isPublic && child.hasChildren && child.isFolder)
                        {

                            await ProcessQueries(child.id);

                        }

                        if (child.wiql != null && !child.isPublic)
                            await CreateQueryOnTarget(child.name, item.path, child.wiql);                         

                    }
            

                    
                }
            }
            else
            {
                await Console.Out.WriteLineAsync("Error: " + response);
            }

            return true;
        }
    }
}
