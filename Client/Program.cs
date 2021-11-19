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
        static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);
            channel.ConnectAsync().ContinueWith((task) => 
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("This client connected successfully, man");

                Console.WriteLine(task.Status);
            });

            //CallDummyService(channel);
            await CallGreetService(channel);
            //CallCalculateService(channel);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
        /*
        private static void CallDummyService(Channel channel) 
        {
            var client = new DummyService.DummyServiceClient(channel);
        }
        */

        private static async Task CallGreetService(Channel channel)
        {
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting() { FirstName = "George", LastName = "Costanza" };
            var greetUnaryRequest = new GreetingRequest() { Greeting = greeting };
            var greetUnaryResponse = client.Greet(greetUnaryRequest);
            Console.WriteLine($"Unary response: {greetUnaryResponse.Result}");

            var greetStreamRequest = new GreetingStreamRequest() { Greeting = greeting };
            var greetStreamResponse = client.GreetStream(greetStreamRequest);
            //Console.WriteLine($"Unary response: {greetStreamResponse.Result}");
            while (await greetStreamResponse.ResponseStream.MoveNext()) 
            {
                Console.WriteLine(greetStreamResponse.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
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
