using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SkullDB;

public class Database
{
    public static void Main(string[] args)
    {
        var dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".skulldb", "database.sdb");
        var rspath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".skulldb", "database.srs");

        BlockStorage storage = new()
        {
            BlockSize = 4096,
            Stream = File.Open(dbpath, FileMode.OpenOrCreate)
        };

        RecordStorage recordStorage = new()
        {
            Storage = storage
        };

        storage.SetBlock(new Block()
        {
            Id = 0,
            Data = new byte[]
            {
                1,3,4,5,6,7,8,9,10,11,12
            }
        });

        var record1 = new Record()
        {
            Data = new Block[]
            {
                new Block()
                {
                    Id = 1,
                    Data = Encoding.UTF8.GetBytes("Record 1 Block 1")
                },
                new Block()
                {
                    Id = 3,
                    Data = Encoding.UTF8.GetBytes("Record 1 Block 2")
                },
                new Block()
                {
                    Id = 4,
                    Data = Encoding.UTF8.GetBytes("Record 1 Block 3")
                },
            }
        };

        var record2 = new Record()
        {
            Data = new Block[]
            {
                new Block()
                {
                    Id = 2,
                    Data = Encoding.UTF8.GetBytes("Record 2 Block 1")
                },
                new Block()
                {
                    Id = 5,
                    Data = Encoding.UTF8.GetBytes("Record 2 Block 2")
                },
                new Block()
                {
                    Id = 6,
                    Data = Encoding.UTF8.GetBytes("Record 2 Block 3")
                },
            }
        };

        recordStorage.SetRecord(record1);
        recordStorage.SetRecord(record2);

        foreach (var b in storage.GetBlock(0).Data) Console.WriteLine(b);

        Console.WriteLine(recordStorage.GetRecord(1).Data.Length);
        Console.WriteLine(recordStorage.GetRecord(2).Data.Length);

        Console.WriteLine(Encoding.UTF8.GetString(recordStorage.GetRecordData(1)));
        Console.WriteLine(Encoding.UTF8.GetString(recordStorage.GetRecordData(2)));
    }
}