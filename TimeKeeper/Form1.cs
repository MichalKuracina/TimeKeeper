using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;

namespace TimeKeeper
{
    public partial class form1 : Form
    {
        private BackgroundWorker _backgroundWorker;
        private bool ElementFound { get; set; }
        public int SecondsOnline { get; set; }

        public form1()
        {
            InitializeComponent();

            // Set windows position

            //this.StartPosition = FormStartPosition.Manual;
            //this.Location = new Point(100, Screen.PrimaryScreen.Bounds.Height - 200);

            // Initialize BackgroundWorker

            _backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
            };

            SecondsOnline = 0;

            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Condition of ToolBar
            AndCondition condition = new AndCondition(
                new PropertyCondition(AutomationElement.ClassNameProperty, "MSTaskListWClass"),
                new PropertyCondition(AutomationElement.FrameworkIdProperty, "Win32"),
                new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "tool bar"),
                new PropertyCondition(AutomationElement.NameProperty, "Running applications"));

            // First toolbar element
            AutomationElement toolBarElement = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, condition);

            // Condition of all 2 types which can be in the ToolBar
            OrCondition condition1 = new OrCondition(
                new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "button"),
                new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "menu item"));

            // FindAll buttons and menu items
            AutomationElementCollection elements = toolBarElement.FindAll(TreeScope.Children, condition1);

            ElementFound = false;
            AutomationElement myElement = null;

            foreach (AutomationElement el in elements)
            {
                string name = el.Current.Name;
                if (name.Contains("Microsoft Teams"))
                {
                    ElementFound = true;
                    myElement = el;
                    break;
                }
            }

            // Terminate the app if Microsoft Teams is not existing

            if (!ElementFound)
            {
                throw new ElementNotAvailableException("Microsoft Teams must be pinned to your taskbar.");
            }

            // Check for 'HelpText' property of the element in infitite loop
            // This is becasue elements in taskbar are not emitting any events except focus
            // Also 'AutomationPropertyChangedEventHandler' is not working here :/

            while (true)
            {
                // Status set to Be right back
                // Status set to Away
                // Status set to Offline
                // property does not exists
                try
                {
                    bool notStarted = myElement.Current.HelpText.Contains("");
                    bool beRigthBack = myElement.Current.HelpText.Contains("Status set to Be right back");
                    bool away = myElement.Current.HelpText.Contains("Status set to Away");
                    bool offline = myElement.Current.HelpText.Contains("Status set to Offline");
                    if (notStarted != true & beRigthBack != true & away != true & offline != true)
                    {
                        SecondsOnline++;
                        _backgroundWorker.ReportProgress(1, SecondsOnline); // Send information to main thread
                    }
                }
                catch (Exception)
                {
                    // 'HelpText' property does not exist because Microsoft Teams is not started
                }

                
                System.Threading.Thread.Sleep(1000);
            }

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string fromWorker = e.UserState.ToString();
            lbl_Status.Text = fromWorker;
        }

    }
}
