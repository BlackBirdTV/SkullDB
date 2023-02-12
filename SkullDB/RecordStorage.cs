using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkullDB;

public class RecordStorage
{
    public BlockStorage Storage { get; init; }

    public RecordStorage(BlockStorage storage) 
    {
        Storage = storage;
    }

    public RecordMetadata GetRecord(long i)
    {
        var block = Storage.GetBlockMetadata(i);
        var next = block.NextBlockId;
        var blocks = new List<BlockMetadata>()
        {
            block
        };

        while (next > 0)
        {
            block = Storage.GetBlockMetadata(next);
            next = block.NextBlockId;
            blocks.Add(block);
        }

        return new RecordMetadata()
        {
            Blocks = blocks.ToArray()
        };
    }

    public byte[] GetRecordData(long i)
    {
        var metadata = GetRecord(i);
        
        var data = GetRecordData(metadata);
        return data;
    }

    public byte[] GetRecordData(RecordMetadata metadata)
    {
        var bytes = new List<byte>();
        
        foreach (var block in metadata.Blocks) {
            bytes.AddRange(Storage.GetBlock(block.Id).Data);
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

    public RecordMetadata WriteData(byte[] data) {
        if (Storage.Stream is not {}) 
            throw new ArgumentNullException();

        var record0 = GetRecordData(0);
        var deletedRecords = new List<long>();

        var storageLength = Storage.Stream.Length / Storage.BlockSize + 1;

        var buffer = new byte[8];
        
        var i = 0;
        foreach (var b in record0) {
            buffer[i] = b;

            i++;

            if (i > 7) {
                deletedRecords.Add(
                    BitConverter.ToInt64(buffer)
                );
                i = 0;
            }
        }

        var indices = new List<long>();
        var blocksRequired = Math.Ceiling((decimal)data.Length / Storage.ContentSize);

        for (int j = 0; j < blocksRequired; j++) {
            if (deletedRecords.Count > 0) 
            {
                indices.Add(deletedRecords[0]);
                UndeleteBlock(deletedRecords[0]);
                deletedRecords.RemoveAt(0);
            }
            else
            {
                indices.Add(storageLength);
                storageLength += 1;
            }
        }

        var blockMetadatas = new List<BlockMetadata>();

        for (var j = 0; j < indices.Count; j++) {
            var dataStart = j * Storage.ContentSize;
            var dataEnd = dataStart + Storage.ContentSize;

            var metadata = new BlockMetadata() {
                Id = indices[j],
                NextBlockId = indices.Count > j + 1 ? indices[j + 1] : 0,
                PrevBlockId = j > 0 ? indices[j - 1] : 0
            };

            blockMetadatas.Add(metadata);

            Storage.SetBlock(new Block() {
                Metadata = metadata,
                Data = data[(dataStart)..(dataEnd >= data.Length ? data.Length : dataEnd)]
            });
        }

        return new RecordMetadata() {
            Blocks = blockMetadatas.ToArray()
        };
    }

    public RecordMetadata UpdateRecord(RecordMetadata metadata, byte[] data) {
        if (Storage.Stream is not {}) 
            throw new ArgumentNullException();

        var deletedRecords = GetDeletedBlocks();

        var storageLength = Storage.Stream.Length / Storage.BlockSize + 1;

        var buffer = new byte[8];

        var indices = metadata.Blocks.Select(b => b.Id).ToList();
        var blocksRequired = Math.Ceiling((decimal)data.Length / Storage.ContentSize);

        if (blocksRequired > indices.Count)
            for (int j = 0; j < blocksRequired; j++) {
                if (deletedRecords.Count > 0) 
                {
                    indices.Add(deletedRecords[0]);
                    UndeleteBlock(deletedRecords[0]);
                    deletedRecords.RemoveAt(0);
                }
                else
                {
                    indices.Add(storageLength);
                    storageLength += 1;
                }
            }
        else if (indices.Count > blocksRequired) {
            var unusedBlocks = indices.GetRange((int)blocksRequired, indices.Count - 1);
            indices.RemoveRange((int)blocksRequired, indices.Count - 1);

            foreach (var index in unusedBlocks) {
                DeleteBlock(index);
            }
        }

        var blockMetadatas = new List<BlockMetadata>();

        for (var j = 0; j < indices.Count; j++) {
            var dataStart = j * Storage.ContentSize;
            var dataEnd = dataStart + Storage.ContentSize;

            var blockMetadata = new BlockMetadata() {
                Id = indices[j],
                NextBlockId = indices.Count > j + 1 ? indices[j + 1] : 0,
                PrevBlockId = j > 0 ? indices[j - 1] : 0
            };

            blockMetadatas.Add(blockMetadata);

            Storage.SetBlock(new Block() {
                Metadata = blockMetadata,
                Data = data[(dataStart)..(dataEnd >= data.Length ? data.Length : dataEnd)]
            });
        }

        return new RecordMetadata() {
            Blocks = blockMetadatas.ToArray()
        };
    }

    public void DeleteRecord(RecordMetadata metadata) {
        foreach (var block in metadata.Blocks)
            DeleteBlock(block.Id);
    }

    public void DeleteBlock(long i) {
        var deletedBlocks = GetDeletedBlocks();
        var j = 0;
        foreach (var block in deletedBlocks) {
            if (block > i) {
                break;
            }
            j++;
        }
        deletedBlocks.Insert(j, i);
        
        var buffer = new List<byte>(); 
        
        foreach (var deletedBlock in deletedBlocks){
            buffer.AddRange(
                BitConverter.GetBytes(deletedBlock)
            );
        }

        UpdateRecord(GetRecord(0), buffer.ToArray());
    }

    public void UndeleteBlock(long i) {
        var deletedBlocks = GetDeletedBlocks();
        deletedBlocks.Remove(i);
        
        var buffer = new List<byte>(); 
        
        foreach (var deletedBlock in deletedBlocks){
            buffer.AddRange(
                BitConverter.GetBytes(deletedBlock)
            );
        }

        UpdateRecord(GetRecord(0), buffer.ToArray());
    }

    public List<long> GetDeletedBlocks() {
        if (Storage.Stream is not {}) 
            throw new ArgumentNullException();

        var record0 = GetRecordData(0);
        var deletedRecords = new List<long>();

        var buffer = new byte[8];
        
        var j = 0;
        foreach (var b in record0) {
            buffer[j] = b;

            j++;

            if (j > 7) {
                deletedRecords.Add(
                    BitConverter.ToInt64(buffer)
                );
                j = 0;
            }
        }

        return deletedRecords;
    }
}

public class Record
{    
    public Block[] Data { get; set; } = new Block[0];
}

public class RecordMetadata
{    
    public BlockMetadata[] Blocks = new BlockMetadata[0];
}
