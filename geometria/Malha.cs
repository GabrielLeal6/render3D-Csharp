using System.Globalization;
using System.Security.Cryptography.X509Certificates;

public struct Malha{

    public Triangulo[] triangulos {get; private set;}
    int qtTriangulos;
    public Vertice[] vertices {get; private set;}
    int qtVertices;

    public Malha(string path){
        try{
            using (StreamReader sr = new(path)){
                string[] line = sr.ReadLine().Split();
                qtVertices = Int32.Parse(line[0]);
                qtTriangulos = Int32.Parse(line[1]);

                vertices = new Vertice[qtVertices];
                triangulos = new Triangulo[qtTriangulos];

                for (int i = 0; i < qtVertices; i++){
                    line = sr.ReadLine().Split();
                    double x = Double.Parse(line[0], CultureInfo.InvariantCulture);
                    double y = Double.Parse(line[1], CultureInfo.InvariantCulture);
                    double z = Double.Parse(line[2], CultureInfo.InvariantCulture);
                    vertices[i] = new(x, y, z);
                }
                for (int i = 0; i < qtTriangulos; i++){
                    line = sr.ReadLine().Split();
                    int v1 = Int32.Parse(line[0]) - 1;
                    int v2 = Int32.Parse(line[1]) - 1;
                    int v3 = Int32.Parse(line[2]) - 1;
                    triangulos[i] = new(v1, v2, v3);
                }
            }
        } catch (Exception e){
            Console.WriteLine("Arquivo não pode ser lido");
            Console.WriteLine(e.Message);
        }
    }
    public void printMalha(){
        Console.WriteLine($"{qtVertices} {qtTriangulos}");
        for (int i = 0; i < qtVertices; i++){
            Console.WriteLine($"{vertices[i].X} {vertices[i].Y} {vertices[i].Z}");
        }
        for (int i = 0; i < qtTriangulos; i++){
            Console.WriteLine($"{triangulos[i].V1 + 1} {triangulos[i].V2 + 1} {triangulos[i].V3 + 1}");
        }
    }

}