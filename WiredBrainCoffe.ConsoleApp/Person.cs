using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiredBrainCoffe.Generators;



namespace WiredBrainCoffe.ConsoleApp
{

   [GenerateToString]
    internal partial class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Address? Address { get; set; } 

        /*public override string ToString()
        {
            return $"FirstName:{FirstName}; LastName:{LastName}";
        }*/
    }
}
