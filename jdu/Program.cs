using JDU;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var commandArgs = new CommandArgs(args);
            if (commandArgs.showHelpFlag)
            {
                commandArgs.showHelp();
                return;
            }
            if (commandArgs.directories.Count == 0)
            {
                throw new ArgumentException("ディレクトリを指定してください");
            }

            foreach (string dir in commandArgs.directories)
            {
                var dirTree = new DirectoryTree(dir);
                if (commandArgs.withJunction)
                {
                    dirTree.CalcUsageWithJunction(commandArgs.depth);
                }
                else
                {
                    dirTree.CalcUsage(commandArgs.depth);
                }
                dirTree.WriteLine(Console.Out, commandArgs.separator);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }

        return;
    }
}

namespace JDU
{
    public class DirectoryTree
    {
        public string dirname = "";
        public long size = 0;
        public List<FileSize> fileList = new();
        public List<DirectoryTree> dirList = new();

        public DirectoryTree(string dirname)
        {
            this.dirname = dirname;
            size = 0;
        }

        public long CalcUsage(int depth)
        {
            foreach (string filename in Directory.GetFiles(dirname))
            {
                var fileSize = new FileSize(filename);
                size += fileSize.size;
                if (depth > 0)
                {
                    fileList.Add(fileSize);
                }
            }

            foreach (string dirname in Directory.GetDirectories(dirname))
            {
                var dirInfo = new DirectoryInfo(dirname);
                if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == 0)
                {
                    var subdir = new DirectoryTree(dirname);
                    subdir.CalcUsage(depth - 1);
                    size += subdir.size;
                    if (depth > 0)
                    {
                        dirList.Add(subdir);
                    }
                }
            }
            return size;
        }

        public void CalcUsageWithJunction(int depth)
        {
            foreach (var file in Directory.GetFiles(dirname))
            {
                var fileSize = new FileSize(file);
                size += fileSize.size;
                if (depth > 0)
                {
                    fileList.Add(fileSize);
                }
            }

            foreach (var subdirname in Directory.GetDirectories(dirname))
            {
                var subdir = new DirectoryTree(subdirname);
                subdir.CalcUsageWithJunction(depth - 1);
                size += subdir.size;
                if (depth > 0)
                {
                    dirList.Add(subdir);
                }
            }
        }

        public void WriteLine(TextWriter output, string separator)
        {
            output.WriteLine("{0}{1}{2}", size, separator, dirname);

            fileList.Sort(FileSize.CompareBySize);
            foreach(FileSize file in fileList)
            {
                file.WriteLine(output, separator);
            }

            dirList.Sort(DirectoryTree.CompareBySize);
            foreach (DirectoryTree dir in dirList)
            {
                dir.WriteLine(output, separator);
            }
        }
        public static int CompareBySize(DirectoryTree a, DirectoryTree b)
        {
            return a.size > b.size ? -1 :
                a.size < b.size ? 1 :
                string.Compare(a.dirname, b.dirname);
        }
    }

    public class FileSize
    {
        public string filename = "";
        public long size = 0;

        public FileSize(string filename)
        {
            this.filename = filename;
            var info = new FileInfo(filename);
            this.size = info.Length;
        }

        public static int CompareBySize(FileSize a, FileSize b)
        {
            return a.size > b.size ? -1 :
                a.size < b.size ? 1 :
                string.Compare(a.filename, b.filename);
        }

        public void WriteLine(TextWriter output, string separator)
        {
            output.WriteLine("{0}{1}{2}", size, separator, filename);
        }
    }

    public class CommandArgs
    {
        public List<string> directories = new List<string>();
        public int depth = 0;
        public string separator = " ";
        public bool showHelpFlag = false;
        public bool withJunction = false;

        public CommandArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg[0] == '-')
                {
                    switch (arg[1])
                    {
                        case 'l':
                            i++;
                            SetDepth(args[i]);
                            break;
                        case 'c':
                            separator = ",";
                            break;
                        case 't':
                            separator = "\t";
                            break;
                        case 'j':
                            withJunction = true;
                            break;
                        case 'h':
                            showHelpFlag = true;
                            break;
                        default:
                            throw new ArgumentException("未知のオプションです");
                    }
                }
                else
                {
                    directories.Add(arg);
                }
            }
        }

        public void showHelp()
        {
            const string helpMessage = "jdu [OPTION] directories\n" +
                " -l depth 結果出力するディレクトリの深さ\n" +
                " -c 結果をコンマ区切りで出力\n" +
                " -t 結果をタブ区切りで出力\n" +
                " -j ジャンクションを含める\n" +
                " -h ヘルプを表示\n";

            Console.Write(helpMessage);
        }

        private void SetDepth(string arg)
        {
            if(!int.TryParse(arg, out depth)){
                throw new ArgumentException("深さには整数を指定してください");
            }
            if (depth < 0)
            {
                throw new ArgumentException("深さには0以上の数を指定してください");
            }
        }
    }
}