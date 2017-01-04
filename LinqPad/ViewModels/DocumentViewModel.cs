using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

namespace LinqPad.ViewModels
{
    public sealed class DocumentViewModel
    {
        private const string FileExtension = ".csx";

        private ObservableCollection<DocumentViewModel> children;

        public DocumentViewModel(string path, bool isFolder)
        {
            this.Path = path;
            this.IsFolder = isFolder;
        }

        public ObservableCollection<DocumentViewModel> Childrens
        {
            get
            {
                if (this.children == null && this.IsFolder)
                {
                    this.children = this.GetChildrens();
                }

                return this.children;
            }
        }

        public string Name => this.IsFolder ? System.IO.Path.GetFileName(this.Path) :
            System.IO.Path.GetFileNameWithoutExtension(this.Path);

        public string Path { get; }

        public bool IsFolder { get; }

        private ObservableCollection<DocumentViewModel> GetChildrens()
        {
            var childsDirs = Directory.EnumerateDirectories(this.Path);
            var childfiles = Directory.EnumerateFiles(this.Path);

            var allFiles = childsDirs.Select(n => new DocumentViewModel(n, true)).Concat(
                childfiles.Where(n => n.EndsWith(FileExtension)).Select(n => new DocumentViewModel(n, false)));

            return new ObservableCollection<DocumentViewModel>(allFiles);
        }
    }
}
