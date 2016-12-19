using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqPad.ViewModels
{
    public sealed class DocumentViewModel
    {
        private static readonly string fileExtension = ".csx";
        private ObservableCollection<DocumentViewModel> children;
        public ObservableCollection<DocumentViewModel> Childrens
        {
            get
            {
                if (children == null && IsFolder)
                    children = GetChildrens();
                return children;
            }
        }

        public string Name
        {
            get
            {
                if (IsFolder)
                    return System.IO.Path.GetFileName(Path);
                else
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }
        public   string Path { get; set; }
        public bool IsFolder { get; private set; }

        public DocumentViewModel(string path, bool isFolder)
        {
            this.Path = path;
            this.IsFolder = isFolder;
        }


        private ObservableCollection<DocumentViewModel> GetChildrens()
        {
            var childsDirs = Directory.EnumerateDirectories(Path);
            var childfiles = Directory.EnumerateFiles(Path);

            var allFiles = childsDirs.Select(n => new DocumentViewModel(n, true)).
                Concat(childfiles.Where(n=> n.EndsWith(fileExtension)).
                Select(n=>new DocumentViewModel(n, false)));

            return new ObservableCollection<DocumentViewModel>(allFiles);
        }
    }
}
