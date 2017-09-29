using System;
using Xunit;

namespace Jint.Tests.Runtime
{
    class MyClass
    {
        public String a { get; set; }
        public int b;
        public double[] c;
        // public Guid d { get; set; }

        public void f() { return; }
    }

    public class CSObjectPropertiesTests : IDisposable
    {
        private readonly Engine _engine;

        public CSObjectPropertiesTests()
        {
            _engine = new Engine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .SetValue("assert", new Action<bool>(Assert.True))
                ;
        }

        void IDisposable.Dispose()
        {
        }

        [Fact]
        public void PropTest1()
        {
            var json = _engine
                .SetValue("x", new MyClass() { a = "Hello", b = 42, c = new[] { 12.345, 6.78 }/*, d = Guid.Empty */ })
                .Execute("JSON.stringify(x)")
                .GetCompletionValue()
                .ToObject();

            Assert.Equal("{\"a\":\"Hello\",\"b\":42,\"c\":[12.345,6.78]}", json);
        }
    }
}
