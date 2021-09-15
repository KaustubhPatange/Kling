using Loamen.KeyMouseHook;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using Adb_gui_Apkbox_plugin;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Components;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Kling
{
    public class Context : ApplicationContext
    {
        private Window _hiddenWindow;
        private System.ComponentModel.IContainer _components;
        private NotifyIcon _notifyIcon;
        private display keyui;
        Timer timer;
        private ContextMenuStrip _contextmenustrip;

        private IKeyboardMouseEvents m_GlobalHook;
        private readonly KeyMouseFactory eventHookFactory = new KeyMouseFactory(Hook.GlobalEvents());
        private readonly KeyboardWatcher keyboardWatcher;
        private List<MacroEvent> _macroEvents;

        bool isaboutshowing = false, specialkeys = false, suppresskey = false, suppresskey2 = false, settingshowing = false,
        notify = true, stdkeys = true; int displaytime = 2; bool record = true; bool logkeys = false;
        System.Drawing.Point location; Components.SettingsUI settingsui;
        private float opacity = 0.7f;
        public Context()
        {
            _components = new System.ComponentModel.Container();

            // Load Settings, but first create if not exist
            if (!File.Exists(@"config.ini"))
            {
                var height = SystemInformation.VirtualScreen.Height;
                File.WriteAllText(@"config.ini",
                    "[Settings]" + Environment.NewLine +
                    "displayindex=2" + Environment.NewLine +
                    "xaxis=20" + Environment.NewLine +
                    "yaxis=" + (height - new display().Height - 60) + Environment.NewLine +
                    "displaytime=2" + Environment.NewLine +
                    "notify=True" + Environment.NewLine +
                    "logkeys=True" + Environment.NewLine +
                    "opacity=0.7" + Environment.NewLine +
                    "stdkeys=True" + Environment.NewLine);
            }
            if (File.Exists(@"config.ini"))
            {
                var myini = new IniFile(@"config.ini");
                location = new System.Drawing.Point(
                    Convert.ToInt16(myini.Read("xaxis", "Settings")),
                    Convert.ToInt16(myini.Read("yaxis", "Settings"))
                    );
                displaytime = Convert.ToInt16(myini.Read("displaytime", "Settings"));
                notify = Convert.ToBoolean(myini.Read("notify", "Settings"));
                stdkeys = Convert.ToBoolean(myini.Read("stdkeys", "Settings"));
                logkeys = Convert.ToBoolean(myini.Read("logkeys", "Settings"));
                opacity = Convert.ToSingle(myini.Read("opacity", "Settings"));
            }

            LoadKeyConfigs();
                
            keyui = new display();
            keyui.Opacity = opacity;
            keyui.Location = location;

            _contextmenustrip = new ContextMenuStrip();
            _contextmenustrip.Items.Add(NewToolStripItem("Stop recording", (o, s) =>
            {
                var firstItem = _contextmenustrip.Items[0];
                if (record)
                {
                    // Stop Recording
                    record = false;
                    firstItem.Text = "Start recording";
                    DisplayStatusMessage("Kling : Service stopped");
                }
                else
                {
                    // Start Recording
                    record = true;
                    firstItem.Text = "Stop recording";
                    DisplayStatusMessage("Kling : Service started");
                }
            }));
            _contextmenustrip.Items.Add(new ToolStripSeparator());
            _contextmenustrip.Items.Add(NewToolStripItem("Settings", ShowSettings));
            _contextmenustrip.Items.Add(NewToolStripItem("Restart", (o, s) => { System.Windows.Forms.Application.Restart(); }));
            _contextmenustrip.Items.Add(NewToolStripItem("About", (o, s) =>
            {
                // About screen dialog
                if (!isaboutshowing)
                {
                    isaboutshowing = true;
                    Components.AboutUI ui = new Components.AboutUI();
                    ui.Closing += (obj, ex) => { isaboutshowing = false; };
                    ui.ShowDialog();
                }
            }));
            _contextmenustrip.Items.Add(new ToolStripSeparator());
            _contextmenustrip.Items.Add(NewToolStripItem("Exit", (o, s) =>
            {
                // Exit Button
                if (eventHookFactory != null)
                    eventHookFactory.Dispose();
                System.Windows.Forms.Application.Exit();
            }));

            _notifyIcon = new NotifyIcon(_components)
            {
                ContextMenuStrip = _contextmenustrip,
                Icon = Kling.Properties.Resources.icon,
                Text = "Kling",
                Visible = true,
            };

            _notifyIcon.DoubleClick += ShowSettings;

            _hiddenWindow = new System.Windows.Window();
            _hiddenWindow.Hide();
            DisplayStatusMessage(_notifyIcon.Text + ": Start pressing keys");

            keyboardWatcher = eventHookFactory.GetKeyboardWatcher();
            keyboardWatcher.OnKeyboardInput += (s, e) =>
            {
                if (!record)
                    return;

                if (_macroEvents != null)
                    _macroEvents.Add(e);

                if (e.KeyMouseEventType == MacroEventType.KeyPress)
                {
                    var keyEvent = (KeyPressEventArgs)e.EventArgs;
                    if (e.KeyMouseEventType.ToString().Contains("KeyUp"))
                    {
                        // This will also show a form
                        DisplayKeys(getKeys(keyEvent.KeyChar.ToString()));
                    }
                }
                else
                {
                    var keyEvent = (System.Windows.Forms.KeyEventArgs)e.EventArgs;
                    if (e.KeyMouseEventType.ToString().Contains("KeyUp"))
                    {
                        // This will show a form
                        var keys = keyEvent.KeyCode;

                        // Suppress next event
                        if (suppresskey)
                        {
                            suppresskey = false;
                            return;
                        }

                        if (suppresskey2)
                        {
                            suppresskey2 = false;
                            return;
                        }
                        
                        if (isControlPressed(keyEvent, keys))
                        {
                            specialkeys = true;
                            if (isShiftPressed(keyEvent, keys))
                            {
                                if (isAltPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Ctrl + Shift + Alt + ", keys);
                                }
                                else showKey("Ctrl + Shift + ", keys);
                            }
                            else if (isAltPressed(keyEvent, keys))
                            {
                                if (isShiftPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Ctrl + Alt + Shift + ", keys);
                                }
                                else showKey("Ctrl + Alt + ", keys);
                            }
                            else DisplayKeys("Ctrl + " + getKeys(keys.ToString()));
                        }
                        else if (isAltPressed(keyEvent, keys))
                        {
                            specialkeys = true;
                            if (isShiftPressed(keyEvent, keys))
                            {
                                if (isControlPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Alt + Shift + Ctrl + ", keys);
                                }
                                else showKey("Alt + Shift + ", keys);
                            }
                            else if (isControlPressed(keyEvent, keys)) 
                            {
                                if (isShiftPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Alt + Ctrl + Shift + ", keys);
                                }else 
                                    showKey("Alt + Ctrl + ", keys);
                            }
                            else DisplayKeys("Alt + " + getKeys(keys.ToString()));
                        }
                        else if (isShiftPressed(keyEvent, keys))
                        {
                            specialkeys = true;
                            if (isControlPressed(keyEvent, keys)) 
                            {
                                if (isAltPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Shift + Ctrl + Alt + ", keys);
                                }else showKey("Shift + Ctrl + ", keys);
                            }
                            else if (isAltPressed(keyEvent, keys))
                            {
                                if (isControlPressed(keyEvent, keys))
                                {
                                    suppresskey2 = true;
                                    showKey("Shift + Alt + Ctrl + ", keys);
                                }else showKey("Shift + Alt + ", keys);
                            }
                            else DisplayKeys("Shift + " + getKeys(keys.ToString()));
                        }
                        else
                        {
                            if (!specialkeys)
                            {
                                DisplayKeys(getKeys(keys.ToString()));
                            }
                            else specialkeys = false;
                        }
                    }
                }
            };

            _macroEvents = new List<MacroEvent>();
            keyboardWatcher.Start(Hook.GlobalEvents());

            timer = new Timer();
            timer.Interval = displaytime * 1000;
            timer.Tick += async (o, e) =>
            {
                timer.Stop();
                while (keyui.Opacity > 0.0)
                {
                    await Task.Delay(20);
                    keyui.Opacity -= 0.05;
                }
                keyui.Opacity = 0;
                keyui.Hide();
                keyui.Opacity = opacity;
            };

            //Subscribe();
            //ThreadExit += (o, e) =>
            //{
            //    Unsubscribe();
            //};
        }

        private List<CodeUI> windows = new List<CodeUI>();
        public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
            m_GlobalHook.KeyDown += M_GlobalHook_KeyDown;
        }

        private void M_GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (var ui in windows)
            {
                ui.PushUp();
            }
            var codeUI = new CodeUI(displaytime)
                .SetText(e.KeyCode.ToString());
            codeUI.Closing += (o, ex) => { windows.Remove(codeUI); };
            //.SetLocation(location.X, location.Y);
            codeUI.Show();
            windows.Add(codeUI);
            Debug.WriteLine("KeyDown: " + e.KeyCode.ToString());
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Debug.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyPress -= GlobalHookKeyPress;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        private void showKey(string Text, Keys keys)
        {
            suppresskey = true;
            DisplayKeys(Text + getKeys(keys.ToString()));
        }
        private bool isControlPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Control && keys != Keys.RControlKey && keys != Keys.LControlKey &&
                        keys != Keys.Control && keys != Keys.ControlKey;
        }
        private bool isAltPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Alt && keys != Keys.RMenu && keys != Keys.LMenu &&
                      keys != Keys.Alt;
        }
        private bool isShiftPressed(KeyEventArgs keyEvent, Keys keys)
        {
            return keyEvent.Shift && keys != Keys.Shift && keys != Keys.LShiftKey &&
                    keys != Keys.RShiftKey && keys != Keys.ShiftKey;
        }
        private void DisplayKeys(string Text)
        {
            if (Text == null) return;
            keyui.Hide();
            timer.Stop();

            keyui.SetText(Text);
            if (logkeys)
                File.AppendAllText("app.log", $"[{DateTime.Now.ToString()}] {Text}{Environment.NewLine}");
            timer.Start();
            keyui.Show();
        }

        public string getKeys(string Text)
        {
            if (!stdkeys)
                return Text;
            if (!DoesContainKey(Text)) return null;
            if (Text.Length == 2)
            {
                if (Text.StartsWith("D"))
                    return Text.Substring(1);
            }
            else if (Text.StartsWith("NumPad"))
            {
                return Text.Substring(6);
            }
            switch (Text)
            {
                case "LMenu":
                    return "Alt";
                case "RMenu":
                    return "Alt";
                case "RControlKey":
                    return "Ctrl";
                case "LControlKey":
                    return "Ctrl";
                case "LShiftKey":
                    return "Shift";
                case "RShiftKey":
                    return "Shift";
                case "LWin":
                    return "Win";
                case "RWin":
                    return "Win";
                case "Add":
                    return "Num +";
                case "Subtract":
                    return "Num -";
                case "Divide":
                    return "Num /";
                case "Multiply":
                    return "Num *";
                // Oem Keys
                case "OemMinus":
                    return "-";
                case "Oemplus":
                    return "=";
                case "Oemtilde":
                    return "`";
                case "Oem5":
                    return "\\";
                case "Oem6":
                    return "]";
                case "OemOpenBrackets":
                    return "[";
                case "Oem1":
                    return ";";
                case "Oem7":
                    return "'";
                case "Oemcomma":
                    return ",";
                case "OemPeriod":
                    return ".";
                case "OemQuestion":
                    return "/";
            }
            return Text;
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            // Show Settings
            if (!settingshowing)
            {
                settingsui = new Components.SettingsUI(keyui.Height, keyui.Width);
                settingsui.Closing += (o, ex) => { settingshowing = false; };
                settingsui.ShowDialog();
            }
        }
        private ToolStripMenuItem NewToolStripItem(string Text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(Text);
            if (handler != null)
            {
                item.Click += handler;
            }
            return item;
        }
        private void DisplayStatusMessage(string text, string message = null)
        {
            _hiddenWindow.Dispatcher.Invoke(delegate
            {
                if (notify)
                {
                    _notifyIcon.BalloonTipText = text;
                    if (message != null)
                        _notifyIcon.Text = message;
                    // The timeout is ignored on recent Windows
                    _notifyIcon.ShowBalloonTip(3000);
                }
            });
        }

        private List<String> showKeys = new List<string>();
        private void LoadKeyConfigs()
        {
            var lines = File.ReadAllLines(@"keys.txt")
                .Where(c => !c.StartsWith("#") && !c.Contains(".") && !string.IsNullOrWhiteSpace(c)).ToList();
            foreach (var line in lines)
            {
                try
                {
                    var items = line.Split('\t');
                    if (items[2] == "1") showKeys.Add(items[0]);
                }
                catch 
                {
                    MessageBox.Show($"Couldn't parse line {line}, incorrect characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool DoesContainKey(string Text)
        {
            if (showKeys.Contains(Text)) return true;
            return false;
        }
    }
}
