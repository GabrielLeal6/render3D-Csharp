using System.Globalization;

public struct Camera
{

    public Ponto3D C;
    public Vetor3D N, V, U;
    public double d, hx, hy;

    public Camera(string path)
    {
        Load(path);
    }

    public void Load(string path)
    {
        try
        {
            using (StreamReader sr = new(path))
            {
                string[] line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                N = new(
                    Double.Parse(line[1], CultureInfo.InvariantCulture),
                    Double.Parse(line[2], CultureInfo.InvariantCulture),
                    Double.Parse(line[3], CultureInfo.InvariantCulture)
                );
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                V = new(
                    Double.Parse(line[1], CultureInfo.InvariantCulture),
                    Double.Parse(line[2], CultureInfo.InvariantCulture),
                    Double.Parse(line[3], CultureInfo.InvariantCulture)
                );
                U = Vetor3D.produtoVetorial(N, V - N * (Vetor3D.produtoEscalar(V, N) / Vetor3D.produtoEscalar(N, N)));
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                d = Double.Parse(line[1], CultureInfo.InvariantCulture);
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                hx = Double.Parse(line[1], CultureInfo.InvariantCulture);
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                hy = Double.Parse(line[1], CultureInfo.InvariantCulture);
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                C = new(
                    Double.Parse(line[1], CultureInfo.InvariantCulture),
                    Double.Parse(line[2], CultureInfo.InvariantCulture),
                    Double.Parse(line[3], CultureInfo.InvariantCulture)
                );
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Arquivo não pode ser lido");
            Console.WriteLine(e.Message);
        }
    }

    private static Ponto3D[] ConverterCoordMundoVista(Ponto3D[] pontos, Camera cam)
    {

        for (int i = 0; i < pontos.Length; i++)
        {
            Vetor3D vetorDir = pontos[i] - cam.C;
            pontos[i] = new(
                Vetor3D.produtoEscalar(cam.U, vetorDir),
                Vetor3D.produtoEscalar(cam.V, vetorDir),
                Vetor3D.produtoEscalar(cam.N, vetorDir)
            );
        }
        return pontos;
    }
    private readonly Ponto3D[] ProjetarPerspectiva(Ponto3D[] pontos, Camera cam)
    {
        for (int i = 0; i < pontos.Length; i++)
        {
            pontos[i] = new(
                d * (pontos[i].X / pontos[i].Z),
                d * (pontos[i].Y / pontos[i].Z),
                pontos[i].Z
            );
        }
        return pontos;
    }
    private readonly Ponto3D[] ConverterCoordNormalizadas(Ponto3D[] pontos, Camera cam)
    {
        for (int i = 0; i < pontos.Length; i++)
        {
            pontos[i] = new(
                pontos[i].X / hx,
                pontos[i].Y / hy,
                pontos[i].Z
            );
        }
        return pontos;
    }
    private static Ponto3D[] ConverterCoordNormalTela(Ponto3D[] pontos, int largura, int altura)
    {
        for (int i = 0; i < pontos.Length; i++)
        {
            pontos[i] = new(
                ((pontos[i].X + 1) / 2) * largura + 0.5,
                ((1 - pontos[i].Y) / 2) * altura + 0.5,
                pontos[i].Z
            );
        }
        return pontos;
    }

    public readonly Ponto3D[] PipelineGrafica(Malha malha, Camera cam, int LarguraTela, int AlturaTela)
    {
        Ponto3D[] pontos = (Ponto3D[])malha.Vertices.Clone();

        pontos = ConverterCoordMundoVista(pontos, cam);
        pontos = ProjetarPerspectiva(pontos, cam);
        pontos = ConverterCoordNormalizadas(pontos, cam);
        pontos = ConverterCoordNormalTela(pontos, LarguraTela, AlturaTela);

        return pontos;
    }

    private static void RasterizarSecao(
    int yInicio, int yFim,
    ref double currentX1, double taxaX1,
    ref double currentX2, double taxaX2,
    ref double currentZ1, double taxaZ1,
    ref double currentZ2, double taxaZ2,
    Pixel[] frame, double[] zBuffer, int largura, int altura)
    {
        for (int y = yInicio; y < yFim; y++)
        {
            double xEsq = currentX1 < currentX2 ? currentX1 : currentX2;
            double xDir = currentX1 < currentX2 ? currentX2 : currentX1;
            double zEsq = currentX1 < currentX2 ? currentZ1 : currentZ2;
            double zDir = currentX1 < currentX2 ? currentZ2 : currentZ1;

            double decliveHorZ = (xDir - xEsq != 0) ? (zDir - zEsq) / (xDir - xEsq) : 0;

            for (int j = (int)xEsq; j <= (int)xDir; j++)
            {
                if (j >= 0 && j < largura && y >= 0 && y < altura)
                {
                    int indice = j + largura * y;
                    double currentZ = zEsq + (j - (int)xEsq) * decliveHorZ;

                    if (currentZ < zBuffer[indice])
                    {
                        zBuffer[indice] = currentZ;
                        frame[indice] = new Pixel(255, 255, 255);
                    }
                }
            }

            currentX1 += taxaX1;
            currentX2 += taxaX2;
            currentZ1 += taxaZ1;
            currentZ2 += taxaZ2;
        }
    }

    private static void RasterizarTriangulo(Ponto3D p1, Ponto3D p2, Ponto3D p3, Pixel[] frame, double[] zBuffer, int largura, int altura)
    {
        // Ordenação vertical (p1 no topo, p3 na base)
        if (p1.Y > p2.Y) (p2, p1) = (p1, p2);
        if (p1.Y > p3.Y) (p3, p1) = (p1, p3);
        if (p2.Y > p3.Y) (p3, p2) = (p2, p3);

        // Aresta longa (p1 -> p3) mantém a mesma taxa nas duas metades
        double taxaLongaX = (p3.Y - p1.Y != 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;
        double taxaLongaZ = (p3.Y - p1.Y != 0) ? (p3.Z - p1.Z) / (p3.Y - p1.Y) : 0;

        double currentX2 = p1.X;
        double currentZ2 = p1.Z;

        // Metade Superior (p1 até p2)
        double taxaCurtaX = (p1.Y - p2.Y != 0) ? (p1.X - p2.X) / (p1.Y - p2.Y) : 0;
        double taxaCurtaZ = (p1.Y - p2.Y != 0) ? (p1.Z - p2.Z) / (p1.Y - p2.Y) : 0;
        double currentX1 = p1.X;
        double currentZ1 = p1.Z;

        RasterizarSecao(
            (int)p1.Y, (int)p2.Y,
            ref currentX1, taxaCurtaX,
            ref currentX2, taxaLongaX,
            ref currentZ1, taxaCurtaZ,
            ref currentZ2, taxaLongaZ,
            frame, zBuffer, largura, altura
        );

        // Metade Inferior (p2 até p3)
        taxaCurtaX = (p2.Y - p3.Y != 0) ? (p2.X - p3.X) / (p2.Y - p3.Y) : 0;
        taxaCurtaZ = (p2.Y - p3.Y != 0) ? (p2.Z - p3.Z) / (p2.Y - p3.Y) : 0;
        currentX1 = p2.X;
        currentZ1 = p2.Z;

        RasterizarSecao(
            (int)p2.Y, (int)p3.Y,
            ref currentX1, taxaCurtaX,
            ref currentX2, taxaLongaX,
            ref currentZ1, taxaCurtaZ,
            ref currentZ2, taxaLongaZ,
            frame, zBuffer, largura, altura
        );
    }

    private static void RasterizarMalha(Malha malha, Ponto3D[] pontos, Pixel[] frame, double[] zBuffer, int largura, int altura)
    {
        for (int i = 0; i < malha.Triangulos.Length; i++)
        {
            Triangulo tri = malha.Triangulos[i];
            RasterizarTriangulo(
                pontos[tri.V1],
                pontos[tri.V2],
                pontos[tri.V3],
                frame,
                zBuffer,
                largura,
                altura
            );
        }
    }

    public readonly Pixel[] RenderFrame(Malha malha, Camera cam, int LarguraTela, int AlturaTela)
    {
        Ponto3D[] pontos = PipelineGrafica(malha, cam, LarguraTela, AlturaTela);
        Pixel[] frame = new Pixel[LarguraTela * AlturaTela];
        double[] zBuffer = new double[LarguraTela * AlturaTela];
        Array.Fill(zBuffer, double.MaxValue);

        RasterizarMalha(malha, pontos, frame, zBuffer, LarguraTela, AlturaTela);

        return frame;
    }


}