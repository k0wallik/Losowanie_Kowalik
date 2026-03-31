using System.Collections.ObjectModel;

namespace Kowalik_Losowanie;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<string> _classNames = new();
    private readonly ObservableCollection<string> _students = new();
    private readonly Random _random = new();
    private readonly string _classesDirectoryPath;
    private string? _selectedStudent;

    public MainPage()
    {
        InitializeComponent();

        _classesDirectoryPath = Path.Combine(FileSystem.AppDataDirectory, "classes");
        Directory.CreateDirectory(_classesDirectoryPath);

        ClassPicker.ItemsSource = _classNames;
        StudentsCollection.ItemsSource = _students;

        LoadClassNames();
    }
    

    
    private Task ShowError(string message) =>
        DisplayAlert("Błąd", message, "OK");

    private string GetClassFilePath(string className)
    {
        var safeName = string.Concat(className.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
        return Path.Combine(_classesDirectoryPath, $"{safeName}.txt");
    }
    
    
    
    private void LoadClassNames()
    {
        _classNames.Clear();

        foreach (var filePath in Directory.GetFiles(_classesDirectoryPath, "*.txt"))
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            _classNames.Add(className);
        }
    }

    private async void OnAddClassClicked(object sender, EventArgs e)
    {
        string className = null;

        if (ClassEntry.Text != null)
        {
            className = ClassEntry.Text.Trim();
        }

        if (className == null || className == "")
        {
            await ShowError("Podaj nazwę klasy.");
            return;
        }

        if (!_classNames.Contains(className))
        {
            _classNames.Add(className);
        }

        ClassPicker.SelectedItem = className;
        ResultLabel.Text = "Wybrana klasa: " + className;
    }
    
private async void OnSaveClassClicked(object sender, EventArgs e)
{
    string selectedClass = null;

    if (ClassPicker.SelectedItem != null)
    {
        selectedClass = ClassPicker.SelectedItem.ToString();
    }

    if (selectedClass == null || selectedClass == "")
    {
        await ShowError("Najpierw wybierz klasę.");
        return;
    }

    string filePath = GetClassFilePath(selectedClass);

    await File.WriteAllLinesAsync(filePath, _students);

    ResultLabel.Text = "Zapisano klasę " + selectedClass + ".";
}

private async void OnClassSelectedIndexChanged(object sender, EventArgs e)
{
    string selectedClass = null;

    if (ClassPicker.SelectedItem != null)
    {
        selectedClass = ClassPicker.SelectedItem.ToString();
    }

    if (selectedClass == null || selectedClass == "")
    {
        return;
    }

    _students.Clear();
    _selectedStudent = null;
    StudentEntry.Text = "";

    string filePath = GetClassFilePath(selectedClass);

    if (File.Exists(filePath))
    {
        string[] students = await File.ReadAllLinesAsync(filePath);
        foreach (string s in students)
        {
            if (s != null && s != "")
            {
                _students.Add(s.Trim());
            }
        }
    }

    ClassEntry.Text = selectedClass;
    ResultLabel.Text = "Wczytano listę klasy " + selectedClass + ".";
}

private async void OnAddStudentClicked(object sender, EventArgs e)
{
    string name = null;

    if (StudentEntry.Text != null)
    {
        name = StudentEntry.Text.Trim();
    }

    if (name == null || name == "")
    {
        await ShowError("Podaj imię i nazwisko ucznia.");
        return;
    }

    _students.Add(name);
    StudentEntry.Text = "";
}

private async void OnEditStudentClicked(object sender, EventArgs e)
{
    string newName = null;

    if (StudentEntry.Text != null)
    {
        newName = StudentEntry.Text.Trim();
    }

    if (_selectedStudent == null || _selectedStudent == "")
    {
        await ShowError("Wybierz ucznia z listy do edycji.");
        return;
    }

    if (newName == null || newName == "")
    {
        await ShowError("Wpisz nową nazwę ucznia.");
        return;
    }

    int index = _students.IndexOf(_selectedStudent);
    // zwraca -1 jeśli element nie istnieje

    if (index >= 0)
    {
        _students[index] = newName;
        _selectedStudent = newName;
        StudentsCollection.SelectedItem = newName;
        ResultLabel.Text = "Dane ucznia zostały zaktualizowane.";
    }
}

private async void OnDeleteStudentClicked(object sender, EventArgs e)
{
    if (_selectedStudent == null || _selectedStudent == "")
    {
        await ShowError("Wybierz ucznia z listy do usunięcia.");
        return;
    }

    _students.Remove(_selectedStudent);
    _selectedStudent = null;
    StudentEntry.Text = "";
    StudentsCollection.SelectedItem = null;
}

private void OnStudentSelectionChanged(object sender, SelectionChangedEventArgs e)
{
    string selected = null;

    if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
    {
        selected = e.CurrentSelection[0].ToString();
    }

    if (selected != null)
    {
        _selectedStudent = selected;
        StudentEntry.Text = selected;
    }
    else
    {
        _selectedStudent = null;
        StudentEntry.Text = "";
    }
}

private async void OnDrawStudentClicked(object sender, EventArgs e)
    {
        if (_students.Count == 0)
        {
            await ShowError("Lista uczniów jest pusta.");
            return;
        }

        int index = _random.Next(_students.Count);
        string chosen = _students[index];

        ResultLabel.Text = "Wylosowano: " + chosen;
        await DisplayAlert("Wynik losowania", chosen, "OK");
    }
}