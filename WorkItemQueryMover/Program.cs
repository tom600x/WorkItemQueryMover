using Microsoft.Extensions.Configuration;


namespace WorkItemQueryMover
{
    internal class Program
    {
        

        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync("Query Mover Starting at " + DateTime.Now.ToString() + "...");
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile($"appSettings.json").Build();

            var UserTokens = Config.GetSection("UserTokens").Get<UserToken[]>();



            foreach (var userToken in UserTokens)
            {
                await Console.Out.WriteLineAsync("Processing queries for user: " + userToken.UserName);
                DevOpsQueryMover devOpsQueryMover = new DevOpsQueryMover(Config, userToken.SourceToken, userToken.DestinationToken);
                await devOpsQueryMover.ProcessQueries("");
                await Console.Out.WriteLineAsync("Finished processing queries for user: " + userToken.UserName);
            }


            await Console.Out.WriteLineAsync("Query Mover Finished processing at " + DateTime.Now.ToString());

        }
    }
}