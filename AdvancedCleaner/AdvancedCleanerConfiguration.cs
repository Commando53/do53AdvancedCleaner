using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;

namespace AdvancedCleaner
{
    public class Configuration : IRocketPluginConfiguration, IDefaultable
    {
        public void LoadDefaults() { }
        public float DefaultRadius = 30;
    }
}
