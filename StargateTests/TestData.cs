using System.Collections.Generic;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests
{
    public static class TestData
    {
        public static List<Person> GetPeople()
        {
            return new List<Person>
            {
                new Person { Id = 1, Name = "Jace" },
                new Person { Id = 2, Name = "Tommy" },
                new Person { Id = 3, Name = "Bobby" }
            };
        }
    }
}

