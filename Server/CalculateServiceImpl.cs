using Calculator;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using static Calculator.CalculatorService;

namespace Server
{
    class CalculateServiceImpl : CalculatorServiceBase
    {
        public override Task<SumResponse> CalculateSum(SumRequest request, ServerCallContext context)
        {
            int result = request.Value1 + request.Value2;
            return Task.FromResult(new SumResponse { Result = result });
        }

        public override async Task<AverageResponse> CalculateAverage(IAsyncStreamReader<AverageRequest> requestStream, ServerCallContext context)
        {
            double responseCount = 0;
            double total = 0;

            while (await requestStream.MoveNext()) 
            {
                responseCount++;
                total += requestStream.Current.Value;
            }

            double result = 0;
            if (responseCount > 0)
            {
                result = total / responseCount;
            }

            return new AverageResponse { Result = result };
        }

        public override async Task FindMaximum(IAsyncStreamReader<FindMaximumRequest> requestStream, IServerStreamWriter<FindMaximumResponse> responseStream, ServerCallContext context)
        {
            int max = 0;
            int value;
            while (await requestStream.MoveNext()) 
            {
                value = requestStream.Current.Value;
                Console.WriteLine($"Processing {value}");
                if (value > max) 
                {
                    await responseStream.WriteAsync(new FindMaximumResponse { Result = value } );
                    max = value;
                    Console.WriteLine($"New max is {max}");
                }
            
            }
        }
    }
}
