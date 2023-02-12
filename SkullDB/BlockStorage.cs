using System.ComponentModel.DataAnnotations;

namespace SkullDB;
public class BlockStorage
{
    public FileStream? Stream { get; init; }
    public int BlockSize { get; set; } = 1024;


    public int HeaderSize { get; private set; }= 4 + 8 + 8;
    public int ContentSize { get; private set; } = 0;

    public BlockStorage() {
        ContentSize = BlockSize - HeaderSize;
    }

    public Block GetBlock(long i)
    {
        if (Stream is not {})
            throw new ArgumentNullException("Stream was null");

        var metadata = GetBlockMetadata(i);

        var buffer = new byte[metadata.Length];

        Stream.Position = i * BlockSize + HeaderSize;

        Stream.Read(buffer, 0, metadata.Length);
        Stream.Position = 0;

        return new Block()
        {
            Metadata = metadata,

            Data = buffer
        };
    }

    public BlockMetadata GetBlockMetadata(long i)
    {
        if (Stream is not {})
            throw new ArgumentNullException("Stream was null");

        var buffer = new byte[4];
        var length = 0;
        long nextBlockId = 0;
        long prevBlockId = 0;

        Stream.Position = i * BlockSize;

        Stream.Read(buffer, 0, 4);
        length = BitConverter.ToInt32(buffer, 0);
        buffer = new byte[8];

        Stream.Read(buffer, 0, 8);
        nextBlockId = BitConverter.ToInt64(buffer, 0);
        buffer = new byte[8];

        Stream.Read(buffer, 0, 8);
        prevBlockId = BitConverter.ToInt64(buffer, 0);
        buffer = new byte[length];

        Stream.Read(buffer, 0, length);
        Stream.Position = 0;

        return new BlockMetadata()
        {
            Id = i,

            NextBlockId = nextBlockId,
            PrevBlockId = prevBlockId,

            Length = length
        };
    }

    public void SetBlock(Block block)
    {
        if (Stream is not {})
            throw new ArgumentNullException("Stream was null");

        Stream.Position = block.Metadata.Id * BlockSize;
        Stream.Write(BitConverter.GetBytes(block.Data.Length));
        Stream.Write(BitConverter.GetBytes(block.Metadata.NextBlockId));
        Stream.Write(BitConverter.GetBytes(block.Metadata.PrevBlockId));
        Stream.Write(block.Data);
        for (int j = 0; j < BlockSize - block.Data.Length - HeaderSize; j++) Stream.WriteByte(0);
        Stream.Position = 0;
    }
}

public class Block
{
    public BlockMetadata Metadata { get; set; } = new BlockMetadata();

    public byte[] Data { get; set; } = new byte[0];
}

public class BlockMetadata {
    public long Id { get; set; } = 0;

    public long NextBlockId { get; set; } = 0;
    public long PrevBlockId { get; set; } = 0;

    public int Length { get; set; } = 0;
}
