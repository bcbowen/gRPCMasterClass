using Calculator;
using Grpc.Core;
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

    }
}
