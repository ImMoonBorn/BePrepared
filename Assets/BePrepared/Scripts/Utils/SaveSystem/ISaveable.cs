namespace MoonBorn.BePrepared.Utils.SaveSystem
{
    public interface ISaveable
    {
        void SaveState(string guid);
        void LoadState(object saveData);
    }
}
