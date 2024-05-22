using System;

class Program
{
    [STAThread]  // This attribute is necessary for compatibility with components that require STA, like OpenFileDialog.
    static void Main()
    {
        using (var game = new Vizualizationizer())
            game.Run();
    }
}