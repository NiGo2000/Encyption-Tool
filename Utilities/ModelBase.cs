using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Encrypter.Utilities
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers a propertychanged event, allowing the view to be updated. Pass your private property, new value, can also pass the property name, but this is done for you.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property">the private field that is used for "setting"</param>
        /// <param name="newValue">the update value of this property</param>
        /// <param name="propertyName">name of the property, need not be specified/field</param>
        public void RaisePropertyChanged<T>(ref T property, T newValue, [CallerMemberName] string propertyName = "")
        {
            property = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
