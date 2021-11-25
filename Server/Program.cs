using Calculator;
using Greet;
using Primes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    class Program
    {
        private static SslServerCredentials _credentials; 
        const int Port = 50051;
        static void Main(string[] args)
        {
            var serverCert = File.ReadAllText("ssl/server.crt");
            var serverKey = File.ReadAllText("ssl/server.key");
            var keyPair = new KeyCertificatePair(serverCert, serverKey);
            var caCert = File.ReadAllText("ssl/ca.crt");
            _credentials = new SslServerCredentials(new List<KeyCertificatePair> { keyPair }, caCert, true);

            RunGreetService();
            //RunCalculateService();            
            //RunPrimesService();
        }

        static void RunCalculateService()
        {
            Grpc.Core.Server server = new Grpc.Core.Server()
            {
                Services = { CalculatorService.BindService(new CalculateServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, _credentials) }
            };
            RunGrpcService(server);
        }

        static void RunGreetService()
        {
            Grpc.Core.Server server = new Grpc.Core.Server()
            {
                Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, _credentials) }
            };
            RunGrpcService(server);
        }

        static void RunPrimesService()
        {
            Grpc.Core.Server server = new Grpc.Core.Server()
            {
                Services = { PrimesService.BindService(new PrimesServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, _credentials) }
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
