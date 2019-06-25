using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modules.ClinicalButtons.Models;
using Panacea.Modules.ClinicalButtons.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.ClinicalButtons.ViewModels
{
    [View(typeof(ClinicalButton))]
    class ClinicalButtonViewModel : ViewModelBase
    {
        private PanaceaServices core;
        private ActionButtonModel actionButton;

        public string Name { get; private set; }
        public string PluginName { get; private set; }
        public string ImgUrl { get; private set; }

        public ClinicalButtonViewModel(PanaceaServices core, ClinicalButtonsPlugin plugin, ActionButtonModel actionButton)
        {
            this.core = core;
            this.actionButton = actionButton;
            Name = actionButton.Name;
            PluginName = actionButton.Settings.PluginName;
            ImgUrl = core.HttpClient.RelativeToAbsoluteUri(actionButton.ImgUrl);
            OpenCommand = new RelayCommand(args =>
            {
                plugin.OpenItem(actionButton);
            });
        }

        public RelayCommand OpenCommand { get; }
    }
}
