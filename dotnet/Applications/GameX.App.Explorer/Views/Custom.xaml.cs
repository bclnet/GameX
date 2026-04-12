using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace GameX.App.Explorer.Views;

/// <summary>
/// Interaction logic for Custom.xaml
/// </summary>
public partial class Custom : UserControl, INotifyPropertyChanged {
    public Custom() => InitializeComponent();

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(Custom), new PropertyMetadata((d, e) => (d as Custom).Load()));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Stream), typeof(Custom), new PropertyMetadata((d, e) => (d as Custom).Load()));

    public object Source {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public Stream Value {
        get => GetValue(ValueProperty) as Stream;
        set => SetValue(SourceProperty, value);
    }

    void Load() { }
}
