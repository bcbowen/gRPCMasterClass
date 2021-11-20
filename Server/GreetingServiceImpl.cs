using Greet;
using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

using static Greet.GreetingService;

namespace Server
{
    public class GreetingServiceImpl : GreetingServiceBase
    {
        public override Task<GreetingResponse> Greet(GreetingRequest request, ServerCallContext context)
        {
            string result = $"hello {request.Greeting.FirstName} {request.Greeting.LastName}";
            return Task.FromResult(new GreetingResponse() { Result = result});
        }

        public override async Task GreetLongResponse(GreetingRequest request, IServerStreamWriter<GreetingStreamResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server received the request");
            Console.WriteLine(request.ToString());
            string result = $"hello {request.Greeting.FirstName} {request.Greeting.LastName}";
            foreach (char c in result) 
            {
                await responseStream.WriteAsync(new GreetingStreamResponse() { Result = c.ToString()});
            }
        }

        public override async Task<GreetingResponse> GreetLongRequest(IAsyncStreamReader<GreetingStreamRequest> requestStream, ServerCallContext context)
        {
            GreetingRequest request = new GreetingRequest() { Greeting = new Greeting()};
            while (await requestStream.MoveNext()) 
            {
                switch (requestStream.Current.Key) 
                {
                    case "first_name":
                        request.Greeting.FirstName = requestStream.Current.Value;
                        break;
                    case "last_name":
                        request.Greeting.LastName = requestStream.Current.Value;
                        break;
                    default:
                        Console.WriteLine($"Unrecognized key: {requestStream.Current.Key}");
                        break;
                }
            }

            return await Greet(request, context);
        }

        public override async Task GreetBidirectional(IAsyncStreamReader<GreetingRequest> requestStream, IServerStreamWriter<GreetingResponse> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext()) 
            {
                var result = await Greet(requestStream.Current, context);
                Console.WriteLine($"processed {result.Result}");
                await responseStream.WriteAsync(result);
            }

        }
    }
}
