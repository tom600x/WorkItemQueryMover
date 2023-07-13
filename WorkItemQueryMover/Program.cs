using Microsoft.Extensions.Configuration;
 

namespace WorkItemQueryMover
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Query Mover Starting at " + DateTime.Now.ToString() + "...");    
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile($"appSettings.json").Build();
 
           var UserTokens = Config.GetSection("UserTokens").Get<UserToken[]>();   

            

            foreach(var userToken in UserTokens)
            {
                Console.WriteLine("Processing queries for user: " + userToken.UserName);
                DevOpsQueryMover devOpsQueryMover = new DevOpsQueryMover(Config, userToken.SourceToken, userToken.DestinationToken);
                await devOpsQueryMover.ProcessQueries("");
                Console.WriteLine("Finished processing queries for user: " + userToken.UserName);
            }   
     

            Console.WriteLine("Query Mover Finished processing at " + DateTime.Now.ToString() );

        }
    }
}