using System.ComponentModel.DataAnnotations;

namespace SkullDB;
public class BlockStorage
{
    [Required] public FileStream Stream { get; init; }
    public int BlockSize { get; set; } = 4096;

    public Block GetBlock(long i)
    {
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

        return new Block()
        {
            NextBlockId = nextBlockId,
            PrevBlockId = prevBlockId,

            Id = i,

            Data = buffer
        };
    }

    public void SetBlock(Block block)
    {
        Stream.Position = block.Id * BlockSize;
        Stream.Write(BitConverter.GetBytes(block.Data.Length));
        Stream.Write(BitConverter.GetBytes(block.NextBlockId));
        Stream.Write(BitConverter.GetBytes(block.PrevBlockId));
        Stream.Write(block.Data);
        for (int j = 0; j < BlockSize - block.Data.Length - 8 - 16 - 16; j++) Stream.WriteByte(0);
        Stream.Position = 0;
    }
}
public class Block
{
    public long Id = 0;

    public long NextBlockId = 0;
    public long PrevBlockId = 0;

    public byte[] Data;
}
