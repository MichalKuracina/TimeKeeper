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
        private Log log;

        private bool ElementFound { get; set; }
        public int SecondsOnline { get; set; }
        public string StartingTime { get; set; }


        public form1()
        {
            InitializeComponent();
            
            // Instantiate log file, get today's progress
            log = new Log();
            log.Create();
            StartingTime = log.Get();
            notifyIcon1.Text = StartingTime;
            SecondsOnline = (int)TimeSpan.Parse(StartingTime).TotalSeconds;
            
            // Set windows position

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(20, Screen.PrimaryScreen.Bounds.Height - 150);
         
            // Initialize BackgroundWorker

            _backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
            };

            

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
                bool notStarted = String.IsNullOrEmpty(myElement.Current.HelpText); // can be empty is Team was not started
                bool beRigthBack = myElement.Current.HelpText.Contains("Status set to Be right back");
                bool away = myElement.Current.HelpText.Contains("Status set to Away");
                bool offline = myElement.Current.HelpText.Contains("Status set to Offline");
                if (!notStarted & !beRigthBack & !away & !offline)
                {
                    SecondsOnline++;
                    TimeSpan t = TimeSpan.FromSeconds(SecondsOnline);

                    string result = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);

                    log.Write(result);
                    _backgroundWorker.ReportProgress(1, result); // Send information to main thread
                }
                System.Threading.Thread.Sleep(1000);
            }

        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string fromWorker = e.UserState.ToString();
            lbl_Status.Text = fromWorker;
            notifyIcon1.Text = fromWorker;
        }

        private void lbl_Status_Click(object sender, EventArgs e)
        {
            Hide();
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
