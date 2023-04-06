using ReportAppAPI.Models;

namespace ReportAppAPI.Services
{
    public class JsonDbService
    {
        public void ThisATest(List<Module> modules)
        {
            foreach (var module in modules)
            {
                Console.WriteLine(module.Title);
            }
            Console.WriteLine("All ok ig");
        }
    }
}
