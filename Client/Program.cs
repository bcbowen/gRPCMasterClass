using Dummy;
//using Greet;
using Calculator;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        const string target = "127.0.0.1:50051";
        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);
            channel.ConnectAsync().ContinueWith((task) => 
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("This client connected successfully, man");

                Console.WriteLine(task.Status);
            });

            //var client = new DummyService.DummyServiceClient(channel);
            //var client = new GreetingService.GreetingServiceClient(channel);
            var client = new CalculatorService.CalculatorServiceClient(channel);

            var request = new Request() 
            {
                Value1 = 3, 
                Value2 = 5
            };

            //var request = new GreetingRequest() { Greeting = greeting };
            //var response = client.Greet(request);
            var response = client.Calculate(request);

            Console.WriteLine($"Calculations are completed: {response.Result}"); 

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}
