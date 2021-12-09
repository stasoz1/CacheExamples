using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CacheExamples.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDistributedController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string employeesCollectionKey = "employeesCollectionKey";

        public EmployeeDistributedController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        public async Task<IEnumerable<Employee>> Get()
        {
            // Find cached item
            byte[] objectFromCache = await _distributedCache.GetAsync(employeesCollectionKey);

            if (objectFromCache != null)
            {
                // Deserialize it
                var jsonToDeserialize = System.Text.Encoding.UTF8.GetString(objectFromCache);
                var cachedResult = JsonSerializer.Deserialize<IEnumerable<Employee>>(jsonToDeserialize);
                if (cachedResult != null)
                {
                    // If found, then return it
                    return cachedResult;
                }
            }

            // If not found, then recalculate response
            var result = GetEmployees();

            // Serialize the response
            byte[] objectToCache = JsonSerializer.SerializeToUtf8Bytes(result);
            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(5))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));

            // Cache it
            await _distributedCache.SetAsync(employeesCollectionKey, objectToCache, cacheEntryOptions);

            return result;
        }

        private static IEnumerable<Employee> GetEmployees()
        {
            return new List<Employee>()
            {
                 new Employee() { Id = 1, Name = "John", Position = "Back-End Develop" },
                 new Employee() { Id = 2, Name = "Daniel", Position = "Front-End Developer" },
                 new Employee() { Id = 3, Name = "Steve", Position = "Team Lead" },
            };
        }

        public class Employee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Position { get; set; }
        }
    }
}
