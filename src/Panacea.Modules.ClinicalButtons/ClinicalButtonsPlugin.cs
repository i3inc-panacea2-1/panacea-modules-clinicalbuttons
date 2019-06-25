using Panacea.Core;
using Panacea.Models;
using Panacea.Modularity;
using Panacea.Modularity.UiManager;
using Panacea.Modularity.WebBrowsing;
using Panacea.Modules.ClinicalButtons.Models;
using Panacea.Modules.ClinicalButtons.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.ClinicalButtons
{
    class ClinicalButtonsPlugin : IPlugin
    {
        private PanaceaServices _core;
        private List<ActionButtonModel> _actionButtons = new List<ActionButtonModel>();
        private readonly List<ActionButtonModel> _actionsRunning = new List<ActionButtonModel>();

        public Task BeginInit()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            return;
        }

        public async Task EndInit()
        {
            await GetSidebar();
            SetupButtons();
            return;
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }

        public ClinicalButtonsPlugin(PanaceaServices core)
        {
            _core = core;
        }
        public async void OpenItem(ServerItem item)
        {
            if (!(item is ActionButtonModel)) return;
            var action = (ActionButtonModel)item;
            switch (action.ActionType)
            {
                case "plugin":
                    var plugin = _core.PluginLoader.LoadedPlugins.FirstOrDefault(s => { return s.Key == action.Settings.PluginName; });
                    if (plugin.Value != null)
                    {
                        if (plugin.Value is ICallablePlugin)
                        {
                            (plugin.Value as ICallablePlugin).Call();
                        }
                    }
                    //Host.CallPlugin(action.Settings.PluginName);
                    break;
                case "program":
                    if (_actionsRunning.Contains(action)) return;
                    _actionsRunning.Add(action);
                    var actionMonitor = new ProcessMonitor(_core);
                    actionMonitor.ProcessCompleted += ActionMonitorOnProcessCompleted;
                    if (action.Settings.PausePanacea) Pause();
                    await actionMonitor.ProcessStart(action);
                    break;
                case "web":
                    if (_core.TryGetWebBrowser(out IWebBrowserPlugin web)){
                        web.OpenUnmanaged(action.Settings.Url); //TODO set "show-nav-bar" = false ??
                    }
                    break;
            }

        }
        private void ActionMonitorOnProcessCompleted(object sender, ActionButtonModel action)
        {
            if (action == null) return;
            var monitor = sender as ProcessMonitor;
            if (monitor != null)
            {
                monitor.ProcessCompleted -= ActionMonitorOnProcessCompleted;
            }

            _actionsRunning.Remove(action);
            if (action.Settings.PausePanacea) Resume();
        }
        private void SetupButtons()
        {
            if (_actionButtons == null) return;
            foreach (var actionButton in _actionButtons)
            {
                var imgUrl = _core.HttpClient.RelativeToAbsoluteUri(actionButton.ImgUrl);
                if(_core.TryGetUiManager(out IUiManager ui)){
                    ui.AddNavigationBarControl(new ClinicalButtonViewModel(_core, this, actionButton));
                }
            }
        }
        private async Task GetSidebar()
        {
            try
            {
                var response =
                    await _core.HttpClient.GetObjectAsync<List<ActionButtonModel>>("clinical_button/get_sidebar_options/");
                if (response.Success)
                    _actionButtons = response.Result;
            }
            catch
            {
                _core.Logger.Error(this, "Failed to get settings");
                await Task.Delay(30000);
                await GetSidebar();
            }
        }
        private void Pause()
        {
            if(_core.TryGetUiManager(out IUiManager ui))
            {
                if (ui.IsPaused) return;
                ui.Pause();
            }
        }

        private void Resume()
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                if (!ui.IsPaused) return;
                if (_actionsRunning.Any(r => r.Settings.PausePanacea)) return;
                ui.Resume();
            }
        }
    }
}
