using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoSqlRepositories.Tests.Shared.Extensions
{
    public static class TestContextExtensions
    {
        private static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }
        }

        public static void CopyFolder(string source, string target)
        {
            CopyFolder(new DirectoryInfo(source), new DirectoryInfo(target));
        }

        /// <summary>
        /// Deploy a file from the build folder into the test project output folder
        /// </summary>
        /// <param name="testContext"></param>
        /// <param name="inPath">Path to the file to copy relative to root of the project build folder</param>
        /// <param name="outputDirectory">Destination path, relative to the root of the test project output</param>
        public static void DeployFile(this TestContext testContext, string inPath, string outputDirectory)
        {
            var filename = Path.GetFileName(inPath);
            var inputFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, inPath);
            var outputFolderPath = Path.Combine(Directory.GetCurrentDirectory(), outputDirectory);
            var outputFilePath = Path.Combine(outputFolderPath, filename);

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException();

            Directory.CreateDirectory(outputFolderPath);
            File.Copy(inputFilePath, outputFilePath, true);
        }

        /// <summary>
        /// Deploy a directory from the build folder into the test project output folder
        /// </summary>
        /// <param name="testContext"></param>
        /// <param name="inPath">Path to the directory to copy relative to root of the project build folder</param>
        /// <param name="outputDirectory">Destination path, relative to the root of the test project output</param>
        public static void DeployDirectory(this TestContext testContext, string inPath, string outputDirectory)
        {
            var inputDirPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, inPath);
            var outputDirPath = Path.Combine(Directory.GetCurrentDirectory(), outputDirectory);

            if (!Directory.Exists(inputDirPath))
                throw new FileNotFoundException();

            if (!Directory.Exists(outputDirPath))
                Directory.CreateDirectory(outputDirPath);

            CopyFolder(inputDirPath, outputDirPath);
        }


    }
}
