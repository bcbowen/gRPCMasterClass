using Calculator;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Calculator.CalculatorService;

namespace Server
{
    class CalculateServiceImpl : CalculatorServiceBase
    {
        public override Task<Response> Calculate(Request request, ServerCallContext context)
        {
            int result = request.Value1 + request.Value2;
            return Task.FromResult(new Calculator.Response() { Result = result });
        }

    }
}
