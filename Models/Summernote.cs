namespace App.Models;

public class Summernote
{
    public Summernote(string iDEditor, bool loadLibrary = true)
    {
        IDEditor = iDEditor;
        LoadLibrary = loadLibrary;
    }

    public string IDEditor {set; get;}

    public bool LoadLibrary {set; get;}

    public int height {set; get;} = 120;
    public string toolbar {set; get;} = @"
        [
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video', 'elfinder']],
            ['height', ['height']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ]
    ";
}