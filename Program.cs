using System.Data;

namespace Rasterizador3D;


static class Program
{

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }    
}