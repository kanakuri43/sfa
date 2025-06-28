using Prism.Mvvm;

namespace Tracer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Tracer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
