//using Greet;
using Calculator;
using Grpc.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        const int Port = 50051;
        static void Main(string[] args)
        {
            Grpc.Core.Server server = null;
            try
            {
                /*
                server = new Grpc.Core.Server()
                {
                    Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };
                */
                server = new Grpc.Core.Server()
                {
                    Services = { CalculatorService.BindService(new CalculateServiceImpl()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine($"The server is listening on port: {Port}");
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine("The server failed to start: " + e.Message);
                throw;
            }
            finally 
            {
                if (server != null)
                    server.ShutdownAsync().Wait();
            }
        }
    }
}
