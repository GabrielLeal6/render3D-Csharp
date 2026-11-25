using System.Globalization;

public struct Malha
{

    public Triangulo[] Triangulos { get; private set; }
    int qtTriangulos;
    public Ponto3D[] Vertices { get; private set; }
    int qtVertices;

    public Vetor3D[] normaisVertices;

    public void CalcularNormais(){
        normaisVertices = new Vetor3D[Vertices.Length];

        for (int i = 0; i < Triangulos.Length; i++)
        {
            Triangulo t = Triangulos[i];
            Vetor3D normalFace = t.CalcularNormal(Vertices);

            normaisVertices[t.V1] += normalFace;
            normaisVertices[t.V2] += normalFace;
            normaisVertices[t.V3] += normalFace;
        }

        normaisVertices = Vetor3D.Normalizar(normaisVertices);
    }

    public Malha(string path)
    {
        try{
            using (StreamReader sr = new(path))
            {
                string[] line = sr.ReadLine().Split();
                qtVertices = Int32.Parse(line[0]);
                qtTriangulos = Int32.Parse(line[1]);

                Vertices = new Ponto3D[qtVertices];
                Triangulos = new Triangulo[qtTriangulos];

                for (int i = 0; i < qtVertices; i++)
                {
                    line = sr.ReadLine().Split();
                    double x = Double.Parse(line[0], CultureInfo.InvariantCulture);
                    double y = Double.Parse(line[1], CultureInfo.InvariantCulture);
                    double z = Double.Parse(line[2], CultureInfo.InvariantCulture);
                    Vertices[i] = new(x, y, z);
                }
                for (int i = 0; i < qtTriangulos; i++)
                {
                    line = sr.ReadLine().Split();
                    int v1 = Int32.Parse(line[0]) - 1;
                    int v2 = Int32.Parse(line[1]) - 1;
                    int v3 = Int32.Parse(line[2]) - 1;
                    Triangulos[i] = new(v1, v2, v3);
                }
            }

            CalcularNormais();
        }
        catch (Exception e)
        {
            Console.WriteLine("Arquivo não pode ser lido");
            Console.WriteLine(e.Message);
        }
    }
    public readonly void PrintMalha()
    {
        Console.WriteLine($"{qtVertices} {qtTriangulos}");
        for (int i = 0; i < qtVertices; i++)
        {
            Console.WriteLine($"{Vertices[i].X} {Vertices[i].Y} {Vertices[i].Z}");
        }
        for (int i = 0; i < qtTriangulos; i++)
        {
            Console.WriteLine($"{Triangulos[i].V1 + 1} {Triangulos[i].V2 + 1} {Triangulos[i].V3 + 1}");
        }
    }

}