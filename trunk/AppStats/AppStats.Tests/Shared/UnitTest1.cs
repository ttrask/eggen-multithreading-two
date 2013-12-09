using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using AppStats.Shared.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppStats.Tests.Shared.Statistics
{
    [TestClass]
    public class QuartilesTests
    {
        // Ideal list count: x = 4n + 3
        private readonly double[] _testDataIdeal =
            new[] {19.0, 19.0, 20.0, 21.0, 22.0, 22.0, 22.0, 23.0, 23.0, 24.0, 25.0};
 
        private readonly double[] _testDataNotIdeal =
            new[] {10.9, 7.3, 1.2, 75.3, 2.3, 4.4, 1.9, 2.1, 53.0, 2.8};
 
        [TestMethod]
        public void LowerQuartileOnIdealList()
        {
            double result = _testDataIdeal
                .OrderBy(n => n)
                .LowerQuartile();
 
            Assert.AreEqual(expected: 20.0, actual: result);
            Debug.WriteLine("Lower quartile (xL): {0}", result);
        }
 
        [TestMethod]
        public void LowerQuartileOnNoTIdealList()
        {
            double result = _testDataNotIdeal
                .OrderBy(n => n)
                .LowerQuartile();
 
            Assert.AreEqual(expected: 2.05, actual: result);
            Debug.WriteLine("Lower quartile (xL): {0}", result);
        }
 
        [TestMethod]
        public void UpperQuartileOnIdealList()
        {
            double result = _testDataIdeal
                .OrderBy(n => n)
                .UpperQuartile();
 
            Assert.AreEqual(expected: 23.0, actual: result);
            Debug.WriteLine("Upper quartile (xU): {0}", result);
        }
 
        [TestMethod]
        public void UpperQuartileOnNoTIdealList()
        {
            double result = _testDataNotIdeal
                .OrderBy(n => n)
                .UpperQuartile();
 
            Assert.AreEqual(expected: 21.425, actual: result);
            Debug.WriteLine("Upper quartile (xU): {0}", result);
        }
 
        [TestMethod]
        public void MiddleQuartileOnIdealList()
        {
            double result = _testDataIdeal
                .OrderBy(n => n)
                .MiddleQuartile();
 
            Assert.AreEqual(expected: 22.0, actual: result);
            Debug.WriteLine("Middle quartile / median (xm): {0}", result);
        }
 
        [TestMethod]
        public void MiddleQuartileOnNoTIdealList()
        {
            double result = _testDataNotIdeal
                .OrderBy(n => n)
                .MiddleQuartile();
 
            Assert.AreEqual(expected: 3.6, actual: result);
            Debug.WriteLine("Middle quartile / median (xm): {0}", result);
        }
 
        [TestMethod]
        public void InterQuartileRangeOnIdealList()
        {
            double result = _testDataIdeal
                .OrderBy(n => n)
                .InterQuartileRange();
 
            Assert.AreEqual(expected: 3.0, actual: result);
            Debug.WriteLine("InterQuartile Range is {0}", result);
        }
 
        [TestMethod]
        public void InterQuartileRangeOnNoTIdealList()
        {
            double result = _testDataNotIdeal
                .OrderBy(n => n)
                .InterQuartileRange();
 
            Assert.AreEqual(expected: 19.375, actual: result);
            Debug.WriteLine("InterQuartile Range is {0}", result);
        }
    }
}
