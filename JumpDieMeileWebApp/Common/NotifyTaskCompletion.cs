namespace JumpDieMeileWebApp.Common
{
    public class NotifyTaskCompletion<TResult> : NotifyTaskCompletion
    {
        public NotifyTaskCompletion(System.Threading.Tasks.Task<TResult> task, System.Action<NotifyTaskCompletion<TResult>>? taskCompletedAction = null, bool configureAwait = false)
            : base(task, completion => taskCompletedAction?.Invoke((NotifyTaskCompletion<TResult>)completion), configureAwait)
        {
        }

        public System.Threading.Tasks.Task<TResult> ResultTask => (System.Threading.Tasks.Task<TResult>)this.Task;

        public TResult? Result => this.IsSuccessfullyCompleted ? this.ResultTask.Result : default;
    }

    public class NotifyTaskCompletion : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly System.Action<NotifyTaskCompletion>? taskCompletedAction;

        public NotifyTaskCompletion(System.Threading.Tasks.Task task, System.Action<NotifyTaskCompletion>? taskCompletedAction = null, bool configureAwait = false)
        {
            this.taskCompletedAction = taskCompletedAction;
            this.Task = task;

            if (!task.IsCompleted)
            {
                _ = this.WatchTaskAsync(task, configureAwait);
            }
            else
            {
                taskCompletedAction?.Invoke(this);
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public System.Threading.Tasks.Task Task { get; }

        public bool IsCancelCompletionActionRequested { get; private set; }

        public System.Threading.Tasks.TaskStatus Status => this.Task.Status;

        public bool IsCompleted => this.Task.IsCompleted;

        public bool IsNotCompleted => !this.Task.IsCompleted;

        public bool IsSuccessfullyCompleted => this.Task.Status == System.Threading.Tasks.TaskStatus.RanToCompletion;

        public bool IsCanceled => this.Task.IsCanceled;

        public bool IsFaulted => this.Task.IsFaulted;

        public System.AggregateException? Exception => this.Task.Exception;

        public System.Exception? InnerException => this.Exception?.InnerException;

        public string ErrorMessage => this.InnerException == null ? string.Empty : this.InnerException.Message;

        public void CancelCompletionAction()
        {
            this.IsCancelCompletionActionRequested = true;
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(this.IsCancelCompletionActionRequested)));
        }

        private async System.Threading.Tasks.Task WatchTaskAsync(System.Threading.Tasks.Task task, bool configureAwait)
        {
            try
            {
                await task.ConfigureAwait(configureAwait);
            }
            catch
            {
                // this is intended, exception information is read from task object
            }

            if (!this.IsCancelCompletionActionRequested)
            {
                this.taskCompletedAction?.Invoke(this);
            }

            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(string.Empty));
        }
    }
}