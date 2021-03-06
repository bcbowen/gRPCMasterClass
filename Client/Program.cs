//using Dummy;
using Blog;
using Greet;
using Calculator;
using Primes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Client
{
    class Program
    {
        const string target = "127.0.0.1:50051";
        static async Task Main(string[] args)
        {
            var clientCert = File.ReadAllText("ssl/client.crt");
            var clientKey = File.ReadAllText("ssl/client.key");
            var caCrt = File.ReadAllText("ssl/ca.crt");

            var channelCredentials = new SslCredentials(caCrt, new KeyCertificatePair(clientCert, clientKey)); 

            Channel channel = new Channel("localhost", 50051, channelCredentials);
            channel.ConnectAsync().ContinueWith((task) => 
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("This client connected successfully, man");

                Console.WriteLine(task.Status);
            });

            //CallDummyService(channel);
            await CallBlogService(channel);
            //await CallGreetService(channel);
            //await CallCalculateService(channel);
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

        private static async Task CallBlogService(Channel channel)
        {
            string separator = new String('*', 50) + Environment.NewLine;
            var client = new BlogService.BlogServiceClient(channel);

            Blog.Blog blog = await CreateBlog(client);
            await ReadBlog(client, blog.Id);
            blog.Title = "Grilling while snockered";
            blog.Content = "Step 1: Get snockered; Step 2: Fire up the grill... to be continued";
            await UpdateBlog(client, blog);
            // uncomment to delete the blog: 
            //DeleteBlog(client, blog.Id);
            List<Blog.Blog> blogs = await ListBlogs(client);
            foreach (Blog.Blog b in blogs) 
            {
                Console.WriteLine(b);
            }
        }


        private static async Task<List<Blog.Blog>> ListBlogs(BlogService.BlogServiceClient client) 
        {
            var response = client.ListBlog(new ListBlogRequest());
            List<Blog.Blog> blogs = new List<Blog.Blog>();
            while (await response.ResponseStream.MoveNext()) 
            {
                blogs.Add(response.ResponseStream.Current.Blog);
            }

            return blogs;
        }

        private static void DeleteBlog(BlogService.BlogServiceClient client, string blogId)
        {
            try 
            {
                var response = client.DeleteBlog(new DeleteBlogRequest() { BlogId = blogId });
                Console.WriteLine($"Blog {blogId} was deleted");
            }
            catch (RpcException ex) 
            {
                Console.WriteLine(ex.Status.Detail); 
            }
        }

        private static async Task<Blog.Blog> CreateBlog(BlogService.BlogServiceClient client) 
        {
            var blogResponse = client.CreateBlog(new CreateBlogRequest()
            {
                Blog = new Blog.Blog()
                {
                    AuthorId = "Ben",
                    Title = "New Blog",
                    Content = "This is the blog, dudes"
                }
            });

            Console.WriteLine($"Blog {blogResponse.Blog.Id} was created");

            return blogResponse.Blog;
        }

        private static async Task ReadBlog(BlogService.BlogServiceClient client, string blogId) 
        {
            try
            {
                var response = client.ReadBlog(new ReadBlogRequest()
                {
                    BlogId = blogId,
                });
                Console.WriteLine(response.Blog.ToString());
            } 
            catch (RpcException ex) 
            {
                Console.WriteLine(ex.Status.Detail);
            }
            
        }

        private static async Task UpdateBlog(BlogService.BlogServiceClient client, Blog.Blog blog)
        {
            try
            {
                var response = client.UpdateBlog(new UpdateBlogRequest()
                {
                    Blog = blog,
                });
                Console.WriteLine(response.Blog.ToString());
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }

        }

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

            CallGreetWithDeadline(client, requests[3], 500, false);
            CallGreetWithDeadline(client, requests[4], 100, true);
            
        }

        private static void CallGreetWithDeadline(GreetingService.GreetingServiceClient client, GreetingRequest request, int timeoutMs, bool expectedTimeout) 
        {
            if (expectedTimeout) 
            {
                Console.WriteLine("Greet with deadline (Expected to fail): ");
            }
            else 
            {
                Console.WriteLine("Greet with deadline (Not expected to fail): ");
            }
                        
            try
            {
                GreetingResponse response = client.GreetWithDeadline(request, deadline: DateTime.UtcNow.AddMilliseconds(timeoutMs));
                Console.WriteLine($"Response: {response.Result}");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("Error: " + ex.Status.Detail);
            }
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

            Console.WriteLine("Done receiving");

            Console.WriteLine("Square root:");
            numbers = new int[] { 16, 0, -1};

            foreach (int number in numbers) 
            {
                try 
                {
                    var response = client.SquareRoot(new SqrtRequest() { Number = number });
                    Console.WriteLine($"The square root of {number} is {response.Result}, allegedly.");
                }
                catch (RpcException ex) 
                {
                    Console.WriteLine("Fuck! An error: " + ex.Status.Detail);
                }
            }
            
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
