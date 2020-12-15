using RoslynBoilerplate;
using NUnit.Framework;

namespace NUnitTestProject1
{
    public class Tests
    {
        [Test]
        public void Test1()
        {
            var provider = new RoslynProjectProvider(@"..\..\..\..\test-.net5-app\ConsoleApp1\ConsoleApp1.sln", "ConsoleApp1");

            var classes = provider.Classes;
        }
    }
}