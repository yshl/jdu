using JDU;
using System.Diagnostics;

namespace test
{
    [TestClass]
    public class DirectoryTreeTest
    {
        string tmpdir = Path.Join(Path.GetTempPath(), "JDUTest");
        private string CreateTempDir(string dirname)
        {
            var path = Path.Join(tmpdir, dirname);
            Directory.CreateDirectory(path);
            return path;
        }
        private string CreateTempJunction(string dirname, string target)
        {
            var path = Path.Join(tmpdir, dirname);
            string[] args = { "/c", "mklink", "/J", path, target };
            Process.Start("cmd.exe", args).WaitForExit();
            return path;
        }
        private string CreateTempFile(string dirname, string filename, string content)
        {
            var filepath = Path.Join(dirname, filename);
            File.WriteAllText(filepath, content);
            return filepath;
        }
        private void DeleteTempJunction(string dirname)
        {
            string[] args = { "/c", "rmdir", dirname };
            Process.Start("cmd.exe", args).WaitForExit();
        }
        [TestMethod]
        public void ディレクトリサイズを測定()
        {
            try
            {
                var dirname = CreateTempDir("テスト");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname, "テストa", content1);
                var file2 = CreateTempFile(dirname, "テストb", content2);

                var dirTree = new DirectoryTree(dirname);
                dirTree.CalcUsage(0);

                Assert.AreEqual(dirTree.size, content1.Length + content2.Length);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ディレクトリサイズを測定する時にはサブディレクトリを含める()
        {
            try
            {
                var dirname1 = CreateTempDir("テスト");
                var dirname2 = CreateTempDir("テスト\\サブ");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname1, "テストa", content1);
                var file2 = CreateTempFile(dirname2, "テストb", content2);

                var dirTree = new DirectoryTree(dirname1);
                dirTree.CalcUsage(0);

                Assert.AreEqual(dirTree.size, content1.Length + content2.Length);

                Assert.IsTrue(dirTree.fileList.Count == 0);
                Assert.IsTrue(dirTree.dirList.Count == 0);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void depth引数を指定すると作られる木の高さがdepthになる()
        {
            try
            {
                var dirname1 = CreateTempDir("テスト");
                var dirname2 = CreateTempDir("テスト\\サブ");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname1, "テストa", content1);
                var file2 = CreateTempFile(dirname2, "テストb", content2);

                var dirTree = new DirectoryTree(dirname1);
                dirTree.CalcUsage(1);

                Assert.IsTrue(dirTree.fileList.Count == 1);
                Assert.IsTrue(dirTree.dirList.Count == 1);
                Assert.IsTrue(dirTree.dirList[0].fileList.Count == 0);

                Assert.AreEqual(dirTree.size, content1.Length + content2.Length);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ジャンクションを無視ししてディレクトリサイズを測定()
        {
            try
            {
                var dirname1 = CreateTempDir("テスト");
                var dirname2 = CreateTempDir("テスト\\サブ");
                var content1 = "abcde";
                var file1 = CreateTempFile(dirname2, "テストa", content1);

                var dirname3 = CreateTempJunction("テスト\\ジャンクション", dirname2);
                try
                {
                    var dirTree = new DirectoryTree(dirname1);
                    dirTree.CalcUsage(1);

                    Assert.AreEqual(dirTree.dirList.Count, 1);
                    Assert.AreEqual(dirTree.size, content1.Length);
                }
                finally
                {
                    DeleteTempJunction(dirname3);
                }
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ジャンクションを含めてディレクトリサイズを測定()
        {
            try
            {
                var dirname1 = CreateTempDir("テスト");
                var dirname2 = CreateTempDir("テスト\\サブ");
                var content1 = "abcde";
                var file1 = CreateTempFile(dirname2, "テストa", content1);

                var dirname3 = CreateTempJunction("テスト\\ジャンクション", dirname2);
                try
                {
                    var dirTree = new DirectoryTree(dirname1);
                    dirTree.CalcUsageWithJunction(1);

                    Assert.AreEqual(dirTree.dirList.Count, 2);
                    Assert.AreEqual(dirTree.size, content1.Length * 2);
                }
                finally
                {
                    DeleteTempJunction(dirname3);
                }
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
    }
    [TestClass]
    public class FileSizeTest
    {
        string tmpdir = Path.Join(Path.GetTempPath(), "JDUTest");
        private string CreateTempFile(string filename, string content)
        {
            Directory.CreateDirectory(tmpdir);
            var filepath = Path.Join(tmpdir, filename);
            File.WriteAllText(filepath, content);
            return filepath;
        }
        [TestMethod]
        public void ファイルサイズを測定()
        {
            try
            {
                var content = "abcde";
                var filename = CreateTempFile("テスト", content);

                var fileSize = new FileSize(filename);
                Assert.AreEqual(fileSize.size, content.Length);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void 並べ替え後はファイルサイズが大きい方が先になる1()
        {
            try
            {
                var content1 = "short";
                var filename1 = CreateTempFile("a", content1);

                var content2 = "long-string";
                var filename2 = CreateTempFile("b", content2);

                var file1 = new FileSize(filename1);
                var file2 = new FileSize(filename2);
                FileSize[] fileSizes = { file1, file2 };
                var fileList = new List<FileSize>(fileSizes);

                fileList.Sort(FileSize.CompareBySize);

                Assert.AreEqual(fileList[0], file2);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void 並べ替え後はファイルサイズが大きい方が先になる2()
        {
            try
            {
                var content1 = "long-string";
                var filename1 = CreateTempFile("b", content1);

                var content2 = "short";
                var filename2 = CreateTempFile("a", content2);

                var file1 = new FileSize(filename1);
                var file2 = new FileSize(filename2);
                FileSize[] fileSizes = { file1, file2 };
                var fileList = new List<FileSize>(fileSizes);

                fileList.Sort(FileSize.CompareBySize);

                Assert.AreEqual(fileList[0], file1);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void 並べ替え後はファイルサイズが同じ場合はファイル名順になる()
        {
            try
            {
                var content1 = "same";
                var filename1 = CreateTempFile("b", content1);

                var content2 = "same";
                var filename2 = CreateTempFile("a", content2);

                var file1 = new FileSize(filename1);
                var file2 = new FileSize(filename2);
                FileSize[] fileSizes = { file1, file2 };
                var fileList = new List<FileSize>(fileSizes);

                fileList.Sort(FileSize.CompareBySize);

                Assert.AreEqual(fileList[0], file2);
            }finally{
                Directory.Delete(tmpdir, true);
            }
        }
    }
    [TestClass]
    public class CommandArgsTest
    {
        [TestMethod]
        public void 通常の引数はdirectoriesリストに格納する()
        {
            var commandArgs = new CommandArgs(new string[]{ "テスト" });
            Assert.AreEqual(commandArgs.directories[0], "テスト");
        }
        [TestMethod]
        public void lフラグを指定すると深さを設定する()
        {
            var commandArgs = new CommandArgs(new string[] { "-l", "3" });
            Assert.AreEqual(commandArgs.depth, 3);
        }
        [TestMethod]
        public void lフラグで数値以外を指定するとエラー()
        {
            bool exception_raised = false;
            try
            {
                var commandArgs = new CommandArgs(new string[] { "-l", "a" });
            }
            catch
            {
                exception_raised = true;
            }
            Assert.IsTrue(exception_raised);
        }
        [TestMethod]
        public void lフラグで負の数を指定するとエラー()
        {
            bool exception_raised = false;
            try
            {
                var commandArgs = new CommandArgs(new string[] { "-l", "-1" });
            }
            catch
            {
                exception_raised = true;
            }
            Assert.IsTrue(exception_raised);
        }
        [TestMethod]
        public void cフラグを指定すると区切り文字をコンマにする()
        {
            var commandArgs = new CommandArgs(new string[] { "-c" });
            Assert.AreEqual(commandArgs.separator, ",");
        }
        [TestMethod]
        public void tフラグを指定すると区切り文字をタブにする()
        {
            var commandArgs = new CommandArgs(new string[] { "-t" });
            Assert.AreEqual(commandArgs.separator, "\t");
        }
        [TestMethod]
        public void jフラグを指定するとwithJunctionフラグをONにする()
        {
            var commandArgs = new CommandArgs(new string[] { "-j" });
            Assert.IsTrue(commandArgs.withJunction);
        }
        [TestMethod]
        public void hフラグを指定するとshowHelpFlagをONにする()
        {
            var commandArgs = new CommandArgs(new string[] { "-h" });
            Assert.IsTrue(commandArgs.showHelpFlag);
        }
    }
}