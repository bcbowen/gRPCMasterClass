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

        public override async Task GreetStream(GreetingStreamRequest request, IServerStreamWriter<GreetingStreamResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server received the request");
            Console.WriteLine(request.ToString());
            string result = $"hello {request.Greeting.FirstName} {request.Greeting.LastName}";
            foreach (int i in Enumerable.Range(1, 10)) 
            {
                await responseStream.WriteAsync(new GreetingStreamResponse() { Result = result});
            }
        }
    }
}
