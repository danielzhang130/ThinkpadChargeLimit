using System;
using System.Timers;
using System.Windows.Forms;

namespace ThinkpadChargeLimit
{
    public class FullChargeHandler
    {
        private readonly ChargeThresholdWrapper wrapper;
        private readonly ThinkPowerStatus powerStatus;
        private event EventHandler FullyChargedEventCallback;

        private readonly int originalLimit;
        private float timeInterval = 60;
        private float prevCharge;
        private float timeElapsed = 0;

        private ThinkTimer timer;

        public FullChargeHandler(ChargeThresholdWrapper wrapper,
                                 ThinkPowerStatus powerStatus,
                                 ThinkTimer timer,
                                 EventHandler handler)
        {
            this.wrapper = wrapper;
            this.powerStatus = powerStatus;
            this.timer = timer;
            this.timer.Elapsed += CheckCharge;
            this.FullyChargedEventCallback += handler;

            originalLimit = wrapper.Limit;
            wrapper.Limit = 100;

            prevCharge = CurrentCharge;

            Wait();
        }

        private void CheckCharge(object sender, ElapsedEventArgs e)
        {
            timeElapsed += timeInterval;

            float current = CurrentCharge;

            if (current > 98)
            {
                ResetLimit();
            }
            else
            {
                float deltaCharge = current - prevCharge;
                if (deltaCharge == 0)
                {
                    Wait();
                }
                else if (deltaCharge < 0)
                {
                    ResetLimit();
                }
                else
                {
                    float chargingSpeed = deltaCharge / timeElapsed;
                    float chargingRemaining = 100f - current;
                    float chargingTime = chargingRemaining / chargingSpeed;
                    timeInterval = (chargingTime * 0.8f);
                    prevCharge = current;
                    Wait();
                }
            }
        }

        public void Cancel()
        {
            wrapper.Limit = originalLimit;
            timer.Stop();
        }

        private void Wait()
        {
            timer.Start(timeInterval * 1000);
        }

        private void ResetLimit()
        {
            wrapper.Limit = originalLimit;
            FullyChargedEventCallback(null, null);
        }

        private float CurrentCharge {
            get
            {
                return (powerStatus.BatteryLifePercent * 100);
            }
        }
    }
}