using Calculator;
using Greet;
using Grpc.Core;
using System;
using System.IO;

namespace Server
{
    class Program
    {
        const int Port = 50051;
        static void Main(string[] args)
        {
            RunGreetService();
            //RunCalculateService();            
        }

        static void RunCalculateService()
        {
            Grpc.Core.Server server = new Grpc.Core.Server()
            {
                Services = { CalculatorService.BindService(new CalculateServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            RunGrpcService(server);
        }

        static void RunGreetService()
        {
            Grpc.Core.Server server = new Grpc.Core.Server()
            {
                Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            RunGrpcService(server);
        }

        private static void RunGrpcService(Grpc.Core.Server server) 
        {
            try
            {
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
