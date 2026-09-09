using UnityEngine;

namespace HighNoon
{
    /// <summary>
    /// One flush for settings + records + campaign. Mutators still own today's PlayerPrefs
    /// keys (<see cref="GameSettings"/>, <see cref="Records"/>, <see cref="Campaign.Save"/>)
    /// and call <see cref="Save"/> instead of <c>PlayerPrefs.Save</c> themselves — so a
    /// typed blob (economy, cosmetics) can land here later without a second save path.
    /// </summary>
    public static class SaveData
    {
        public static void Save() => PlayerPrefs.Save();
    }
}
