using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;


namespace DCore_API_Test
{
    public partial class Form1 : Form
    {
        List<TextBox> paramboxes = new List<TextBox>();

        public Form1()
        {
            InitializeComponent();
            paramboxes.Add(textBox_Parameter1);
            paramboxes.Add(textBox_Parameter2);
            paramboxes.Add(textBox_Parameter3);
            paramboxes.Add(textBox_Parameter4);
            paramboxes.Add(textBox_Parameter5);
            paramboxes.Add(textBox_Parameter6);

        }

        private async void button_Test_Click(object sender, EventArgs e)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var result = await httpClient.GetAsync(textBox_IP.Text);
                    int StatusCode = (int)result.StatusCode;
                    richTextBox_Response.Text = "Status Code: " + StatusCode;
                }
                catch
                {
                    richTextBox_Response.Text = "ERROR! Could not connect to specified URL.";
                }
            }

        }



        private void button_Execute_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(textBox_Method.Text))
            {
                richTextBox_Response.Text = "ERROR! No Method to test!";
            }
            else
            {
                List<string> parameters = new List<string>();
                for (int i = 0; i < 6; i++)
                {
                    if (!string.IsNullOrWhiteSpace(paramboxes[i].Text))
                    {
                        parameters.Add(paramboxes[i].Text);
                    }
                }

                displayCode(parameters);
            }
        }

        private void displayCode(List<string> paramList)
        {
            string values = "";
            if (paramList.Count != 0)
            {
                int x = 0;
                values = ",\"params\":[";
                foreach (string item in paramList)
                {
                    if (!item.Contains(','))
                    {
                        values = values + "\"" + item.Trim() + "\"";
                    }
                    else
                    {
                        string[] temp = item.Split(',');
                        values = values + "[";
                        foreach (string subitem in temp)
                        {
                            values = values + "\"" + subitem.Trim() + "\",";
                        }
                        values = values + "]";
                        values = values.Remove(values.Length - 2, 1);
                    }
                    x++;
                    if (x != paramList.Count)
                    {
                        values = values + ",";
                    }
                }
                values = values + "]";
            }

            richTextBox_Method.Clear();
            string tab = "  ";
            richTextBox_Method.AppendText("private async Task<string> APICall_" + textBox_Method.Text + "()\n");
            richTextBox_Method.AppendText("{\n");
            richTextBox_Method.AppendText(tab + "using (var httpClient = new HttpClient())\n");
            richTextBox_Method.AppendText(tab + "{\n");
            richTextBox_Method.AppendText(tab + tab + "try\n");
            richTextBox_Method.AppendText(tab + tab + "{\n");
            richTextBox_Method.AppendText(tab + tab + tab + "using  (var request = new HttpRequestMessage(new HttpMethod(\"POST\"), \"");
            richTextBox_Method.AppendText(textBox_IP.Text + "\"))\n");
            richTextBox_Method.AppendText(tab + tab + tab + "{\n");
            string format = values.Replace("\"", "\\\"");
            richTextBox_Method.AppendText(tab + tab + tab + "string content = \"{\\\"jsonrpc\\\":\\\"2.0\\\",\\\"id\\\":1,\\\"method\\\":\\\"");
            richTextBox_Method.AppendText(textBox_Method.Text + "\\\"" + format + "}\";\n");
            richTextBox_Method.AppendText(tab + tab + tab + "request.Content = new StringContent(content, Encoding.UTF8, \"application/x-www-form-urlencoded\");\n");
            richTextBox_Method.AppendText(tab + tab + tab + "var response = await httpClient.SendAsync(request);\n");
            richTextBox_Method.AppendText(tab + tab + tab + "response.EnsureSuccessStatusCode();\n");
            richTextBox_Method.AppendText(tab + tab + tab + "string responseBody = await response.Content.ReadAsStringAsync();\n");
            richTextBox_Method.AppendText(tab + tab + tab + "return responseBody;\n");
            richTextBox_Method.AppendText(tab + tab + tab + "}\n");
            richTextBox_Method.AppendText(tab + tab + "}\n");
            richTextBox_Method.AppendText(tab + tab + "catch\n");
            richTextBox_Method.AppendText(tab + tab + "{\n");
            richTextBox_Method.AppendText(tab + tab + tab + "return \"Error\";\n");
            richTextBox_Method.AppendText(tab + tab + "}\n");
            richTextBox_Method.AppendText(tab + "}\n");
            richTextBox_Method.AppendText("}\n");

            callAPI(values);
        }

        private async void callAPI(string parameterString)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), textBox_IP.Text))
                    {
                        string content = "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"" + textBox_Method.Text + "\"" + parameterString + "}";
                        request.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
                        var response = await httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        richTextBox_Response.Text = FormatJson(responseBody);
                    }
                }
                catch
                {
                    richTextBox_Response.AppendText("An unknown error occured");
                }
            }
        }

        private const string INDENT_STRING = "    ";
        static string FormatJson(string json)
        {

            int indentation = 0;
            int quoteCount = 0;
            var result =
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(INDENT_STRING, --indentation)) + ch : ch.ToString()
                select lineBreak == null
                            ? openChar.Length > 1
                                ? openChar
                                : closeChar
                            : lineBreak;

            return String.Concat(result);
        }
    }
}
