using Panacea.Core;
using Panacea.Modules.ClinicalButtons.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UserActivity;

namespace Panacea.Modules.ClinicalButtons
{
    public class ProcessMonitor
    {
        private PanaceaServices _core;
        private ActionButtonModel _action;
        private IdleTimer _processTimer;
        public event EventHandler<ActionButtonModel> ProcessCompleted;
        public ProcessMonitor(PanaceaServices core)
        {
            _core = core;
        }
        public async Task ProcessStart(ActionButtonModel action)
        {
            if (_action != null) throw new InvalidOperationException("Cannot start a new process if another one is running");
            _action = action;
            if (!string.IsNullOrEmpty(_action.Settings.PreRunScript))
            {
                try
                {
                    await ExecuteScriptAsync(_action.Settings.PreRunScript);
                }
                catch (Exception ex)
                {
                    _core.Logger.Error(this, $"PreRun script failed with message: {ex.Message}");
                    OnProcessCompleted();
                    return;
                }
            }
            try
            {
                var mainProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(_action.Settings.Path)
                    {
                        Arguments = _action.Settings.ProgramParams ?? ""
                    }
                };

                mainProcess.Start();
                mainProcess.WaitForInputIdle();
                _processTimer = new IdleTimer(TimeSpan.FromSeconds(_action.Settings.IdleTime * 1000 * 60));
                _processTimer.Start();

                _processTimer.Tick += ProcessTimerCheck;
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, $"Main process failed with message: {ex.Message}");
                OnProcessCompleted();
                return;
            }

        }

        private async Task<bool> IsRunning()
        {
            try
            {
                return await ExecuteScriptAsync(_action.Settings.IsRunningScript) == 0;
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, $"IsRunning script raised an exception with message: {ex.Message}");
                return false;
            }
        }

        private static Task<int> ExecuteScriptAsync(string multiLineScript)
        {
            if (multiLineScript == null) throw new ArgumentNullException(nameof(multiLineScript));
            return Task.Run(() =>
            {
                using (var p = new Process())
                {
                    var info = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    p.StartInfo = info;
                    p.Start();

                    using (var sw = p.StandardInput)
                    {
                        foreach (
                            var line in
                            multiLineScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!sw.BaseStream.CanWrite) continue;
                            sw.WriteLine(line);
                        }
                    }

                    p.WaitForExit();
                    return p.ExitCode;
                }
            });
        }

        private async void ProcessTimerCheck(object sender, EventArgs e)
        {
            var isRunning = await IsRunning();
            if (_action.Settings.IdleTime != 0 && isRunning)
            {
                await CloseCurrentAction();
                return;

            }
            if (isRunning)
            {
                _processTimer.Start();
                return;
            }

            await CloseCurrentAction();
            //Console.WriteLine(isRunning);
        }

        private static void InvokeInUiThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }


        private async Task CloseCurrentAction()
        {
            if (_action == null) return;
            try
            {
                if (!string.IsNullOrEmpty(_action.Settings.CloseScript))
                    await ExecuteScriptAsync(_action.Settings.CloseScript);
                //var isRunning = await IsRunning();
                //if (isRunning) Host.Logger.Error(this,$"Close script failed to close {_action.Name}");
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, $"Close script failed with message: {ex.Message}");
            }

            try
            {
                if (!string.IsNullOrEmpty(_action.Settings.PostRunScript))
                    await ExecuteScriptAsync(_action.Settings.PostRunScript);
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, $"PostRun script failed with message: {ex.Message}");
            }
            finally
            {
                OnProcessCompleted();
                _action = null;
            }
        }

        protected void OnProcessCompleted()
        {
            InvokeInUiThread(() =>
            {
                ProcessCompleted?.Invoke(this, _action);
            });
        }
    }
}
