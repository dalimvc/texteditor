using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Net.WebRequestMethods;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace texteditor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //fileName stores value of the current file
        //newFileOpened and newFileStarted will be used to check if a file is edited or not
        //isFileSaved will be used to decide if a file should be automaticly saved or user will be asked about location to save a file
        string fileName;
        bool newFileOpened;
        bool newFileStarted;
        bool isFileSaved;
        private StorageFile savedLocation;

        public MainPage()
        {
            this.InitializeComponent();
            fileName = "doc.txt";
            FileName.Text = fileName;
        }

        //event listner that checks if file is edited
        //when creating a  new file file name will be "doc.txt". part => if (newFileStarted).....
        //if eventlistner detects chane in text box but file if not a new file opened(current file is changed) file with get "*" besides its name => else if (!newFileOpened)
        //otherwise file name would be presented without "*"
        //at the end newFileOpened and newFileStarted would be false
        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine(fileName);
            if (newFileStarted)
            {
                FileName.Text = fileName;
            }
            else if (!newFileOpened)
            {
                FileName.Text = fileName + "*";
                isFileSaved = false;
            }
            else
            {
                FileName.Text = fileName;
            }
            newFileOpened = false;
            newFileStarted = false;
        }

        //method to create a new file
        //if a file is edited and it is not saved user will be asked if they want to save a file
        //if they want to save it and there is a file location(where the file was previously saved), file would be automaticly saved into the same location
        //othevise the program will ask a user where they want to save a file
        //if user doesnt want to save the file or file is already saved, no dialog box will be shown and new file will be started automaticly
        private async void CreateNewFile(object sender, RoutedEventArgs e)
        {
            if (TextValue.Text != "" && !isFileSaved)
            {
                MessageDialog msg = new MessageDialog("Innan du skapar en ny fil, vill du spara den nuvarande filen?");
                msg.Commands.Add(new UICommand("Ja"));
                msg.Commands.Add(new UICommand("Nej"));
                var resultat = await msg.ShowAsync();
                if (resultat.Label == "Ja")
                {
                    newFileStarted = true;
                    if (savedLocation != null)
                    {
                        string textToSave = TextValue.Text;
                        await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
                        Debug.WriteLine(savedLocation.Name);
                        FileName.Text = savedLocation.Name;
                        isFileSaved = true;
                    }
                    else
                    {
                        string textToSave = TextValue.Text;
                        var fileSavePicker = new FileSavePicker();
                        fileSavePicker.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
                        savedLocation = await fileSavePicker.PickSaveFileAsync();
                        if (savedLocation != null)
                        {
                            await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
                            FileName.Text = savedLocation.Name;
                            isFileSaved = true;
                        }
                    }
                    TextValue.Text = "";
                }
                else if (resultat.Label == "Nej")
                {
                    newFileStarted = true;
                    TextValue.Text = "";
                }
                newFileOpened = false;
                fileName = "doc.txt";
            }
            else
            {
                newFileStarted = true;
                TextValue.Text = "";
                newFileOpened = false;
                fileName = "doc.txt";
            }
            savedLocation = null;
        }

        //method to open a text file
        //is there is a text in the editor that is not saved, user will be asked if they want to save it
        //if yes and file location existst(file was previously saved or opened)  file will be automaticly saved
        //othervise the program will ask a user where they want to save the file
        //after that a dialog box will be opened to ask a user what file they want to open
        //if a user dont want to save the file or file is already saved, user will be asked what file they want to open
        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            if (TextValue.Text != "" && !isFileSaved)
            {
                MessageDialog msg = new MessageDialog("Innan du öppnar en ny fil, vill du spara den nuvarande filen?");
                msg.Commands.Add(new UICommand("Nej"));
                msg.Commands.Add(new UICommand("Ja"));
                msg.Commands.Add(new UICommand("Avbryt"));

                var resultat = await msg.ShowAsync();
                if (resultat.Label == "Ja" && !isFileSaved)
                {
                    string textToSave = TextValue.Text;
                    var fileSavePicker = new FileSavePicker();
                    fileSavePicker.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
                    savedLocation = await fileSavePicker.PickSaveFileAsync();
                    if (savedLocation != null)
                    {
                        await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
                        isFileSaved = true;
                    }

                    var fileOpenPicker = new FileOpenPicker();
                    fileOpenPicker.FileTypeFilter.Add(".txt");
                    savedLocation = await fileOpenPicker.PickSingleFileAsync();
                    if (savedLocation != null)
                    {
                        var text = await Windows.Storage.FileIO.ReadTextAsync(savedLocation);
                        TextValue.Text = text;
                        isFileSaved = true;
                    }
                    fileName = savedLocation.Name;
                }
                else if (resultat.Label == "Nej")
                {
                    var fileOpenPicker = new FileOpenPicker();
                    fileOpenPicker.FileTypeFilter.Add(".txt");
                    savedLocation = await fileOpenPicker.PickSingleFileAsync();
                    if (savedLocation != null)
                    {
                        var text = await Windows.Storage.FileIO.ReadTextAsync(savedLocation);
                        TextValue.Text = text;
                        fileName = savedLocation.Name;
                        isFileSaved = true;
                    }
                }
                else if (resultat.Label == "Avbryt")
                {
                }
            }
            else
            {
                var fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.FileTypeFilter.Add(".txt");
                savedLocation = await fileOpenPicker.PickSingleFileAsync();
                if (savedLocation != null)
                {
                    var text = await Windows.Storage.FileIO.ReadTextAsync(savedLocation);
                    TextValue.Text = text;
                    fileName = savedLocation.Name;
                    isFileSaved = true;
                }
            }
            newFileOpened = true;
        }


        //method to save a file
        //if there is savedLocation, user would not be asked where they want to save the file and this would be done automaticly
        //othervise the program would ask a user where they want to save the file
        private async void SaveFile(object sender, RoutedEventArgs e)
        {
            if (savedLocation != null)
            {
                string textToSave = TextValue.Text;
                await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
                Debug.WriteLine(savedLocation.Name);
                fileName = savedLocation.Name;
                FileName.Text = savedLocation.Name;
                isFileSaved = true;
            }
            else
            {
                string textToSave = TextValue.Text;
                var fileSavePicker = new FileSavePicker();
                fileSavePicker.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
                savedLocation = await fileSavePicker.PickSaveFileAsync();
                if (savedLocation != null)
                {
                    await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
                    fileName = savedLocation.Name;
                    FileName.Text = savedLocation.Name;
                    isFileSaved = true;
                }
            }
        }

        //methos for save as...
        //it aleays asks user for a location to save the file
        private async void SaveFileAs(object sender, RoutedEventArgs e)
        {
            string textToSave = TextValue.Text;
            var fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
            savedLocation = await fileSavePicker.PickSaveFileAsync();
            if (savedLocation != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(savedLocation, textToSave);
            }
            if (savedLocation != null)
            {
                fileName = savedLocation.Name;
                FileName.Text = savedLocation.Name;
                isFileSaved = true;
            }
        }

        //method to clear the text box
        private void ClearFile(object sender, RoutedEventArgs e)
        {
            TextValue.Text = " ";
        }
    }
}
