using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SkullDB;

public class SkullDB
{
    public static void Main(string[] args)
    {
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".skulldb");
        var dbpath = Path.Combine(folderPath, "database.sdb");

        Directory.CreateDirectory(folderPath);

        var stream = File.Open(dbpath, FileMode.OpenOrCreate);

        var database = new Database(stream);

        var rec1 = database.Storage.WriteData(Encoding.UTF8.GetBytes("Hallo, Welt!"));
        database.Storage.UpdateRecord(rec1, Encoding.UTF8.GetBytes("Hello, world!"));
        var rec2 = database.Storage.WriteData(Encoding.UTF8.GetBytes("How are you?"));
        database.Storage.DeleteRecord(rec2);
        var rec3 = database.Storage.WriteData(Encoding.UTF8.GetBytes("Hallo, Welt!"));
    }
}