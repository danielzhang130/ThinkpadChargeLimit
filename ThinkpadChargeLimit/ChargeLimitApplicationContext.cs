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

        private readonly MenuItem fullChargeMenuItem;
        private readonly MenuItem limitsMenuItem;

        private int chargeLimit;
        private MenuItem prevChecked;

        private FullChargeHandler fullChargeHandler;

        public ChargeLimitApplicationContext(ChargeThresholdWrapper _wrapper)
        {
            wrapper = _wrapper;

            chargeLimit = wrapper.Limit;

            fullChargeMenuItem = new MenuItem("Fully Charge Once", new EventHandler(FullyChargeOnce));
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
            limitsMenuItem = new MenuItem("Change Charge Limit", limitItems);

            notifyIcon = new NotifyIcon
            {
                ContextMenu = new ContextMenu(new MenuItem[]
                    { limitsMenuItem, fullChargeMenuItem, exitMenuItem }),
                Visible = true
            };
            notifyIcon.ContextMenu.Popup += new EventHandler(UpdateState);

            UpdateState();
        }

        private void FullyChargeOnce(object sender, EventArgs e)
        {
            if (fullChargeMenuItem.Checked)
            {
                fullChargeHandler.Cancel();

                fullChargeMenuItem.Checked = false;
                limitsMenuItem.Enabled = true;
            }
            else
            {
                fullChargeHandler = new FullChargeHandler(wrapper,
                                          new ThinkPowerStatus(),
                                          new ThinkTimer(),
                                          new EventHandler(FullyChargedCallback));

                fullChargeMenuItem.Checked = true;
                limitsMenuItem.Enabled = false;
            }
            UpdateState();
        }

        private void FullyChargedCallback(object sender, EventArgs e)
        {
            fullChargeMenuItem.Checked = false;
            limitsMenuItem.Enabled = true;
            
            notifyIcon.ShowBalloonTip(15000,
                                      "Battery is fully charged",
                                      "Battery charging limit reset to " + chargeLimit + "%",
                                      ToolTipIcon.Info);
            UpdateState();
        }

        private void SetLimit(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                chargeLimit = int.Parse(menuItem.Text.Substring(0, menuItem.Text.IndexOf('%')));
                menuItem.Checked = true;
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

        private void UpdateState(object sender = null, EventArgs e = null)
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