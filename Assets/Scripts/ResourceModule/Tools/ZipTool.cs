
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using UnityEngine;

namespace ResourceModule
{
	public static class ZipTool
	{
		static ZipTool()
		{
			ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;
		}

		public static byte[] Compress(byte[] input, int level = Deflater.BEST_COMPRESSION)
		{
			if (input == null || input.Length == 0)
			{
				Debug.LogError("Compress error inputBytes Len = 0");
				return input;
			}

			// Create the compressor with highest level of compression  
			Deflater compressor = new Deflater(level);

			// Give the compressor the data to compress  
			compressor.SetInput(input);
			compressor.Finish();

			/* 
             * Create an expandable byte array to hold the compressed data. 
             * You cannot use an array that's the same size as the orginal because 
             * there is no guarantee that the compressed data will be smaller than 
             * the uncompressed data.
             */
			MemoryStream result = new MemoryStream(input.Length);

			// Compress the data  
			byte[] buffer = new byte[1024];
			while (!compressor.IsFinished)
			{
				int count = compressor.Deflate(buffer);
				result.Write(buffer, 0, count);
			}

			// Get the compressed data  
			return result.ToArray();
		}

		public static byte[] Uncompress(byte[] input)
		{
			if (input == null || input.Length == 0)
			{
				Debug.LogError("Uncompress error inputBytes Len = 0");
				return input;
			}

			Inflater decompressor = new Inflater();
			decompressor.SetInput(input);

			// Create an expandable byte array to hold the decompressed data  
			MemoryStream result = new MemoryStream(input.Length);

			// Decompress the data  
			byte[] buffer = new byte[4096];
			while (!decompressor.IsFinished)
			{
				int count = decompressor.Inflate(buffer);
				result.Write(buffer, 0, count);
			}

			return result.ToArray();
		}

		public static void UncompressFile(string zipFile, string targetDir, string password = null)
		{
			ZipFile zf = null;
			try
			{
				FileStream fs = File.OpenRead(zipFile);
				zf = new ZipFile(fs);
				if (!string.IsNullOrEmpty(password))
				{
					zf.Password = password;     // AES encrypted entries are handled automatically
				}
				foreach (ZipEntry zipEntry in zf)
				{
					if (!zipEntry.IsFile)
					{
						continue;           // Ignore directories
					}
					string entryFileName = zipEntry.Name;
					// to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
					// Optionally match entrynames against a selection list here to skip as desired.
					// The unpacked length is available in the zipEntry.Size property.

					byte[] buffer = new byte[4096];     // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);

					// Manipulate the output filename here as desired.
					string fullZipToPath = Path.Combine(targetDir, entryFileName);
					string outputDir = Path.GetDirectoryName(fullZipToPath);
					if (!string.IsNullOrEmpty(outputDir))
						Directory.CreateDirectory(outputDir);

					// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
					// of the file, but does not waste memory.
					// The "using" will close the stream even if an exception occurs.
					using (FileStream streamWriter = File.Create(fullZipToPath))
					{
						StreamUtils.Copy(zipStream, streamWriter, buffer);
						//Debug.LogErrorFormat("Unzip file: " + fullZipToPath);
					}
				}
			}
			finally
			{
				if (zf != null)
				{
					zf.IsStreamOwner = true; // Makes close also shut the underlying stream
					zf.Close(); // Ensure we release resources
				}
			}
		}

		public static void CompressFile(string zipPath, string filePath, string password = "")
		{
			string outputDir = Path.GetDirectoryName(zipPath);
			if (!string.IsNullOrEmpty(outputDir)) Directory.CreateDirectory(outputDir);

			using (var zipFile = ZipFile.Create(zipPath))
			{
				zipFile.Password = password;
				zipFile.BeginUpdate();
				zipFile.Add(filePath);
				zipFile.CommitUpdate();
			}
		}

		public static void CompressFiles(string zipPath, string[] files, string password = "")
		{
			string outputDir = Path.GetDirectoryName(zipPath);
			if (!string.IsNullOrEmpty(outputDir)) Directory.CreateDirectory(outputDir);

			using (var zipFile = ZipFile.Create(zipPath))
			{
				zipFile.Password = password;
				zipFile.BeginUpdate();
				foreach (string file in files)
				{
					zipFile.Add(file);
				}
				zipFile.CommitUpdate();
			}
		}

		public static void CompressFolder(string zipPath, string dirPath, string searchPattern = "*.*", string password = "")
		{
			string outputDir = Path.GetDirectoryName(zipPath);
			if (!string.IsNullOrEmpty(outputDir)) Directory.CreateDirectory(outputDir);

			using (var zipFile = ZipFile.Create(zipPath))
			{
				zipFile.Password = password;
				zipFile.BeginUpdate();
				foreach (string file in Directory.GetFiles(dirPath, searchPattern, SearchOption.AllDirectories))
				{
					zipFile.Add(file, file.Replace(dirPath, ""));
				}
				zipFile.CommitUpdate();
			}
		}
	}
}
