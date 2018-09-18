using NUnit.Framework;

namespace Monik.Service.Tests
{
    [TestFixture]
    public class GaugeWindowCalculatorTests
    {   
        [TestCase(0, 0)]
        [TestCase(10, 10)]
        public void OnNewValue_SingleValue(double expected, double newValue)
        {
            var window = new GaugeWindowCalculator();

            window.OnNewValue(newValue);
            
            Assert.AreEqual(expected, window.GetValue());
        }

        [TestCase(0, new[] {0.0, 0.0, 0.0})]
        [TestCase(10, new[] {10.0, 10.0, 10.0})]
        [TestCase(-2.5, new[] {10.0, 0.0, -10.0})]
        [TestCase(2.25, new[] {1.0, 2.0, 3.0})]
        public void OnNewValue_Sequence(double expected, double[] newValues)
        {
            var window = new GaugeWindowCalculator();

            foreach (var val in newValues)
                window.OnNewValue(val);

            Assert.AreEqual(expected, window.GetValue());
        }

        [Test]
        public void OnNextSecond_OneTime()
        {
            var window = new GaugeWindowCalculator();

            window.OnNewValue(10);
            window.OnNextSecond();
            window.OnNewValue(0);

            Assert.AreEqual(5.0, window.GetValue());
        }

        [TestCase(0.0, new[] {0.0, 0.0, 0.0})]
        [TestCase(0.0, new[] {1.0, 0.0, -1.0})]
        [TestCase(2.5, new[] {0.0, 1.0, 2.0, 3.0, 4.0, 5.0})]
        public void OnNextSecond_Sequence(double expected, double[] values)
        {
            var window = new GaugeWindowCalculator();

            foreach (var val in values)
            {
                window.OnNewValue(val);
                window.OnNextSecond();
            }

            Assert.AreEqual(expected, window.GetValue());
        }

        [TestCase(0.0, 10.0, 0.0, 0.0, 300)]
        [TestCase(0.0, 10.0, 0.0, 0.0, 299)]
        [TestCase(10.0/300, 10.0, 0.0, 0.0, 298)]
        [TestCase(30.0 / 300, 10.0, 0.0, 20.0, 298)]
        [TestCase(20.0 / 300, 10.0, 0.0, 20.0, 299)]
        public void OnNextSecond_(double expected, double firstVal, double populateVal, double lastVal, int populateTimes)
        {
            var window = new GaugeWindowCalculator();

            window.OnNewValue(firstVal);
            for (var i = 0; i < populateTimes; ++i)
            {
                window.OnNextSecond();
                window.OnNewValue(populateVal);
            }
            window.OnNextSecond();
            window.OnNewValue(lastVal);

            Assert.AreEqual(expected, window.GetValue());
        }

        [Test]
        public void GetValue_Zero()
        {
            var window = new GaugeWindowCalculator();

            var result = window.GetValue();

            Assert.AreEqual(0, result);
        }
    }
}