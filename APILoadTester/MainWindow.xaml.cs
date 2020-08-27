using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ComponentModel;
using System.Diagnostics;

namespace APILoadTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rng = new Random(5);

        int _successCount;
        int _failedCount;
        int _sentCount;

        int SuccessCount
        {
            get => _successCount;
            set
            {
                _successCount = value;
                this.Dispatcher.Invoke(() => { TextBlock_SuccessCount.Text = value.ToString(); });
            }
        }

        int FailedCount
        {
            get => _failedCount;
            set
            {
                _failedCount = value;
                this.Dispatcher.Invoke(() => { TextBlock_FailedCount.Text = value.ToString(); });
            }
        }

        int SentCount
        {
            get => _sentCount;
            set
            {
                _sentCount = value;
                this.Dispatcher.Invoke(() => { TextBlock_SentCount.Text = value.ToString(); });
            }
        }

        string Token;
        int Trials = 0;
        int Threads;

        object CountLock = new object();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            int res = 0;
            if (int.TryParse(TextBox_RequestCount.Text, out res))
                Trials = res;
            if (int.TryParse(TextBox_ThreadCount.Text, out res))
                Threads = res;
            Token = TextBox_Token.Text;

            for (int i = 0; i < Threads; i++)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerAsync();
            }
        }

        private async void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            for (int i = 0; i < Trials; i++)
            {
                SendRequest(client);
                await Task.Delay(250);
            }
        }

        private async void SendRequest(HttpClient client)
        {
            lock (CountLock)
            {
                SentCount++;
            }

            string url = "https://portfolio.api.dev.brightspace.com/api/2289ed2f-1e5f-41ca-8f63-23c5db2cb820/evidence?userId=30227&orgUnitId=123064&section=All%20Users&pageSize=1&approved=true&excludeUnapprovedCount=false";
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                lock (CountLock)
                {
                    SuccessCount++;
                }
            }
            else
            {
                lock (CountLock)
                {
                    FailedCount++;
                }

                string res = await response.Content.ReadAsStringAsync();
                this.Dispatcher.Invoke(() =>
                {
                    TextBlock_Response.Text = res;
                });
                await Task.Delay(rng.Next(250, 5000));
            }

        }

        private void Button_ResetStats_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ResetStats();
            });
        }

        private void ResetStats()
        {
            SentCount = 0;
            SuccessCount = 0;
            FailedCount = 0;
        }
    }
}
