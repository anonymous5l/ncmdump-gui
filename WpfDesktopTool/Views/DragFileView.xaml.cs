using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WpfDesktopTool.Annotations;

namespace WpfDesktopTool.Views
{
    /// <summary>
    /// DragFileView.xaml 的交互逻辑
    /// </summary>
    public partial class DragFileView : UserControl,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BindingList<NcmFileInfo> _files = new BindingList<NcmFileInfo>();
        private bool _isAutoDelete;
        private bool _isMove;
        private bool _isDefault;
        private string _dirPath ;
        private string _movePath;
        public BindingList<NcmFileInfo> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public bool IsAutoDelete
        {
            get { return _isAutoDelete; }
            set { _isAutoDelete = value; OnPropertyChanged();}
        }

        public string DirPath
        {
            get { return _dirPath; }
            set { _dirPath = value; OnPropertyChanged(); }
        }

        public bool IsMove
        {
            get { return _isMove; }
            set { _isMove = value; OnPropertyChanged(); }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; OnPropertyChanged(); }
        }

        public string MovePath
        {
            get { return _movePath; }
            set { _movePath = value;OnPropertyChanged(); }
        }

        public DragFileView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames==null || filenames.Length<=0)
            {
                return;
            }
            AddFileList(filenames);
        }

        private async void btn_Convert_Click(object sender, RoutedEventArgs e)
        {
            if (Files==null||Files.Count<=0)
            {
                return;
            }
            btn_Convert.IsEnabled = false;
            if (IsMove)
            {
                MovePath = Path.GetFullPath(@".\MovedPath\");
                ShowSelectFolderBroserDialog((selectedPath) =>
                {
                    MovePath = selectedPath;
                }, "请选择移动Ncm源文件的目标文件夹");
            }
            await Task.Run(() => { DumpFile(); });
            btn_Convert.IsEnabled = true;
        }

        private async void btn_selectDir_Click(object sender, RoutedEventArgs e)
        {
            ShowSelectFolderBroserDialog( async (selectedPath) =>
            {
                DirPath = selectedPath;
                List<string> fileList = new List<string>();
                await Task.Run(() =>
                {
                    GetFile(DirPath, fileList);
                });
                AddFileList(fileList.ToArray());
            }, "请选择保存Music所在文件夹");
        }

        public static void ShowSelectFolderBroserDialog(Action<string> callback,string description=null)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = description;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return;
                }
                callback?.Invoke(dialog.SelectedPath);
            }
        }

        private void AddFileList(string[] filenames)
        {
            Files.Clear();
            foreach (var filename in filenames)
            {
                if (Path.GetExtension(filename).ToLower() != ".ncm")
                {
                    continue;
                }
                Files.Add(new NcmFileInfo()
                {
                    FilePath = filename,
                    Index = Files.Count
                });
            }
        }

        private void DumpFile()
        {
            List<Task> tlist = new List<Task>();
            foreach (var fileInfo in Files)
            {
                var t = new Task(() =>
                {
                    fileInfo.Status = NcmDecrypt.NeteaseCrypto.DumpFile(fileInfo.FilePath, IsAutoDelete, (s) =>
                    {
                        fileInfo.Message += s;
                        fileInfo.Message += "\n";
                        if (s == "转换完成")
                        {
                            if (IsMove)
                            {
                                File.Move(fileInfo.FilePath, Path.Combine(MovePath, Path.GetFileName(fileInfo.FilePath)));
                            }
                        }
                    });
                });
                tlist.Add(t);
                t.Start();
            }
            Task.WaitAll(tlist.ToArray());
        }

        /// <summary>
        /// 获取路径下所有文件以及子文件夹中文件
        /// </summary>
        /// <param name="path">全路径根目录</param>
        /// <param name="FileList">存放所有文件的全路径</param>
        /// <returns></returns>
        public static List<string> GetFile(string path, List<string> FileList)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
            {
                //int size = Convert.ToInt32(f.Length);
                //long size = f.Length;
                FileList.Add(f.FullName);//添加文件路径到列表中
            }
            //获取子文件夹内的文件列表，递归遍历
            foreach (DirectoryInfo d in dii)
            {
                GetFile(d.FullName, FileList);
            }
            return FileList;
        }

    }

    public class NcmFileInfo: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _filePath;
        private bool _status;
        string _fileName;
        private int _index;
        string _message;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; FileName = Path.GetFileName(_filePath); OnPropertyChanged(); }
        }

        public bool Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged();}
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; OnPropertyChanged(); }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged(); }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
