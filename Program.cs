using System.Data;

namespace Rasterizador3D;


static class Program
{

    [STAThread]
    static void Main()
    {
        Malha exemplo = new("malha.txt");
        exemplo.printMalha();

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
        
    }    
}