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
        public void �f�B���N�g���T�C�Y�𑪒�()
        {
            try
            {
                var dirname = CreateTempDir("�e�X�g");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname, "�e�X�ga", content1);
                var file2 = CreateTempFile(dirname, "�e�X�gb", content2);

                var dirTree = new DirectoryTree(dirname);
                dirTree.CalcUsage(0);

                Assert.AreEqual(content1.Length + content2.Length, dirTree.size);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void �f�B���N�g���T�C�Y�𑪒肷�鎞�ɂ̓T�u�f�B���N�g�����܂߂�()
        {
            try
            {
                var dirname1 = CreateTempDir("�e�X�g");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname1, "�e�X�ga", content1);
                var file2 = CreateTempFile(dirname2, "�e�X�gb", content2);

                var dirTree = new DirectoryTree(dirname1);
                dirTree.CalcUsage(0);

                Assert.AreEqual(content1.Length + content2.Length, dirTree.size);

                Assert.IsTrue(dirTree.fileList.Count == 0);
                Assert.IsTrue(dirTree.dirList.Count == 0);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void depth�������w�肷��ƍ����؂̍�����depth�ɂȂ�()
        {
            try
            {
                var dirname1 = CreateTempDir("�e�X�g");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u");
                var content1 = "abcde";
                var content2 = "fghi";
                var file1 = CreateTempFile(dirname1, "�e�X�ga", content1);
                var file2 = CreateTempFile(dirname2, "�e�X�gb", content2);

                var dirTree = new DirectoryTree(dirname1);
                dirTree.CalcUsage(1);

                Assert.IsTrue(dirTree.fileList.Count == 1);
                Assert.IsTrue(dirTree.dirList.Count == 1);
                Assert.IsTrue(dirTree.dirList[0].fileList.Count == 0);

                Assert.AreEqual(content1.Length + content2.Length, dirTree.size);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void �W�����N�V�����𖳎����ăf�B���N�g���T�C�Y�𑪒�()
        {
            try
            {
                var dirname1 = CreateTempDir("�e�X�g");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u");
                var content1 = "abcde";
                var file1 = CreateTempFile(dirname2, "�e�X�ga", content1);

                var dirname3 = CreateTempJunction("�e�X�g\\�W�����N�V����", dirname2);
                try
                {
                    var dirTree = new DirectoryTree(dirname1);
                    dirTree.CalcUsage(1);

                    Assert.AreEqual(1, dirTree.dirList.Count);
                    Assert.AreEqual(content1.Length, dirTree.size);
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
        public void �W�����N�V�������܂߂ăf�B���N�g���T�C�Y�𑪒�()
        {
            try
            {
                var dirname1 = CreateTempDir("�e�X�g");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u");
                var content1 = "abcde";
                var file1 = CreateTempFile(dirname2, "�e�X�ga", content1);

                var dirname3 = CreateTempJunction("�e�X�g\\�W�����N�V����", dirname2);
                try
                {
                    var dirTree = new DirectoryTree(dirname1);
                    dirTree.CalcUsageWithJunction(1);

                    Assert.AreEqual(2, dirTree.dirList.Count);
                    Assert.AreEqual(content1.Length * 2, dirTree.size);
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
        public void ���בւ���̓f�B���N�g���T�C�Y���傫��������ɂȂ�1()
        {
            try
            {
                var dirname = CreateTempDir("�e�X�g");
                var dirname1 = CreateTempDir("�e�X�g\\�T�u1");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u2");

                var content1 = "short";
                var filename1 = CreateTempFile(dirname1, "a", content1);

                var content2 = "long-string";
                var filename2 = CreateTempFile(dirname2, "b", content2);

                var dirTree = new DirectoryTree(dirname);
                dirTree.CalcUsage(1);
                dirTree.dirList.Sort(DirectoryTree.CompareBySize);

                Assert.AreEqual(dirname2, dirTree.dirList[0].dirname);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ���בւ���̓f�B���N�g���T�C�Y���傫��������ɂȂ�2()
        {
            try
            {
                var dirname = CreateTempDir("�e�X�g");
                var dirname1 = CreateTempDir("�e�X�g\\�T�u1");
                var dirname2 = CreateTempDir("�e�X�g\\�T�u2");

                var content1 = "long-string";
                var filename1 = CreateTempFile(dirname1, "b", content1);
                var content2 = "short";
                var filename2 = CreateTempFile(dirname2, "a", content2);

                var dirTree = new DirectoryTree(dirname);
                dirTree.CalcUsage(1);
                dirTree.dirList.Sort(DirectoryTree.CompareBySize);

                Assert.AreEqual(dirname1, dirTree.dirList[0].dirname);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void �f�B���N�g���T�C�Y�������ꍇ���בւ���̓f�B���N�g�������ɂȂ�()
        {
            try
            {
                var dirname = CreateTempDir("�e�X�g");
                var dirname1 = CreateTempDir("�e�X�g\\�T�ub");
                var dirname2 = CreateTempDir("�e�X�g\\�T�ua");

                var content1 = "same";
                var filename1 = CreateTempFile(dirname1, "a", content1);
                var content2 = "same";
                var filename2 = CreateTempFile(dirname2, "b", content2);

                var dirTree = new DirectoryTree(dirname);
                dirTree.CalcUsage(1);
                dirTree.dirList.Sort(DirectoryTree.CompareBySize);

                Assert.AreEqual(dirname2, dirTree.dirList[0].dirname);
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
        public void �t�@�C���T�C�Y�𑪒�()
        {
            try
            {
                var content = "abcde";
                var filename = CreateTempFile("�e�X�g", content);

                var fileSize = new FileSize(filename);
                Assert.AreEqual(content.Length, fileSize.size);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ���בւ���̓t�@�C���T�C�Y���傫��������ɂȂ�1()
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

                Assert.AreEqual(file2, fileList[0]);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void ���בւ���̓t�@�C���T�C�Y���傫��������ɂȂ�2()
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

                Assert.AreEqual(file1, fileList[0]);
            }
            finally
            {
                Directory.Delete(tmpdir, true);
            }
        }
        [TestMethod]
        public void �t�@�C���T�C�Y�������ꍇ���בւ���̓t�@�C�������ɂȂ�()
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

                Assert.AreEqual(file2, fileList[0]);
            }finally{
                Directory.Delete(tmpdir, true);
            }
        }
    }
    [TestClass]
    public class CommandArgsTest
    {
        [TestMethod]
        public void �ʏ�̈�����directories���X�g�Ɋi�[����()
        {
            var commandArgs = new CommandArgs(new string[]{ "�e�X�g" });
            Assert.AreEqual("�e�X�g", commandArgs.directories[0]);
        }
        [TestMethod]
        public void l�t���O���w�肷��Ɛ[����ݒ肷��()
        {
            var commandArgs = new CommandArgs(new string[] { "-l", "3" });
            Assert.AreEqual(3, commandArgs.depth);
        }
        [TestMethod]
        public void l�t���O�Ő��l�ȊO���w�肷��ƃG���[()
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
        public void l�t���O�ŕ��̐����w�肷��ƃG���[()
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
        public void c�t���O���w�肷��Ƌ�؂蕶�����R���}�ɂ���()
        {
            var commandArgs = new CommandArgs(new string[] { "-c" });
            Assert.AreEqual(",", commandArgs.separator);
        }
        [TestMethod]
        public void t�t���O���w�肷��Ƌ�؂蕶�����^�u�ɂ���()
        {
            var commandArgs = new CommandArgs(new string[] { "-t" });
            Assert.AreEqual("\t", commandArgs.separator);
        }
        [TestMethod]
        public void j�t���O���w�肷���withJunction�t���O��ON�ɂ���()
        {
            var commandArgs = new CommandArgs(new string[] { "-j" });
            Assert.IsTrue(commandArgs.withJunction);
        }
        [TestMethod]
        public void h�t���O���w�肷���showHelpFlag��ON�ɂ���()
        {
            var commandArgs = new CommandArgs(new string[] { "-h" });
            Assert.IsTrue(commandArgs.showHelpFlag);
        }
    }
}