using Panacea.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.ClinicalButtons.Models
{
    [DataContract]
    public class ActionButtonModel : ServerItem
    {
        [DataMember(Name = "actionType")]
        public string ActionType { get; set; }

        [DataMember(Name = "img")]
        public string ImgUrl { get; set; }

        [DataMember(Name = "settings")]
        public ActionSetting Settings { get; set; }
    }
}
