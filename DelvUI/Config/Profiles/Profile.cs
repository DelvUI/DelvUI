using DelvUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DelvUI.Config.Profiles
{
    public class Profile
    {
        public string Name;

        public bool AutoSwitchEnabled = false;
        public AutoSwitchData AutoSwitchData = new AutoSwitchData();

        public Profile(string name, bool autoSwitchEnabled = false, AutoSwitchData? autoSwitchData = null)
        {
            Name = name;
            AutoSwitchEnabled = autoSwitchEnabled;
            AutoSwitchData = autoSwitchData ?? AutoSwitchData;
        }
    }

    public class AutoSwitchData
    {
        public Dictionary<JobRoles, List<bool>> Map;

        public AutoSwitchData()
        {
            Map = new Dictionary<JobRoles, List<bool>>();

            JobRoles[] roles = (JobRoles[])Enum.GetValues(typeof(JobRoles));

            foreach (JobRoles role in roles)
            {
                int count = JobsHelper.JobsByRole[role].Count;
                List<bool> list = new List<bool>(count);

                for (int i = 0; i < count; i++)
                {
                    list.Add(false);
                }

                Map.Add(role, list);
            }
        }

        public bool GetRoleEnabled(JobRoles role)
        {
            foreach (bool value in Map[role])
            {
                if (!value)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetRoleEnabled(JobRoles role, bool value)
        {
            for (int i = 0; i < Map[role].Count; i++)
            {
                Map[role][i] = value;
            }
        }
    }
}
