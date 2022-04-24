using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WPF.MVVM.Annotations;
namespace WPF.MVVM
{
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		
		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		protected void OnPropertiesChanged(string[] propertiesNames = null)
		{
			foreach (var propertyName in propertiesNames ?? Enumerable.Empty<string>())
				OnPropertyChanged(propertyName);
		}
	}	
}
