using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Kbg.NppPluginNET.PluginInfrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kbg.NppPluginNET
{
    class Main
    {
        internal const string PluginName = "Json2String";

        static string userConfigPath = null;
        static int idMyDlg = -1;


        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
        }

        internal static void CommandMenuInit()
        {
            // config folder
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            userConfigPath = sbIniFilePath.ToString();
            if (!Directory.Exists(userConfigPath)) Directory.CreateDirectory(userConfigPath);

            // menu items
            //PluginBase.SetCommand(0, "MyMenuCommand", myMenuFunction, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(0, "STRING to CODE", string2Json); 
            PluginBase.SetCommand(1, "CODE to STRING", json2String);

            idMyDlg = 0;
        }

        internal static void SetToolBarIcon()
        {
            // create struct
            toolbarIcons tbIcons = new toolbarIcons();

            // convert to c++ pointer
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);

            // call Notepad++ api
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON_FORDARKMODE, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);

            // release pointer
            Marshal.FreeHGlobal(pTbIcons);
        }

        internal static void PluginCleanUp()
        {
            // any clean up code here
        }

        internal static void string2Json()
        {
            ScintillaGateway editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            string originalTtext = editor.GetText(editor.GetTextLength()+1);
            if (!string.IsNullOrEmpty(originalTtext))
            {
                try
                {
                    string text = originalTtext.Substring(1, originalTtext.Length - 2).Replace(@"\""", "\"");
                    JObject json = JObject.Parse(text);
                    string formatted = JsonConvert.SerializeObject(json, Formatting.Indented);
                    editor.SetText(formatted);
                }
                catch
                {
                    MessageBox.Show("ERROR: When trying to convert from String to JSON.");
                }
            }
        }

        internal static void json2String()
        {
            ScintillaGateway editor = new ScintillaGateway(PluginBase.GetCurrentScintilla());
            string originalTtext = editor.GetText(editor.GetTextLength() + 1);
            if (!string.IsNullOrEmpty(originalTtext))
            {
                try
                {
                    JObject json = JObject.Parse(originalTtext);
                    string jsonString = parseJson(json.ToString(Formatting.None));
                    editor.SetText(jsonString);
                }
                catch
                {
                    MessageBox.Show("ERROR: When trying to convert from JSON to String.");
                }
                
            }
        }

        internal static string parseJson(string json)
        {
            string input = json.Replace("\\s", "").Replace("\"", "\\\"");
            return "\"" + input + "\"";
        }

        public static string GetVersion()
        {
            // version for example "1.3.0.0"
            string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // if 4 version digits, remove last two ".0" if any, example  "1.3.0.0" ->  "1.3" or  "2.0.0.0" ->  "2.0"
            while ((ver.Length > 4) && (ver.Substring(ver.Length - 2, 2) == ".0"))
            {
                ver = ver.Substring(0, ver.Length - 2);
            }
            return ver;
        }
    }
}