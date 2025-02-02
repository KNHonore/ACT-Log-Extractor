﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ACT_Log_Extractor
{
    internal class LogParser
    {

        List<Tuple<string, string>> list = new List<Tuple<string, string>>();

        public LogParser(MainForm form)
        {
            refresh(form);
        }

        public void refresh(MainForm form)
        {
            Debug.WriteLine("Path: " + getLogDirPath());

            string[] files = Directory.GetFiles(getLogDirPath(), "*.log", SearchOption.AllDirectories);

            form.listBox_logs.Items.Clear();

            long size = 0;

            foreach (string file in files)
            {
                Debug.WriteLine("Adding item: " + getDate(file));
                string displayName = getDate(file);
                list.Add(new Tuple<string, string>(displayName, file));
                form.listBox_logs.Items.Add(getDate(file));
                size += new System.IO.FileInfo(file).Length;
            }

            form.label_size.Text = "Total size: " + (size / (1024 * 1024)).ToString() + " MB";
        }

        internal void parse(string filePath, MainForm form, bool html)
        {
            if (form.listBox_logs.SelectedItem == null)
            {
                MessageBox.Show("No log file selected.", "Error");
                return;
            }

            Debug.WriteLine("Filepath: " + getFile(form.listBox_logs.SelectedItem.ToString()));
            var lines = File.ReadAllLines(getFile(form.listBox_logs.SelectedItem.ToString()));

            bool useOldRegex = false;
            Debug.WriteLine("File creation date: " + ConvertToUnixTime(File.GetCreationTime(getFile(form.listBox_logs.SelectedItem.ToString()))));
            if (ConvertToUnixTime(File.GetLastWriteTime(getFile(form.listBox_logs.SelectedItem.ToString()))) < 1484136000)
            {
                useOldRegex = true;
                Debug.WriteLine("Using old regex.");
            }


            Match match;
            Debug.WriteLine("Number of lines:" + lines.Length);

            String fileText = "";

            foreach (var line in lines)
            {
                if (useOldRegex) match = new Regex(@"00\|\d+-\d+-\d+T(?<time>\d+:\d+:\d+).+?\|(?<code>\d+)\|.*?(?<name>[A-Z][A-z']+? [A-z']+).*?\|(?<message>.+)").Match(line);
                else match = new Regex(@"00\|\d+-\d+-\d+T(?<time>\d+:\d+:\d+).+?\|(?<code>\d+)\|.*?(?<name>[A-Z][A-z']+? [A-z']+).*?\|(?<message>.+)\|[^\|]+").Match(line);
                if (match.Success)
                {
                    String outputLine = constructLine(match, form, html);

                    if (!outputLine.Equals(""))
                    {
#if DEBUG
                        Debug.WriteLine("LINE: " + outputLine);
#else
                        fileText += outputLine;
#endif
                    }

                }

            }
            Debug.WriteLine("Parse complete.");
#if Release
            saveFile(fileText);
#endif
        }

        private void parseCodes(Match match, string filePath, bool separate)
        {
            //string processedLine = constructLine(match);

            // Say
            // Party
            // Alliance
            // Yell
            // Shout
            // Free Company
            // Tell
            // Yell
            // Lindshell 1
            // Lindshell 2
            // Lindshell 3
            // Lindshell 4
            // Lindshell 5
            // Lindshell 6
            // Lindshell 7
            // Lindshell 8


        }

        private string constructLine(Match match, MainForm form, bool html)
        {
            string completeString = "";


            if ((form.checkBox_alliance.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Alliance")) ||
                (form.checkBox_freeCompany.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Free Company")) ||
                (form.checkBox_linkshell1.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 1")) ||
                (form.checkBox_linkshell2.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 2")) ||
                (form.checkBox_linkshell3.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 3")) ||
                (form.checkBox_linkshell4.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 4")) ||
                (form.checkBox_linkshell5.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 5")) ||
                (form.checkBox_linkshell6.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 6")) ||
                (form.checkBox_linkshell7.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 7")) ||
                (form.checkBox_linkshell8.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Linkshell 8")) ||
                (form.checkBox_party.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Party")) ||
                (form.checkBox_say.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Say")) ||
                (form.checkBox_shout.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Shout")) ||
                (form.checkBox_tell.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Tell")) ||
                (form.checkBox_yell.Checked && codeToChannel(match.Groups["code"].ToString()).Equals("Yell"))
                )
            {
                String color;
                if (html)
                {
                    color = String.Format("{0,6:X}", ((0xeeeeee & match.Groups["name"].ToString().GetHashCode())));
                    completeString += "<font color=\"" + color + "\">";
                }

                if (form.checkBox_timestamps.Checked)
                {
                    completeString += "[" + match.Groups["time"].ToString() + "]";
                }
                if (form.checkBox_channel.Checked)
                {
                    completeString += "[" + codeToChannel(match.Groups["code"].ToString()) + "]";
                }
                if (form.checkBox_names.Checked)
                {
                    if(html) completeString += "&lt;" + match.Groups["name"].ToString() + "&gt; ";

                    completeString += "<" + match.Groups["name"].ToString() + "> ";
                }
                completeString += match.Groups["message"].ToString();
                if(html) completeString += "</font> <br>";
            }


            return completeString;
        }

        private string codeToChannel(string code)
        {
            if (code.Equals("000a")) return "Say";
            if (code.Equals("000e")) return "Party";
            if (code.Equals("000f")) return "Alliance";
            if (code.Equals("001e")) return "Yell";
            if (code.Equals("000b")) return "Shout";
            if (code.Equals("0018")) return "Free Company";
            if (code.Equals("000C")) return "Tell";
            if (code.Equals("0025")) return "Linkshell 1";
            if (code.Equals("0011")) return "Linkshell 2";
            if (code.Equals("0012")) return "Linkshell 3";
            if (code.Equals("0013")) return "Linkshell 4";
            if (code.Equals("0014")) return "Linkshell 5";
            if (code.Equals("0015")) return "Linkshell 6";
            if (code.Equals("0016")) return "Linkshell 7";
            if (code.Equals("0017")) return "Linkshell 8";
            //TODO Add mentor chat
            //TODO Add Emotes and /em chat 
            return "";
        }

        private string getDate(string file)
        {
            Regex regex = new Regex(@".+FFXIVLogs\\[A-z]+_(?<date>\d+)\.");
            Match match = regex.Match(file);
            return match.Groups["date"].Value;
        }

        private string getFile(string date)
        {
            foreach (var entry in list)
            {
                if (entry.Item1.Equals(date))
                {
                    return entry.Item2;
                }
            }
            return null;
        }

        private string getLogDirPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Roaming\Advanced Combat Tracker\FFXIVLogs";
        }

        public static long ConvertToUnixTime(DateTime input)
        {
            DateTime t = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(input - t).TotalSeconds;
        }
    }
}