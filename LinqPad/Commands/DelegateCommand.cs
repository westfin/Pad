using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LinqPad.Commands
{
    public class DelegateCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<Task> executeAsync;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            this.executeAsync = executeAsync;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object parameter)
        {
            if(executeAsync !=null)
            {
                 ExecuteAsync();
            }
            else
            {
                execute();
            }
        }

        public void RaiseExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private async void ExecuteAsync()
        {
            await executeAsync().ConfigureAwait(true);
        }
    }
}
