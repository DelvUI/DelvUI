using System.Collections.Generic;

namespace DelvUI.Config.Profiles
{
    public class Profile
    {
        public readonly string Name;

        public bool AutoSwitchEnabled = false;
        public List<uint> AutoSwitchJobIds = new List<uint>();

        public Profile(string name, bool autoSwitchEnabled = false, List<uint>? autoSwitchJobIds = null)
        {
            Name = name;
            AutoSwitchEnabled = autoSwitchEnabled;
            AutoSwitchJobIds = autoSwitchJobIds ?? AutoSwitchJobIds;
        }
    }
}
