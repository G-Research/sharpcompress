using System.IO;
using SharpCompress.Common;
using SharpCompress.IO;

namespace SharpCompress.Archives;

public static class IArchiveEntryExtensions
{
    public static void WriteTo(this IArchiveEntry archiveEntry, Stream streamToWriteTo)
    {
        if (archiveEntry.IsDirectory)
        {
            throw new ExtractionException("Entry is a file directory and cannot be extracted.");
        }

        var streamListener = (IArchiveExtractionListener)archiveEntry.Archive;
        streamListener.EnsureEntriesLoaded();
        streamListener.FireEntryExtractionBegin(archiveEntry);
        streamListener.FireFilePartExtractionBegin(
            archiveEntry.Key ?? "Key",
            archiveEntry.Size,
            archiveEntry.CompressedSize
        );
        var entryStream = archiveEntry.OpenEntryStream();
        using (entryStream)
        {
            using Stream s = new ListeningStream(streamListener, entryStream);
            s.TransferTo(streamToWriteTo);
        }
        streamListener.FireEntryExtractionEnd(archiveEntry);
    }

    /// <summary>
    /// Extract to specific directory, retaining filename
    /// </summary>
    public static void WriteToDirectory(
        this IArchiveEntry entry,
        string destinationDirectory,
        ExtractionOptions? options = null
    ) =>
        ExtractionMethods.WriteEntryToDirectory(
            entry,
            destinationDirectory,
            options,
            entry.WriteToFile
        );

    /// <summary>
    /// Extract to specific file
    /// </summary>
    public static void WriteToFile(
        this IArchiveEntry entry,
        string destinationFileName,
        ExtractionOptions? options = null
    ) =>
        ExtractionMethods.WriteEntryToFile(
            entry,
            destinationFileName,
            options,
            (x, fm) =>
            {
                using var fs = File.Open(destinationFileName, fm);
                entry.WriteTo(fs);
            }
        );
}
