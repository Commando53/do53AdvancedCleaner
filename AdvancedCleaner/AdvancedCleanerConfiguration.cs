using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace do53AdvancedCleaner
{
    public class do53AdvancedCleanerConfiguration : IRocketPluginConfiguration, IDefaultable
    {
        public void LoadDefaults() { }
        public float DefaultRadius = 30;
        public bool AskForConfirmBeforeCleaning = true;
    }

}
