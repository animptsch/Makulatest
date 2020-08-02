using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MakulaTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application    
    {
        public App()
        {
            AnalyseControl.IsFatalError = false;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if(e.Exception != null)
            {
                AnalyseControl.IsFatalError = true;
                this.Dispatcher.Invoke(() =>
               {
                   ErrorView view = new ErrorView(e.Exception);
                   view.ShowDialog();
               });
            }
        }
    } 

    
}
