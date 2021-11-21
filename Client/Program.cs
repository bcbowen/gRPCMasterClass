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
            List<GreetingRequest> requests = GetRandomNames();
            string separator = new String('*', 50) + Environment.NewLine;
            var client = new GreetingService.GreetingServiceClient(channel);
            var greetUnaryRequest = requests[0];
            var greetUnaryResponse = client.Greet(greetUnaryRequest);
            Console.WriteLine($"Unary response: {greetUnaryResponse.Result}");

            var greetStreamResponse = client.GreetLongResponse(requests[1]);
            Console.WriteLine(separator);

            Console.WriteLine("Stream response: ");
            while (await greetStreamResponse.ResponseStream.MoveNext()) 
            {
                Console.WriteLine(greetStreamResponse.ResponseStream.Current.Result);
                await Task.Delay(20);
            }
            Console.WriteLine("End of stream");
            Console.WriteLine(separator);

            var streamingClientRequest = client.GreetLongRequest();
            await streamingClientRequest.RequestStream.WriteAsync(new GreetingStreamRequest { Key = "first_name", Value = requests[2].Greeting.FirstName });
            await streamingClientRequest.RequestStream.WriteAsync(new GreetingStreamRequest { Key = "last_name", Value = requests[2].Greeting.LastName });
            await streamingClientRequest.RequestStream.CompleteAsync();
            var response = await streamingClientRequest.ResponseAsync;
            Console.WriteLine($"Client stream response: {response.Result}");
            Console.WriteLine(separator);

            Console.WriteLine("Bidirectional stream: ");
            var bidirectionalRequest = client.GreetBidirectional();

            var responseReaderTask = Task.Run(async () => {
                while (await bidirectionalRequest.ResponseStream.MoveNext()) 
                {
                    Console.WriteLine($"Received: {bidirectionalRequest.ResponseStream.Current.Result}"); 
                }
            });

            foreach (GreetingRequest request in requests) 
            {
                await bidirectionalRequest.RequestStream.WriteAsync(request);
                Console.WriteLine($"Writing {request.Greeting.FirstName} {request.Greeting.LastName} to stream");
            }

            await bidirectionalRequest.RequestStream.CompleteAsync();

            Console.WriteLine("Bidirectional stream done writing ");
            Console.WriteLine(separator);

            await responseReaderTask;
            
            Console.WriteLine("Bidirectional stream done reading ");
            Console.WriteLine(separator);
        }

        private static List<GreetingRequest> GetRandomNames() 
        {
            List<GreetingRequest> names = new List<GreetingRequest>();
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Jermaine", LastName = "Boone" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Jason", LastName = "Roach" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Hugh", LastName = "Gould" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Savanah", LastName = "Black" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Juliet", LastName = "Lucero" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Elijah", LastName = "Graham" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Dakota", LastName = "Acosta" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Brooklynn", LastName = "Montoya" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Aylin", LastName = "Stanley" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Jacoby", LastName = "Odom" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Maddison", LastName = "Norris" } });
            names.Add(new GreetingRequest { Greeting = new Greeting { FirstName = "Jeremy", LastName = "Greer" } });

            return names;
        }

        private static async Task CallCalculateService(Channel channel)
        {
            var client = new CalculatorService.CalculatorServiceClient(channel);

            Console.WriteLine("Calculate Sum: ");
            var request = new SumRequest()
            {
                Value1 = 3,
                Value2 = 5
            };

            var sumResponse = client.CalculateSum(request);

            Console.WriteLine($"The sum is: {sumResponse.Result}");

            Console.WriteLine("Calculate Average: ");
            int[] numbers = { 1, 2, 3, 4};

            var averageStream = client.CalculateAverage();
            foreach (int value in numbers) 
            {
                await averageStream.RequestStream.WriteAsync(new AverageRequest { Value = value });
            }
            await averageStream.RequestStream.CompleteAsync();
            var avgResponse = await averageStream.ResponseAsync;

            Console.WriteLine($"The average is: {avgResponse.Result}");

            Console.WriteLine("Find max in stream:");

            var maxStream = client.FindMaximum();
            numbers = new int[] {1, 5, 3, 6, 2, 20};

            var responseReaderTask = Task.Run(async () => {
                while (await maxStream.ResponseStream.MoveNext()) 
                {
                    Console.WriteLine($"Current max is {maxStream.ResponseStream.Current.Result}");
                }
            });

            foreach (int value in numbers) 
            {
                Console.WriteLine($"Sending {value}");
                await maxStream.RequestStream.WriteAsync(new FindMaximumRequest { Value = value });
                await Task.Delay(20);
            }
            Console.WriteLine("Done sending"); 

            await responseReaderTask;
            Console.WriteLine("Done receiving"); 
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
