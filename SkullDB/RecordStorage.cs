using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkullDB;

public class RecordStorage
{
    [Required] public BlockStorage Storage { get; init; }

    public Record GetRecord(long i)
    {
        var block = Storage.GetBlock(i);
        var next = block.NextBlockId;
        var blocks = new List<Block>()
        {
            block
        };

        while (next > 0)
        {
            block = Storage.GetBlock(i);
            next = block.NextBlockId;
            blocks.Add(block);
        }

        return new Record()
        {
            Data = blocks.ToArray()
        };
    }

    public byte[] GetRecordData(long i)
    {
        var block = Storage.GetBlock(i);
        var next = block.NextBlockId;
        var bytes = new List<byte>();
        bytes.AddRange(block.Data);

        while (next > 0)
        {
            block = Storage.GetBlock(i);
            next = block.NextBlockId;
            bytes.AddRange(block.Data);
        }

        return bytes.ToArray();
    }

    public void SetRecord(Record record)
    {
        foreach (var block in record.Data)
        {
            Storage.SetBlock(block);
        }
    }
}

public class Record
{    
    public Block[] Data { get; set; }
}
