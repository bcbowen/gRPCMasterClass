//using Dummy;
using Greet;
using Calculator;
using Primes;
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
            //await CallGreetService(channel);
            await CallCalculateService(channel);
            //await CallPrimesService(channel);

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
            string separator = new String('*', 50) + Environment.NewLine;
            var client = new GreetingService.GreetingServiceClient(channel);
            var greeting = new Greeting() { FirstName = "George", LastName = "Costanza" };
            var greetUnaryRequest = new GreetingRequest() { Greeting = greeting };
            var greetUnaryResponse = client.Greet(greetUnaryRequest);
            Console.WriteLine($"Unary response: {greetUnaryResponse.Result}");

            var greetRequest = new GreetingRequest() { Greeting = greeting };
            var greetStreamResponse = client.GreetLongResponse(greetRequest);
            Console.WriteLine(separator);

            Console.WriteLine("Stream response: ");
            while (await greetStreamResponse.ResponseStream.MoveNext()) 
            {
                Console.WriteLine(greetStreamResponse.ResponseStream.Current.Result);
                await Task.Delay(20);
            }
            Console.WriteLine("End of stream");
            Console.WriteLine(separator);

            var stream = client.GreetLongRequest();
            await stream.RequestStream.WriteAsync(new GreetingStreamRequest { Key = "first_name", Value = "Frank" });
            await stream.RequestStream.WriteAsync(new GreetingStreamRequest { Key = "last_name", Value = "Costanza" });
            await stream.RequestStream.CompleteAsync();
            var response = await stream.ResponseAsync;
            Console.WriteLine($"Client stream response: {response.Result}");
            Console.WriteLine(separator);
        }

        private static async Task CallCalculateService(Channel channel)
        {
            var client = new CalculatorService.CalculatorServiceClient(channel);

            var request = new SumRequest()
            {
                Value1 = 3,
                Value2 = 5
            };

            var sumResponse = client.CalculateSum(request);

            Console.WriteLine($"The sum is: {sumResponse.Result}");

            int[] numbers = { 1, 2, 3, 4};

            var averageStream = client.CalculateAverage();
            foreach (int value in numbers) 
            {
                await averageStream.RequestStream.WriteAsync(new AverageRequest { Value = value });
            }
            await averageStream.RequestStream.CompleteAsync();
            var avgResponse = await averageStream.ResponseAsync;

            Console.WriteLine($"The average is: {avgResponse.Result}");
        }

        private static async Task CallPrimesService(Channel channel)
        {
            var client = new PrimesService.PrimesServiceClient(channel);
            List<int> values = new List<int> { 120, 210, 59};
            foreach (int value in values) 
            {
                Console.WriteLine($"Factoring {value}");
                var primesRequest = new PrimesRequest() { Value = value };
                var primesResponse = client.CalculatePrimes(primesRequest);

                while (await primesResponse.ResponseStream.MoveNext())
                {
                    Console.WriteLine(primesResponse.ResponseStream.Current.Factor);
                    await Task.Delay(20);
                }
                Console.WriteLine("...");
                Console.WriteLine("");
            }
            
        }
    }
}
