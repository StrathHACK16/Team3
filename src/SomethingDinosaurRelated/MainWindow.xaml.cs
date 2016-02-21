using System.Windows;
using System.Windows.Input;

namespace SomethingDinosaurRelated
{
    public partial class MainWindow : Window
    {
        KinectControl kinectCtrl = new KinectControl();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void chkNoClick_Checked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }


        public void chkNoClickChange()
        {
            Properties.Settings.Default.Save();
        }

        private void chkNoClick_Unchecked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectCtrl.Close();
        }

        public void rdiGripGestureChange()
        {
            kinectCtrl.useGripGesture = true;
            Properties.Settings.Default.GripGesture = kinectCtrl.useGripGesture;
            Properties.Settings.Default.Save();
        }

        private void rdiGrip_Checked(object sender, RoutedEventArgs e)
        {
            rdiGripGestureChange();
        }

        private void rdiPause_Checked(object sender, RoutedEventArgs e)
        {
            rdiGripGestureChange();
        }
    }


}
