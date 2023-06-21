using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace SuCoS;

/// <summary>
/// Build Command will build the site based on the source files.
/// </summary>
public class BuildCommand : BaseGeneratorCommand
{
    private readonly BuildOptions options;

    /// <summary>
    /// Entry point of the build command. It will be called by the main program
    /// in case the build command is invoked (which is by default).
    /// </summary>
    /// <param name="options"></param>
    public BuildCommand(BuildOptions options) : base(options: options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        this.options = options;

        Log.Information("Output path: {output}", options.Output);

        // Generate the site pages
        CreateOutputFiles();

        // Copy static folder files into the root of the output folder
        CopyFolder(site.SourceStaticPath, site.OutputPath);

        // Generate the build report
        stopwatch.LogReport(site.Title);
    }

    private void CreateOutputFiles()
    {
        stopwatch.Start("Create");

        // Print each page
        var pagesCreated = 0; // counter to keep track of the number of pages created
        _ = Parallel.ForEach(site.Pages, pair =>
        {
            var (url, frontmatter) = pair;
            var result = CreateOutputFile(frontmatter);

            var path = (url + (site.UglyURLs ? "" : "/index.html")).TrimStart('/');

            // Generate the output path
            var outputAbsolutePath = Path.Combine(site.OutputPath, path);

            var outputDirectory = Path.GetDirectoryName(outputAbsolutePath);
            if (!Directory.Exists(outputDirectory))
            {
                _ = Directory.CreateDirectory(outputDirectory!);
            }

            // Save the processed output to the final file
            File.WriteAllText(outputAbsolutePath, result);

            // Log
            if (options.Verbose)
            {
                Log.Information("Page created: {Permalink}", frontmatter.Permalink);
            }

            // Use interlocked to safely increment the counter in a multi-threaded environment
            _ = Interlocked.Increment(ref pagesCreated);
        });

        // Stop the stopwatch
        stopwatch.Stop("Create", pagesCreated);
    }

    /// <summary>
    /// Copy a folder content from source into the output folder.
    /// </summary> 
    /// <param name="source">The source folder to copy from.</param>
    /// <param name="output">The output folder to copy to.</param>
    private static void CopyFolder(string source, string output)
    {
        // Check if the source folder even exists
        if (!Directory.Exists(source))
        {
            return;
        }

        // Create the output folder if it doesn't exist
        _ = Directory.CreateDirectory(output);

        // Get all files in the source folder
        var files = Directory.GetFiles(source);

        foreach (var file in files)
        {
            // Get the filename from the full path
            var fileName = Path.GetFileName(file);

            // Create the destination path by combining the output folder and the filename
            var destinationPath = Path.Combine(output, fileName);

            // Copy the file to the output folder
            File.Copy(file, destinationPath, overwrite: true);
        }
    }
}
