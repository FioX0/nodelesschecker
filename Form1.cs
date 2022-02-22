using Dapper;
using HeadlessChecker.Models;
using MySqlConnector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeadlessChecker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            NodeWorker.RunWorkerAsync();
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if(senderGrid.Rows.Count > 0 && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn )
                {
                    var index = e.RowIndex;
                    var row = dataGridView1.Rows[index].Cells[0].Value;
                    string pattern = @"\d+";
                    string input = row.ToString();
                    RegexOptions options = RegexOptions.Multiline;

                    Match m = Regex.Match(input, pattern, options);

                    string selectednode = "9c-main-rpc-" + m.Value.ToString() + ".nine-chronicles.com";

                    string path2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Nine Chronicles\\config.json";
                    string path3 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Nine Chronicles\\configoldrpc.json";

                    string text = System.IO.File.ReadAllText(path2);

                    if (!text.Contains("RemoteNodeList"))
                    {
                        if (File.Exists(path3))
                        {
                            File.Delete(path3);
                        }

                        File.Move(path2, path3);

                        File.Delete(path2);

                        WebClient lWebClient2 = new WebClient();

                        lWebClient2.Timeout = 600 * 60 * 1000;
                        lWebClient2.DownloadFile("https://cdn.discordapp.com/attachments/897944903030538280/939976268164788304/config.json", Environment.CurrentDirectory + "\\config.json");

                        string path = Environment.CurrentDirectory + "\\config.json";

                        File.Move(path, path2);
                    }

                    string str = File.ReadAllText(path2);
                    for (int y = 1; y <= 99; y++)
                    {
                        string FindMe = "9c-main-rpc-" + y + ".nine-chronicles.com";
                        str = str.Replace(FindMe, selectednode);
                    }
                    File.WriteAllText(path2, str);

                    try
                    {
                        var chromeDriverProcesses = Process.GetProcesses().
                        Where(pr => pr.ProcessName == "Nine Chronicles"); // without '.exe'

                        foreach (var process in chromeDriverProcesses)
                        {
                            process.Kill();
                        }

                        string command = Environment.GetEnvironmentVariable("LocalAppData");
                        command = command + @"""\Programs\Nine Chronicles\Nine Chronicles.exe""";
                        Process p = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = @"/C " + command; // cmd.exe spesific implementation
                        startInfo.UseShellExecute = true;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.StartInfo = startInfo;
                        p.Start();
                    }
                    catch (Exception ex) { }
                }
            }
        }

        private class WebClient : System.Net.WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest lWebRequest = base.GetWebRequest(uri);
                lWebRequest.Timeout = Timeout;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
                return lWebRequest;
            }
        }

        public ChainModel[] LoadRPCALL()
        {

            using (MySqlConnection cnn = new MySqlConnection(connectingstring))
            {
                try
                {
                    var sql = @"Select * from info;";

                    using (var multi = cnn.QueryMultiple(sql, new { }))
                    {
                        var customer = multi.Read<ChainModel>().ToArray();
                        return customer;
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        private async void NodeWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            //var nodes = RPCNodes();

            int i = 0;

            while (i == 0)
            {
                //string address = string.Empty;
                //int blockindex = 0;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //var client = new RestClient("https://api.9cscan.com/status");
                //client.Timeout = -1;
                //var request = new RestRequest(Method.GET);
                //IRestResponse response = client.Execute(request);

                //var resultObjects = AllChildren(JObject.Parse(response.Content))
                //    .First(c => c.Type == JTokenType.Array && c.Path.Contains("nodes"))
                //    .Children<JObject>();

                //foreach (var entry in resultObjects)
                //{
                //    var indexstring = (string)entry["blockIndex"];
                //    if (indexstring != null)
                //    {
                //        if (blockindex < (int)entry["blockIndex"])
                //        {
                //            blockindex = (int)entry["blockIndex"];
                //            address = (string)entry["endpoint"];
                //        }
                //    }
                //}

                dataGridView1.Invoke(new Action(() => dataGridView1.Rows.Clear()));

                var nodes = LoadRPCALL();

                foreach (var node in nodes)
                {
                    try { 
                        dataGridView1.Invoke(new Action(() => dataGridView1.Rows.Add(node.address, node.active, node.difference, node.users)));
                        var lastrowindex = dataGridView1.Rows.Count;

                        switch (node.difference)
                        {
                            case double n when n >= 0 && n <= 100:
                            case double y when y >= -5 && y <= -0:
                                dataGridView1.Rows[lastrowindex - 1].Cells[2].Style.BackColor = Color.Green;
                                break;

                            case double n when n >= -15 && n <= -6:
                                dataGridView1.Rows[lastrowindex - 1].Cells[2].Style.BackColor = Color.DarkGoldenrod;
                                break;

                            case double y when y >= -100000 && y <= 16:
                                dataGridView1.Rows[lastrowindex - 1].Cells[2].Style.BackColor = Color.Red;
                                break;
                        }
                    }catch(Exception ex) { Console.ReadLine(); }
                }


                dataGridView1.Invoke(new Action(() => dataGridView1.Sort(dataGridView1.Columns[2], ListSortDirection.Descending)));
                dataGridView1.Invoke(new Action(() => this.dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = System.Windows.Forms.SortOrder.Descending));
                dataGridView1.Invoke(new Action(() => dataGridView1.AutoResizeColumns()));
                //dataGridView1.AutoResizeRows();

                var sw1 = Stopwatch.StartNew();
                int seconds = 120;
                for (int ix = 0; ix < 120; ++ix)
                {
                    seconds--;
                    await Task.Delay(1000);
                    ThreadHelperClass.SetText(this, timerLBL, seconds.ToString()+" Seconds");
                }
                sw1.Stop();
            }
        }

    }
}
