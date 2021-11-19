//using Dummy;
using Greet;
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
        static void Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);
            channel.ConnectAsync().ContinueWith((task) => 
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("This client connected successfully, man");

                Console.WriteLine(task.Status);
            });

            //CallDummyService(channel);
            //CallGreetService(channel);
            CallCalculateService(channel);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
        /*
        private static void CallDummyService(Channel channel) 
        {
            var client = new DummyService.DummyServiceClient(channel);
        }
        */

        private static void CallGreetService(Channel channel)
        {
            var client = new GreetingService.GreetingServiceClient(channel);
            var request = new GreetingRequest() { Greeting = new Greeting() { FirstName = "George", LastName = "Costanza" } };
            var response = client.Greet(request);
            Console.WriteLine(response.Result);
        }

        private static void CallCalculateService(Channel channel)
        {
            var client = new CalculatorService.CalculatorServiceClient(channel);

            var request = new SumRequest()
            {
                Value1 = 3,
                Value2 = 5
            };

            var response = client.CalculateSum(request);

            Console.WriteLine($"Calculations are completed: {response.Result}");
            
        }
    }
}
