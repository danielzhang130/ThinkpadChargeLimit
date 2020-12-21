using System;
using System.Timers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ThinkpadChargeLimit;
using ThinkTimer = ThinkpadChargeLimit.ThinkTimer;

namespace UnitTest
{
    [TestClass]
    public class FullChargeHandlerTest
    {

        private Mock<ChargeThresholdWrapper> wrapper;
        private Mock<EventHandler> eventHandler;
        private Mock<ThinkTimer> timer;
        private Mock<ThinkPowerStatus> powerStatus;

        [TestInitialize]
        public void Setup()
        {
            wrapper = new Mock<ChargeThresholdWrapper>();
            eventHandler = new Mock<EventHandler>();
            timer = new Mock<ThinkTimer>();
            powerStatus = new Mock<ThinkPowerStatus>();
        }

        [TestMethod]
        public void SetsLimitTo100()
        {
            wrapper.SetupGet(m => m.Limit).Returns(80);

            FullChargeHandler handler = new FullChargeHandler(wrapper.Object,
                                                              powerStatus.Object,
                                                              timer.Object,
                                                              eventHandler.Object);

            wrapper.VerifySet(m => m.Limit = 100);
        }

        [TestMethod]
        public void ResetsLimitToOriginal()
        {
            int original = 80;

            wrapper.SetupGet(m => m.Limit).Returns(original);
            timer.Setup(m => m.Start(It.IsAny<float>())).Raises(m => m.Elapsed += null, (ElapsedEventArgs)null);
            powerStatus.SetupGet(m => m.BatteryLifePercent).Returns(0.99f);

            FullChargeHandler handler = new FullChargeHandler(wrapper.Object,
                                                  powerStatus.Object,
                                                  timer.Object,
                                                  eventHandler.Object);

            wrapper.VerifySet(m => m.Limit = original);
        }

        [TestMethod]
        public void WaitUntilChargeChanged()
        {
            timer.Setup(m => m.Start(It.IsAny<float>())).Raises(m => m.Elapsed += null, (ElapsedEventArgs)null);
            powerStatus.SetupSequence(m => m.BatteryLifePercent).Returns(0.8f).Returns(0.8f).Returns(0.8f).Returns(0.99f);

            FullChargeHandler handler = new FullChargeHandler(wrapper.Object,
                                      powerStatus.Object,
                                      timer.Object,
                                      eventHandler.Object);

            timer.Verify(m => m.Start(It.IsAny<float>()), Times.Exactly(3));
            eventHandler.Verify(m => m(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Once);
        }

        [TestMethod]
        public void WaitProportionalTime()
        {
            timer.Setup(m => m.Start(It.IsAny<float>())).Raises(m => m.Elapsed += null, (ElapsedEventArgs)null);
            powerStatus.SetupSequence(m => m.BatteryLifePercent).Returns(0.8f).Returns(0.82f);

            FullChargeHandler handler = new FullChargeHandler(wrapper.Object,
                                      powerStatus.Object,
                                      timer.Object,
                                      eventHandler.Object);

            timer.Verify(m => m.Start(It.IsInRange<float>((9*60*0.5f*1000), 9*60f*1000, Range.Inclusive)), Times.Once);
        }
    }
}
