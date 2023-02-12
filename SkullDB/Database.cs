namespace SkullDB;

public class Database {
    public RecordStorage Storage { get; set; }

    public static Database FromStream(FileStream stream) 
    {
        BlockStorage storage = new()
        {
            Stream = stream
        };

        return new Database(new RecordStorage(storage));
    }

    public Database(FileStream stream) 
    {
        BlockStorage storage = new()
        {
            Stream = stream
        };

        Storage = new RecordStorage(storage);

        Storage.SetRecord(new Record() {
            Data = new Block[] {
                new Block() {
                    Metadata = new BlockMetadata() {
                        Id = 0
                    },
                    Data = new byte[0]
                }
            }
        });
    }

    public Database(RecordStorage storage) 
    {
        Storage = storage;
    }
}