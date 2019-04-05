﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Terraria.ModLoader.Setup
{
	public class DiffTask : Task
	{
		public static string[] extensions = { ".cs", ".csproj", ".ico", ".resx", ".png", "App.config", ".json" };
		public static string[] excluded = { "bin" + Path.DirectorySeparatorChar, "obj" + Path.DirectorySeparatorChar };
		public static readonly string RemovedFileList = "removed_files.list";
		public static readonly Regex HunkOffsetRegex = new Regex(@"@@ -(\d+),(\d+) \+([_\d]+),(\d+) @@", RegexOptions.Compiled);


		public readonly string baseDir;
		public readonly string patchedDir;
		public readonly string patchDir;
		public readonly ProgramSetting<DateTime> cutoff;

		public DiffTask(ITaskInterface taskInterface, string baseDir, string srcDir, string patchDir, 
			ProgramSetting<DateTime> cutoff) : base(taskInterface)
		{
			this.baseDir = baseDir;
			this.patchedDir = srcDir;
			this.patchDir = patchDir;
			this.cutoff = cutoff;
		}

		public override void Run()
		{
			var patchFiles = new HashSet<string>(
				Directory.EnumerateFiles(patchDir, "*", SearchOption.AllDirectories)
				.Select(file => RelPath(patchDir, file)));
			var oldFiles = new HashSet<string>(
				Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories)
				.Select(file => RelPath(baseDir, file))
				.Where(relPath => !relPath.EndsWith(".patch") && !excluded.Any(relPath.StartsWith)));

			var items = new List<WorkItem>();

			foreach (var file in Directory.EnumerateFiles(patchedDir, "*", SearchOption.AllDirectories))
			{
				var relPath = RelPath(patchedDir, file);
				oldFiles.Remove(relPath);
				if (!extensions.Any(relPath.EndsWith))
					continue;

				patchFiles.Remove(relPath);
				patchFiles.Remove(relPath + ".patch");

				if (excluded.Any(relPath.StartsWith) || File.GetLastWriteTime(file) < cutoff.Get())
					continue;

				items.Add(File.Exists(Path.Combine(baseDir, relPath))
					? new WorkItem("Creating Diff: " + relPath, () => Diff(relPath))
					: new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(patchDir, relPath))));
			}

			ExecuteParallel(items);

			taskInterface.SetStatus("Deleting Unnessesary Patches");
			foreach (var file in patchFiles)
				File.Delete(Path.Combine(patchDir, file));

			taskInterface.SetStatus("Noting Removed Files");
			var removedFileList = Path.Combine(patchDir, RemovedFileList);
			if (oldFiles.Count > 0)
				File.WriteAllText(removedFileList, string.Join("\r\n", oldFiles));
			else if (File.Exists(removedFileList))
				File.Delete(removedFileList);

			cutoff.Set(DateTime.Now);
		}

		private void Diff(string relPath)
		{
			var srcFile = Path.Combine(patchedDir, relPath);
			var baseFile = Path.Combine(baseDir, relPath);

			/*string temp = null;
			if (srcFile.EndsWith(".cs") && format != null)
			{
				temp = Path.GetTempPath() + Guid.NewGuid() + ".cs";
				File.WriteAllText(temp, FormatTask.FormatCode(File.ReadAllText(baseFile), format, taskInterface.CancellationToken()));
				baseFile = temp;
			}*/

			var patch = CallDiff(baseFile, srcFile, Path.Combine(baseDir, relPath), Path.Combine(patchedDir, relPath));

			/*if (temp != null)
				File.Delete(temp);*/

			var patchFile = Path.Combine(patchDir, relPath + ".patch");
			if (patch.Trim() != "") {
				CreateParentDirectory(patchFile);
				File.WriteAllText(patchFile, StripDestHunkOffsets(patch));
			}
			else if (File.Exists(patchFile))
				File.Delete(patchFile);

		}

		private static string StripDestHunkOffsets(string patchText) {
			var lines = patchText.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
			for (int i = 0; i < lines.Length; i++)
				if (lines[i].StartsWith("@@"))
					lines[i] = HunkOffsetRegex.Replace(lines[i], "@@ -$1,$2 +_,$4 @@");

			return string.Join(Environment.NewLine, lines);
		}

		private string CallDiff(string baseFile, string srcFile, string baseName, string srcName)
		{
			var output = new StringBuilder();
			Program.RunCmd(Program.toolsDir, Path.Combine(Program.toolsDir, "py.exe"),
				$"diff.py {baseFile} {srcFile} {baseName} {srcName}",
				s => output.Append(s));

			return output.ToString();
		}

	}
}