using System;
using System.Windows.Forms;

namespace ThinkpadChargeLimit
{
    internal class ChargeLimitApplicationContext : ApplicationContext
    {
        private static readonly string CHARGING_TO_FULL = "Charging to Full";
        private static readonly string CHARGING_TO_PERCENT = "Charging to {0}%";

        private readonly NotifyIcon notifyIcon;
        private readonly ChargeThresholdWrapper wrapper;

        private int chargeLimit;
        private MenuItem prevChecked;

        public ChargeLimitApplicationContext()
        {
            wrapper = new ChargeThresholdWrapper();

            chargeLimit = wrapper.Limit;

            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));

            MenuItem[] limitItems = new MenuItem[6];
            int j = 0;
            for (int i = 50; i <= 100; i += 10)
            {
                limitItems[j] = new MenuItem(i.ToString() + '%', new EventHandler(SetLimit))
                {
                    RadioCheck = true
                };

                if (i == chargeLimit)
                {
                    limitItems[j].Checked = true;
                    prevChecked = limitItems[j];
                }
                j++;
            }
            MenuItem limitsMenuItem = new MenuItem("Change Charge Limit", limitItems);

            notifyIcon = new NotifyIcon
            {
                ContextMenu = new ContextMenu(new MenuItem[]
                    { limitsMenuItem, exitMenuItem }),
                Visible = true
            };
            notifyIcon.ContextMenu.Popup += new EventHandler(OnShowMenu);

            UpdateState();
        }

        private void SetLimit(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                chargeLimit = int.Parse(menuItem.Text.Substring(0, menuItem.Text.IndexOf('%')));
                menuItem.Checked = true;
                // todo needed?
                prevChecked.Checked = false;
                prevChecked = menuItem;

                wrapper.Limit = chargeLimit;
                UpdateState();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void OnShowMenu(object sender, EventArgs e)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            if (wrapper.HasLimit)
            {
                notifyIcon.Icon = Properties.Resources.HalfCharged;
                notifyIcon.Text = string.Format(CHARGING_TO_PERCENT, chargeLimit);
            }
            else
            {
                notifyIcon.Icon = Properties.Resources.FullyCharged;
                notifyIcon.Text = CHARGING_TO_FULL;
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}