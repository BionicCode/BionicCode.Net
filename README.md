# BionicCode.Net
Collection of .NET libraries like utilities and controls that target .NET Standard, .NET Core, .NET Framework, .NET Core WPF, .NET Framework WPF andUWP



## [NuGet package](https://www.nuget.org/packages/BionicUtilities.Net/)

## [Class Reference](https://rawcdn.githack.com/BionicCode/BionicUtilities.Net/d1765f5da61687fac04e0fd7fc3ca4142935ecf1/BionicUtilities.Net/Documentation/Help/index.html)

## Contains 
* [`BaseViewModel`](https://github.com/BionicCode/BionicLibraryNet#baseviewmodel)
* [`AsyncRelayCommand<T>`](https://github.com/BionicCode/BionicLibraryNet#asyncrelaycomandt)
* Extension Methods for WPF e.g.
  * `TryFindVisualParentElement<TParent> : bool` 
  * `TryFindVisualParentElementByName : bool` 
  * `TryFindVisualChildElement<TChild> : bool`
  * `TryFindVisualChildElementByName : bool`
  * `FindVisualChildElements<TChildren> : IEnumerable<TChildren>`
  * `ICollection.AddRange<T>`
* EventArgs
  * [`ValueChangedEventArgs<T>`](https://github.com/BionicCode/BionicUtilities.Net#valuechangedeventargst)
* ValueConverters
  * `BoolToStringConverter`
  * `BooleanMultiValueConverter`
  * `FilePathTruncateConverter`
  * `InvertValueConverter`
* Collections
  * `ObservablePropertyChangedCollection<T>`
* MarkupExtensions
  * `PrimitiveTypeExtension`
* [`Profiler`](https://github.com/BionicCode/BionicLibraryNet#Profiler)
* [`AppSettingsConnector`](https://github.com/BionicCode/BionicLibraryNet#AppSettingsConnector) - A defaul API to the AppSettings that provides strongly typed reading and writing (e.g. `boo`, `int`, `double`, `string`) of key-value pair values
* [`MruManager`](https://github.com/BionicCode/BionicUtilities.Net/blob/master/README.md#mru-most-recently-used-file-manager) - Most Recently Used (MRU) file manager. An API that maintains an MRU table stored in the Application Settings file. 
* [`EventAggregator`](https://github.com/BionicCode/BionicUtilities.Net#eventaggregator) - Implememtation of the EventAggregator pattern that supports dynamic aggregation of different typed event sources
* Easy to use [`Dialog` attached behavior](https://github.com/BionicCode/BionicUtilities.Net/blob/master/README.md#mvvm-dialog-attached-behavior) and infrastucture to allow MVVM friendly dialog handling from the view models in a fire-and-forget manner. To display dialogs implement `IDialogViewModel` classes and create a `DataTemplate` for each implementation. The `DataTemplate is the rendered in a native `Window`. Addition attached properties allow for styling of the dialog `Window` or to assign an optional `DataTemplateSelector`. The attached behavior will handle showing and closing of  the dialog.
  
### `BaseViewModel`
* Implements and encapsulates `INotifyPropertyChanged` and `INotifyDataErrorInfo`. 
* Allows to control whether invalid data is set on a property or neglected until validation passes by setting the default parameter `isRejectInvalidValueEnabled` of `TrySetValue()` to `true` (neglects invalid values by default). 
* Also allows to control whether to throw an exception on validation error or not (silent) by setting the default parameter `isThrowExceptionOnValidationErrorEnabled` of `TrySetValue()` to `true` (is silent by default).
* Additionally exposes a `PropertyValueChanged` event which is raised in tandem with `INotifyPropertyChanged.PropertyChanged` but additionally carries old value and new value as event args.

#### Example with validation

```c#
private string name;
public string Name
{
  get => this.name;
  set
  {
    if (TrySetValue(
      value,
      (stringValue) =>
      {
        var messages = new List<string>() {"Name must start with an underscore"};
        return (stringValue.StartsWith("_"), messages);
      },
      ref this.name))
    {
      DoSomething(this.name);
    }
  }
}
```
#### Example without validation

```c#
private string name;
public string Name
{
  get => this.name;
  set
  {
    if (TrySetValue(value, ref this.name))
    {
      DoSomething(this.name);
    }
  }
}
```
----

### `AsyncRelayComand<T>` 
Reusable generic command class that encapsulates `ICommand` and allows asynchronous execution.
When used with a `Binding` the command will execute asynchronously when an awaitable execute handler is assigned to the command.

#### Example

```c#
// ICommand property
public IAsyncRelayCommand<string> StringAsyncCommand => new AsyncRelayCommand<string>(ProcessStringAsync);
    
// Execute asynchronously
await StringAsyncCommand.ExecuteAsync("String value");
    
// Execute synchronously
StringAsyncCommand.Execute("String value");
    
```

### `Profiler`
Static helper methods to measure performance e.g. the execution time of a code portion.

#### Example

```c#
// Specify a custom output
Profiler.LogPrinter = (timeSpan) => PrintToFile(timeSpan);
    
// Measure the average execution time of a specified number of iterations.
TimeSpan elapsedTime = Profiler.LogAverageTime(() => ReadFromDatabase(), 1000);
    
// Measure the execution times of a specified number of iterations.
List<TimeSpan> elapsedTime = Profiler.LogTimes(() => ReadFromDatabase(), 1000);
    
// Measure the execution time.
TimeSpan elapsedTime = Profiler.LogTime(() => ReadFromDatabase());
```
### `ValueChangedEventArgs<T>`
Generic `EventArgs` implementation that provides value change information like `OldValue` and `NewValue`.

#### Example

```c#
// Specify a named ValueTuple as event argument
event EventHandler<ValueChangedEventArgs<(bool HasError, string Message)>> Completed;    
    
protected virtual void RaiseCompleted((bool HasError, string Message) oldValue, (bool HasError, string Message) newValue)
{
  this.Completed?.Invoke(this, new ValueChangedEventArgs<(bool HasError, string Message)>(oldValue, newValue));
}

private void OnCompleted(object sender, ValueChangedEventArgs<(bool HasError, string Message)> e)
{
  (bool HasError, string Message) newValue = e.NewValue;
  if (newValue.HasError)
  {
    this.TaskCompletionSource.TrySetException(new InvalidOperationException(newValue.Message));
  }
  this.TaskCompletionSource.TrySetResult(true);
}
```
### `AppSettingsConnector` 
A static default API to the AppSettings that provides strongly typed reading and writing (e.g. `boo`, `int`, `double`, `string`) of key-value pair values.

#### Example

```c#
// Write the Most Recently Used file count to the AppSettings file
AppSettingsConnector.WriteInt("mruCount", 10);

// If key exists read the Most Recently Used file count from the AppSettings file
if (TryReadInt("mruCount", out int mruCount))
{
  this.MruCount = mruCount;
}
```

### MRU (Most Recently Used) file manager
The `MruManager` maintains a collection of `MostRecentlyUsedFileItem` elrecentlyements that map to a recently used file. Once the max number of recently used files is reached and a new file is added, the `MruManager` automatically removes the least used file to free the MRU table.

#### Example: Save file to MRU list

```c#

var mruManager = new MruManager();
mruManager.MaxMostRecentlyUsedCount = 30;

var openFileDialog = new OpenFileDialog();
bool? result = openFileDialog.ShowDialog();

// Process open file dialog box results
if (result == true)
{
  string filename = openFileDialog.FileName;

  // Save the picked file in the MRU list
  mruManager.AddMostRecentlyUsedFile(filename);
}
```

#### Example: Read file from MRU list

```c#

var mruManager = new MruManager();
mruManager.MaxMostRecentlyUsedCount = 30;

MostRecentlyUsedFileItem lastUsedFile = mruManager.MostRecentlyUsedFile;

// Since the list is a ReadOnlyObservableCollection you can directly bind to it 
// and receive CollectionChanged notifications, which will automatically update the binding target
ReadOnlyObservableCollection<MostRecentlyUsedFileItem> mruList = mruManager.MostRecentlyUsedFiles;
```

### `EventAggregator`
Dynamic implementation of the EventAggregator design pattern. Listen to events broadcasted by a specific type or by a specific event.

#### Example
##### Aggregate events
Let the `EventAggregator` subscribe to events:

```C#
var aggregator = new EventAggregator();

// Create instances that are source of an event
var mainWindowViewModel = new MainWindowViewModel();
var mainPageViewModel = new MainPageViewModel();
var settingsPageViewModel = new SettingsPageViewModel();

// Listen to a list of events published by a specific instance
aggregator.TryRegisterObservable(mainWindowViewModel, 
  new[] 
  {
    nameof(INotifyPropertyChanged.PropertyChanged), 
    nameof(MainWindowViewModel.ItemCreated)
  });
aggregator.TryRegisterObservable(mainPageViewModel, new[] {nameof(INotifyPropertyChanged.PropertyChanged)});
aggregator.TryRegisterObservable(settingsPageViewModel, new[] {nameof(INotifyPropertyChanged.PropertyChanged)});
```

##### Listen to all aggregated event sources by event name
Subscribe to the `EventAggregator` and listen to specific events of all aggregated event sources:

```C#
// Listen to everything that publishes the 'INotifyPropertyChanged.PropertyChanged' event
aggregator.TryRegisterObserver<PropertyChangedEventHandler>(nameof(INotifyPropertyChanged.PropertyChanged), (s, args) => MessageBox.Show($"'PropertyChanged event'. Sender={sender.GetType().Name}; Value={args.PropertyName}"));
```

##### Listen to specific aggregated event sources by event name
Subscribe to the `EventAggregator` and listen to specific events of specific aggregated event sources:

```C#
// Only listen to the 'INotifyPropertyChanged.PropertyChanged' event of the 'mainWindowViewModel' instance
aggregator.TryRegisterObserver<PropertyChangedEventHandler>(nameof(INotifyPropertyChanged.PropertyChanged), mainWindowViewModel.GetType(), (s, args) => MessageBox.Show($"'PropertyChanged event'. Sender={sender.GetType().Name}; Value={args.PropertyName}"));

// Only listen to the 'INotifyPropertyChanged.PropertyChanged' event of all instances that implemnt 'IPage'
aggregator.TryRegisterObserver<PropertyChangedEventHandler>(nameof(INotifyPropertyChanged.PropertyChanged), typeof(IPage), (s, args) => MessageBox.Show($"'PropertyChanged event'. Sender={sender.GetType().Name}; Value={args.PropertyName}"));
```

##### Type declarations used in examples

```C#
class MainPageViewModel : IPage, INotifyPropertyChanged
{
  public string Title 
  { 
    private string title;
    get => this.title; 
    set 
    { 
      this.title = value;
      OnPropertyChanged();
    }
  }
}

class SettingsPageViewModel : IPage, INotifyPropertyChanged
{
  private string title;
  public string Title 
  { 
    get => this.title; 
    set 
    { 
      this.title = value;
      OnPropertyChanged();
    }
  }
}

class MainWindowViewModel : INotifyPropertyChanged
{
  public void CreateItem()
  {
    this.Items.Add("New Item");
    OnItemCreated();
  }
  
  private ObservableCollection<string> items;
  public ObservableCollection<string> Items
  { 
    get => this.items; 
    set 
    { 
      this.items = value;
      OnPropertyChanged();
    }
  }
  
  public event PropertyChangedEventHandler PropertyChanged;
  protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  
  public event EventHandler ItemCreated;
  protected virtual void OnItemCreated() => this.ItemCreated?.Invoke(this, EventArgs.Empty);
}
```
### MVVM Dialog attached behavior
Provides a clean way to show dialogs that are requested by view models. This apporach uses dialog view models that are templated by specific `DataTemplate` definitions, This templates are rendered as content of native dialog `Window` instances.

Because once the dialog was closed a custom asynchronous continuation delegate is invoked by the `DialogViewModel` the dialog can be shown without the requirement for the requesting view model to wait for a response. This way the dialogs can be shown in a fire-and-forget manner supporting asynchronous delegates.

`DialogViewModel` is the only mandatory abstract class (or alternatively the `IDialogViewModel` interface) to implement. `DialogViewModel` offers ready to use logic for the data and response handling. Virtual methods add a way to add customization or adjust behavior. The `Dialog` attached behavior handles the view logic which includes aplying styles and templates on the dialog and showing/ closing the Window.

The interfaces `IDialogViewModelProvider` and `IDialogViewModelProviderSource` are not a requirement and are supposed to provide a clean separation. The `IDialogViewModelProviderSource.DialogRequested` event driven flow can be omitted. Just provide a mechanism to bind a `DialogViewModel` (or `IDialogViewModel`) implementation to the `Dialog.DialogDataContext` Attached Property. `Dialog` and `DialogViewMoel` are the only core classes that are required to make it work.


#### Minimal Example

##### Implementing IDialogViewModelProvider (just provide a binding source for the `Dialog` attached behavior)

```C#
class MainWindowViewModel : IDialogViewModelProvider
{
  public void ShowDialog()
  {  
    // Create the IDialogViewModel for the File Exists dialog
    var dialogTitleBarIcon = new BitmapImage(new Uri("../../logo.ico", UriKind.Relative));
    if (titleBarIcon.CanFreeze)
    {
      titleBarIcon.Freeze();
    }
    
    var message = "File exists. Do you want to replace it?";
    var dialogTitle = "File Exists";
      
    // Set the continuation callback which will be invoked once the dialog closed
    var fileExistsdialogViewModel = new FileExistsDialogViewModel(
      message, 
      dialogTitle, 
      dialogTitleBarIcon, 
      dialogViewModel => HandleFileExistsDialogResponseAsync(dialogViewModel, filePath, settingsData));
    
    // Show the dialog by setting the DialogViewModel property to an instance of IDialogViewModel
    this.DialogViewModel = fileExistsdialogViewModel;
  }
  
  // Continuation callback. Will be invoked once the dialog closed. 
  // The parameter is the previously created FileExistsDialogViewmodel containing data set from the dialog.
  private async Task HandleFileExistsDialogResponseAsync(IDialogViewModel dialogViewModel, string filePath, string settingsData)
  {
    if (dialogViewModel.DialogResult == DialogResult.Accepted)
    {
      await SaveFileAsync(filePath, settingsData);
    }
  }
  
  // IDialogViewModelProvider interface implementation
  private IDialogViewModel dialogViewModel;  
  public IDialogViewModel DialogViewModel
  {
    get => this.dialogViewModel;
    private set => TrySetValue(value, ref this.dialogViewModel);
  }
}
```

##### Implementing DialogViewModel (required)
This is the only mandatory abstract class (or alternatively the `IDialogViewModel` interface) to implement.
The interfaces `IDialogViewModelProvider` and `IDialogViewModelProvider` are just to provide a clean separation. The `IDialogViewModelProviderSource.DialogRequested` can be omitted. Just provide a mechanism to bind a `DialogViewModel` (or `IDialogViewModel`) implementation to the `Dialog.DialogDataContext` Attached Property. 
`Dialog` and `DialogViewMoel` are the core classes to make it work.

```C#
public class FileExistsDialogViewModel : DialogViewModel
{
  public FileExistsDialogViewModel(string message, string title) : base(message, title)
  { 
  }
  public FileExistsDialogViewModel(string message, string title, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : base(message, title, sendResponseCallbackAsync)
  { 
  }
  public FileExistsDialogViewModel(string message, string title, ImageSource titleBarIcon, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : base(message, title, titleBarIcon, sendResponseCallbackAsync)
  { 
  }
}
```

##### Implementing `DataTemplate` for `FileExistsDialogViewModel` (required)
Make sure the templates are declared in the proper scope. It is recommended to declare them in the App.xaml.

```XAML
Application x:Class="BionicCode.BionicNuGetDeploy.Main.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pages="clr-namespace:BionicCode.BionicNuGetDeploy.Main.Pages"
             xmlns:dialog="clr-namespace:BionicUtilities.Net.Dialog;assembly=BionicUtilities.Net"
             Startup="RunApplication">
    <Application.Resources>

      <Viewbox x:Key="WarningIcon"
               x:Shared="False">
        <ContentControl FontFamily="Segoe MDL2 Assets"
                        Content="&#xE814;" />
      </Viewbox>

      <Viewbox x:Key="WarningLightIcon"
               x:Shared="False">
        <ContentControl FontFamily="Segoe MDL2 Assets"
                        Content="&#xE7BA;" />
      </Viewbox>
    
      <DataTemplate DataType="{x:Type pages:FileExistsDialogViewModel}">
        <Grid Margin="12">
          <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
          </Grid.RowDefinitions>
          <StackPanel Grid.Row="0"
                      Orientation="Horizontal"
                      Margin="0,0,48,24">
            <Grid Margin="0,0,16,0">
              <ContentControl Panel.ZIndex="1"
                              Content="{StaticResource WarningIcon}"
                              VerticalAlignment="Center"
                              Height="32"
                              Foreground="Orange"
                              Background="Black" />
              <ContentControl Panel.ZIndex="2"
                              Content="{StaticResource WarningLightIcon}"
                              VerticalAlignment="Center"
                              Height="32"
                              Margin="0,4,0,0" />
            </Grid>
            <TextBlock Text="{Binding Message}" />
          </StackPanel>
          <StackPanel Grid.Row="1"
                      FocusManager.FocusedElement="{Binding ElementName=CancelButton}"
                      Orientation="Horizontal"
                      HorizontalAlignment="Right">
            <Button Content="Yes"
                    Padding="0"
                    Command="{Binding SendResponseAsyncCommand}"
                    CommandParameter="{x:Static dialog:DialogResult.Accepted}"
                    Margin="0,0,16,0" />
            <Button x:Name="CancelButton"
                    Content="No"
                    IsCancel="True"
                    IsDefault="True"
                    BorderThickness="3"
                    Padding="0"
                    Command="{Binding SendResponseAsyncCommand}"
                    CommandParameter="{x:Static dialog:DialogResult.Denied}" />

          </StackPanel>
        </Grid>
      </DataTemplate>
  </Application.Resources>
</Application>
```


##### Setting the Attached Property  `Dialog.DialogDataContext` on `Window` (or any other `FrameworkElement` - required)

```XAML
<Window x:Class="BionicCode.BionicNuGetDeploy.Main.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"        
        xmlns:dialog="clr-namespace:BionicUtilities.Net.Dialog;assembly=BionicUtilities.Net"
        mc:Ignorable="d"
        Title="MainWindow"
        dialog:Dialog.DialogDataContext="{Binding DialogViewModel}">
</Window>      
```

#### Extended Example

##### Implementing IDialogViewModelProviderSource (optional)

```C#

class SettingsPageViewModel : IDialogViewModelProviderSource
{
  public async Task TrySaveFileAsync(string filePath, string settingsData)
  {
    if (File.Exists(filePath))
    {
      // Create the IDialogViewModel for the File Exists dialog
      var dialogTitleBarIcon = new BitmapImage(new Uri("../../logo.ico", UriKind.Relative));
      if (titleBarIcon.CanFreeze)
      {
        titleBarIcon.Freeze();
      }
      var message = "File exists. Do you want to replace it?";
      var dialogTitle = "File Exists";
      
      // Set the continuation callback which will be invoked once the dialog closed
      var fileExistsdialogViewModel = new FileExistsDialogViewModel(
        message, 
        dialogTitle, 
        dialogTitleBarIcon, 
        dialogViewModel => HandleFileExistsDialogResponseAsync(dialogViewModel, filePath, settingsData));
      
      // Request File Exists dialog to ask if existing file can be overwritten
      OnDialogRequested(newfileExistsdialogViewModel);
      return;
    }
    
    await SaveFileAsync(filePath, settingsData);
  }
  
  // Continuation callback. Will be invoked once the dialog closed. 
  // The parameter is the previously created FileExistsDialogViewmodel containing data set from the dialog.
  private async Task HandleFileExistsDialogResponseAsync(IDialogViewModel dialogViewModel, string filePath, string settingsData)
  {
    if (dialogViewModel.DialogResult == DialogResult.Accepted)
    {
      await SaveFileAsync(filePath, settingsData);
    }
  }

  // IDialogViewModelProviderSource interface implementation
  public event EventHandler<ValueEventArgs<IDialogViewModel>> DialogRequested;
  protected virtual void OnDialogRequested(IDialogViewModel dialogViewModel)
  {
    this.DialogRequested?.Invoke(this, new ValueEventArgs<IDialogViewModel>(dialogViewModel));
  }
}
```

##### Implementing IDialogViewModelProvider (optional - just provide a binding source for the `Dialog` attached behavior)

```C#
class MainWindowViewModel : IDialogViewModelProvider
{
  public MainPageViewModel()
  {
    // A view model that implements IDialogViewModelProvider and can request dispalying of a dialog
    var settingsPageViewModel = new SettingsPageViewModel();
    
    // Listen for dialog requests and provide the dialog view model for binding of the attahced behavior.    
    // Show the dialog by setting the DialogViewModel property to an instance of IDialogViewModel.
    settingsPageViewModel.DialogRequested += (sender, args) => this.DialogViewModel = args.Value);
    
    this.Pages = new ObservableCollection<IPage>() { settingsPageViewModel };
  }
  
  // IDialogViewModelProvider interface implementation
  private IDialogViewModel dialogViewModel;  
  public IDialogViewModel DialogViewModel
  {
    get => this.dialogViewModel;
    private set => TrySetValue(value, ref this.dialogViewModel);
  }
}
```

##### Implementing DialogViewModel (required)
This is the only mandatory abstract class (or alternatively the `IDialogViewModel` interface) to implement.
The interfaces `IDialogViewModelProvider` and `IDialogViewModelProvider` are just to provide a clean separation. The `IDialogViewModelProviderSource.DialogRequested` can be omitted. Just provide a mechanism to bind a `DialogViewModel` (or `IDialogViewModel`) implementation to the `Dialog.DialogDataContext` Attached Property. 
`Dialog` and `DialogViewMoel` are the core classes to make it work.

```C#
public class FileExistsDialogViewModel : DialogViewModel
{
  public FileExistsDialogViewModel(string message, string title) : base(message, title)
  { 
  }
  public FileExistsDialogViewModel(string message, string title, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : base(message, title, sendResponseCallbackAsync)
  { 
  }
  public FileExistsDialogViewModel(string message, string title, ImageSource titleBarIcon, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : base(message, title, titleBarIcon, sendResponseCallbackAsync)
  { 
  }
}
```

##### Implementing `DataTemplate` for `FileExistsDialogViewModel` (required)
Make sure the templates are declared in the proper scope. It is recommended to declare them in the App.xaml.

```XAML
Application x:Class="BionicCode.BionicNuGetDeploy.Main.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pages="clr-namespace:BionicCode.BionicNuGetDeploy.Main.Pages"
             xmlns:dialog="clr-namespace:BionicUtilities.Net.Dialog;assembly=BionicUtilities.Net"
             Startup="RunApplication">
    <Application.Resources>

      <Viewbox x:Key="WarningIcon"
               x:Shared="False">
        <ContentControl FontFamily="Segoe MDL2 Assets"
                        Content="&#xE814;" />
      </Viewbox>

      <Viewbox x:Key="WarningLightIcon"
               x:Shared="False">
        <ContentControl FontFamily="Segoe MDL2 Assets"
                        Content="&#xE7BA;" />
      </Viewbox>
    
      <DataTemplate DataType="{x:Type pages:FileExistsDialogViewModel}">
        <Grid Margin="12">
          <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
          </Grid.RowDefinitions>
          <StackPanel Grid.Row="0"
                      Orientation="Horizontal"
                      Margin="0,0,48,24">
            <Grid Margin="0,0,16,0">
              <ContentControl Panel.ZIndex="1"
                              Content="{StaticResource WarningIcon}"
                              VerticalAlignment="Center"
                              Height="32"
                              Foreground="Orange"
                              Background="Black" />
              <ContentControl Panel.ZIndex="2"
                              Content="{StaticResource WarningLightIcon}"
                              VerticalAlignment="Center"
                              Height="32"
                              Margin="0,4,0,0" />
            </Grid>
            <TextBlock Text="{Binding Message}" />
          </StackPanel>
          <StackPanel Grid.Row="1"
                      FocusManager.FocusedElement="{Binding ElementName=CancelButton}"
                      Orientation="Horizontal"
                      HorizontalAlignment="Right">
            <Button Content="Yes"
                    Padding="0"
                    Command="{Binding SendResponseAsyncCommand}"
                    CommandParameter="{x:Static dialog:DialogResult.Accepted}"
                    Margin="0,0,16,0" />
            <Button x:Name="CancelButton"
                    Content="No"
                    IsCancel="True"
                    IsDefault="True"
                    BorderThickness="3"
                    Padding="0"
                    Command="{Binding SendResponseAsyncCommand}"
                    CommandParameter="{x:Static dialog:DialogResult.Denied}" />

          </StackPanel>
        </Grid>
      </DataTemplate>
  </Application.Resources>
</Application>
```


##### Setting the Attached Property  `Dialog.DialogDataContext` on `Window` (or any other `FrameworkElement` - required)

```XAML
<Window x:Class="BionicCode.BionicNuGetDeploy.Main.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"        
        xmlns:dialog="clr-namespace:BionicUtilities.Net.Dialog;assembly=BionicUtilities.Net"
        mc:Ignorable="d"
        Title="MainWindow"
        dialog:Dialog.DialogDataContext="{Binding DialogViewModel}">
</Window>      
```

