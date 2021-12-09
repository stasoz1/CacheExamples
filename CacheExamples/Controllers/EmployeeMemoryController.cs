using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace CacheExamples.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeMemoryController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string employeesCollectionKey = "employeesCollectionKey";

        public EmployeeMemoryController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public IEnumerable<Employee> Get()
        {
            IEnumerable<Employee> employeesCollection = null;

            if (_memoryCache.TryGetValue(employeesCollectionKey, out employeesCollection))
            {
                return employeesCollection;
            }

            employeesCollection = GetEmployees();

            // Sliding + Absolute Expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(5))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(10));

            _memoryCache.Set(employeesCollectionKey, employeesCollection, cacheOptions);

            return employeesCollection;
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
