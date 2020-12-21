using System.Windows.Forms;

namespace ThinkpadChargeLimit
{
    public class ThinkPowerStatus
    {
        public virtual float BatteryLifePercent
        {
            get
            {
                return SystemInformation.PowerStatus.BatteryLifePercent;
            }
        }
    }
}