using Grpc.Core;
using Primes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Primes.PrimesService;

namespace Server
{
    public class PrimesServiceImpl : PrimesServiceBase
    {
        public override async Task CalculatePrimes(PrimesRequest request, IServerStreamWriter<PrimesResponse> responseStream, ServerCallContext context)
        {
            List<int> factors = GetFactors(request.Value);
            foreach (int factor in factors) 
            {
                await responseStream.WriteAsync(new PrimesResponse { Factor = factor });
            }
            
        }

        private List<int> GetFactors(int value)
        {
            List<int> factors = new List<int>();
            int n = value;
            int k = 2;
            while (n > 1)
            {
                if (n % k == 0)
                {
                    // if k evenly divides into N
                    factors.Add(k);
                    n /= k;
                }
                else
                {
                    k++;
                }
            }
            return factors;
        }

    }
}
