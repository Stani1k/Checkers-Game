using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Checkers.Tools
{
    [DataContract]
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            var body = property.Body as MemberExpression;
            var handler = PropertyChanged;
            if (handler != null && body != null)
            {
                handler(this, new PropertyChangedEventArgs(body.Member.Name));
            }
        }
    }
}
