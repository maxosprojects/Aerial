using ScreenSaver;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aerial
{
    class ScreenSaverApp
    {
        private List<ScreenSaverForm> screenSaverForms = new List<ScreenSaverForm>();

        public void Run(string[] args)
        {
            if (args.Length > 0)
            {
                string firstArgument = args[0].ToLower().Trim();
                string secondArgument = null;

                // Handle cases where arguments are separated by colon. 
                // Examples: /c:1234567 or /P:1234567
                if (firstArgument.Length > 2)
                {
                    secondArgument = firstArgument.Substring(3).Trim();
                    firstArgument = firstArgument.Substring(0, 2);
                }
                else if (args.Length > 1)
                    secondArgument = args[1];

                if (firstArgument == "/c")           // Configuration mode
                {
                    var settings = new SettingsForm();
                    settings.StartPosition = FormStartPosition.CenterScreen;
                    Application.Run(settings);
                }
                else if (firstArgument == "/p")      // Preview mode
                {
                    Application.Exit();
                    //if (secondArgument == null)
                    //{
                    //    MessageBox.Show("Sorry, but the expected window handle was not provided.",
                    //        "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //    return;
                    //}

                    //IntPtr previewWndHandle = new IntPtr(long.Parse(secondArgument));
                    //Application.Run(new ScreenSaverForm(previewWndHandle));
                }
                else if (firstArgument == "/s")      // Full-screen mode
                {
                    ShowScreenSaver();
                    Application.Run();
                }
                else if (firstArgument == "/w") // if executable, windowed mode.
                {
                    ScreenSaverForm form = new ScreenSaverForm(ScreenSaverForm_KeyPress, WindowMode: true);
                    screenSaverForms.Add(form);
                    form.MainScreen = true;
                    Application.Run(form);
                }
                else    // Undefined argument
                {
                    MessageBox.Show("Sorry, but the command line argument \"" + firstArgument +
                        "\" is not valid.", "ScreenSaver",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                if (System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.EndsWith("exe")) // treat like /w
                {
                    ScreenSaverForm form = new ScreenSaverForm(ScreenSaverForm_KeyPress, WindowMode: true);
                    screenSaverForms.Add(form);
                    form.MainScreen = true;
                    Application.Run(form);
                }
                else // No arguments - treat like /c
                {
                    Application.Run(new SettingsForm());
                }
            }
        }

        /// <summary>
        /// Display the form on each of the computer's monitors.
        /// </summary>
        private void ShowScreenSaver()
        {
            var multiMonitorMode = new RegSettings().MultiMonitorMode;

            switch (multiMonitorMode)
            {
                case RegSettings.MultiMonitorModeEnum.SameOnEach:
                case RegSettings.MultiMonitorModeEnum.DifferentVideos:
                    {
                        var allScreens = Screen.AllScreens;
                        if (allScreens.Length < 1)
                        {
                            throw new System.Exception("No screens found to display on");
                        }
                        for (var index = 0; index < allScreens.Length; index++)
                        {
                            var screen = allScreens[index];
                            ScreenSaverForm form = new ScreenSaverForm(screen.Bounds, shouldCache: screen.Primary, showVideo: true, keyPressEventHandler: ScreenSaverForm_KeyPress);
                            screenSaverForms.Add(form);
                            if (index == 0)
                            {
                                form.MainScreen = true;
                            }
                            form.Show();
                        }
                        break;
                    }
                case RegSettings.MultiMonitorModeEnum.SpanAll:
                    {
                        ScreenSaverForm form = new ScreenSaverForm(Screen.AllScreens.GetBounds(), shouldCache: true, showVideo: true, keyPressEventHandler: ScreenSaverForm_KeyPress);
                        screenSaverForms.Add(form);
                        form.MainScreen = true;
                        form.Show();
                        break;
                    }
                case RegSettings.MultiMonitorModeEnum.MainOnly:
                default:
                    {
                        foreach (var screen in Screen.AllScreens)
                        {
                            ScreenSaverForm form = new ScreenSaverForm(screen.Bounds, shouldCache: screen.Primary, showVideo: screen.Primary, keyPressEventHandler: ScreenSaverForm_KeyPress);
                            screenSaverForms.Add(form);
                            form.MainScreen = true;
                            form.Show();
                        }
                        break;
                    }
            }
        }

        private void ScreenSaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'n')
            {
                foreach (ScreenSaverForm form in screenSaverForms)
                {
                    if (form.MainScreen)
                    {
                        form.SetNextVideo(true);
                    }
                }
            }
            else if (e.KeyChar == 'm')
            {
                foreach (ScreenSaverForm form in screenSaverForms)
                {
                    if (!form.MainScreen)
                    {
                        form.SetNextVideo(true);
                    }
                }
            }
            else if (e.KeyChar == 'i')
            {
                foreach (ScreenSaverForm form in screenSaverForms)
                {
                    form.ShowInfo();
                }
            }
            else if (screenSaverForms.Count > 0 && screenSaverForms[0].ShouldExit())
            {
                Application.Exit();
            }
        }
    }
}
