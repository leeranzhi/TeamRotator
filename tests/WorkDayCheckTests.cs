using Xunit;
using Buzz.Utilities;

namespace Tests
{
    public class WorkingDayCheckTests
    {
        [Fact]
        public void IsWorkingDay_ShouldReturnTrue_ForWorkingDay()
        {
            var date = new DateTime(2024, 10, 16);
            bool result = WorkingDayCheck.IsWorkingDay(date);

            Assert.True(result);
        }
        [Fact]
        public void IsWorkingDay_ShouldReturnFalse_ForWeekend()
        {
            var date = new DateTime(2024, 10, 19);
            bool result = WorkingDayCheck.IsWorkingDay(date);

            Assert.False(result);
        }
        
        [Fact]
        public void IsWorkingDay_ShouldReturnFalse_ForHoliday()
        {
            var date = new DateTime(2024, 10, 1);
            bool result = WorkingDayCheck.IsWorkingDay(date);

            Assert.False(result);
        }
    }
}
