using System.Runtime.Serialization;

namespace Panacea.Modules.ClinicalButtons.Models
{
    [DataContract]
    public class ActionSetting
    {
        [DataMember(Name = "isRunningScript")]
        public string IsRunningScript { get; set; }

        [DataMember(Name = "closeProgramScript")]
        public string CloseScript { get; set; }

        [DataMember(Name = "pausePanacea")]
        public bool PausePanacea { get; set; }

        [DataMember(Name = "idleTimeout")]
        public int IdleTime { get; set; }

        [DataMember(Name = "pluginName")]
        public string PluginName { get; set; }

        [DataMember(Name = "programParams")]
        public string ProgramParams { get; set; }

        [DataMember(Name = "preRunScript")]
        public string PreRunScript { get; set; }

        [DataMember(Name = "postRunScript")]
        public string PostRunScript { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }
    }
}
