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
        int yInicio, int yFim,ref VerticeScanline current1, VerticeScanline taxa1,
        ref VerticeScanline current2, VerticeScanline taxa2,Camera cam, Iluminacao luz,
        Pixel[] frame, double[] zBuffer, int largura, int altura
    )
    {
        for (int y = yInicio; y < yFim; y++)
        {
            VerticeScanline vEsq = current1.Tela.X < current2.Tela.X ? current1 : current2;
            VerticeScanline vDir = current1.Tela.X < current2.Tela.X ? current2 : current1;

            double dx = vDir.Tela.X - vEsq.Tela.X;
            VerticeScanline decliveHor = (dx != 0) ? (vDir - vEsq) * (1.0 / dx) : default;
            VerticeScanline atual = vEsq;

            for (int j = (int)vEsq.Tela.X; j <= (int)vDir.Tela.X; j++)
            {
                if (j >= 0 && j < largura && y >= 0 && y < altura)
                {
                    int indice = j + largura * y;

                    if (atual.Tela.Z < zBuffer[indice])
                    {
                        zBuffer[indice] = atual.Tela.Z;
                        frame[indice] = luz.CalcularPhong(atual.Mundo, atual.Normal, cam.C);
                    }
                }

                atual += decliveHor;
            }

            current1 += taxa1;
            current2 += taxa2;
        }
    }

    private static void RasterizarTriangulo(
        VerticeScanline v1, VerticeScanline v2, VerticeScanline v3,
        Camera cam, Iluminacao luz, Pixel[] frame, double[] zBuffer, int largura, int altura)
    {
        // Ordenação vertical (v1 no topo, v3 na base)
        if (v1.Tela.Y > v2.Tela.Y) (v2, v1) = (v1, v2);
        if (v1.Tela.Y > v3.Tela.Y) (v3, v1) = (v1, v3);
        if (v2.Tela.Y > v3.Tela.Y) (v3, v2) = (v2, v3);

        double dyLongo = v3.Tela.Y - v1.Tela.Y;
        VerticeScanline taxaLonga = (dyLongo != 0) ? (v3 - v1) * (1.0 / dyLongo) : default;
        VerticeScanline current2 = v1;

        // Metade Superior (v1 até v2)
        double dyCurtoSup = v2.Tela.Y - v1.Tela.Y;
        VerticeScanline taxaCurta = (dyCurtoSup != 0) ? (v2 - v1) * (1.0 / dyCurtoSup) : default;
        VerticeScanline current1 = v1;

        RasterizarSecao(
            (int)v1.Tela.Y, (int)v2.Tela.Y,
            ref current1, taxaCurta,
            ref current2, taxaLonga,
            cam, luz, frame, zBuffer, largura, altura
        );

        // Metade Inferior (v2 até v3)
        double dyCurtoInf = v3.Tela.Y - v2.Tela.Y;
        taxaCurta = (dyCurtoInf != 0) ? (v3 - v2) * (1.0 / dyCurtoInf) : default;
        current1 = v2;

        RasterizarSecao(
            (int)v2.Tela.Y, (int)v3.Tela.Y,
            ref current1, taxaCurta,
            ref current2, taxaLonga,
            cam, luz, frame, zBuffer, largura, altura
        );
    }

    private static void RasterizarMalha(
        Malha malha, Ponto3D[] pontos, Camera cam, Iluminacao luz,
        Pixel[] frame, double[] zBuffer, int largura, int altura)
    {
        for (int i = 0; i < malha.Triangulos.Length; i++)
        {
            Triangulo tri = malha.Triangulos[i];

            var v1 = new VerticeScanline(pontos[tri.V1], malha.Vertices[tri.V1], malha.normaisVertices[tri.V1]);
            var v2 = new VerticeScanline(pontos[tri.V2], malha.Vertices[tri.V2], malha.normaisVertices[tri.V2]);
            var v3 = new VerticeScanline(pontos[tri.V3], malha.Vertices[tri.V3], malha.normaisVertices[tri.V3]);

            RasterizarTriangulo(v1, v2, v3, cam, luz, frame, zBuffer, largura, altura);
        }
    }

    public readonly Pixel[] RenderFrame(Malha malha, Camera cam, Iluminacao luz, int LarguraTela, int AlturaTela)
    {
        Ponto3D[] pontos = PipelineGrafica(malha, cam, LarguraTela, AlturaTela);
        Pixel[] frame = new Pixel[LarguraTela * AlturaTela];
        double[] zBuffer = new double[LarguraTela * AlturaTela];
        Array.Fill(zBuffer, double.MaxValue);

        RasterizarMalha(malha, pontos, cam, luz, frame, zBuffer, LarguraTela, AlturaTela);

        return frame;
    }


}