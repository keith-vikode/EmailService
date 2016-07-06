using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmailService.Core.Test
{
    public class Tests
    {
        [Fact]
        public void AssertTrue()
        {
            int expected = 4;
            int actual = 2 + 2;
            Assert.Equal(expected, actual);
        }
    }
}
