using System.Globalization;

public struct Camera{

    public Ponto3D C;
    public Vetor3D N, V, U;
    public double d, hx, hy;

    public Camera(string path){
        load(path);
    }

    public void load(string path){
        try{
            using (StreamReader sr = new(path)){
                string[] line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                N = new (
                    Double.Parse(line[1], CultureInfo.InvariantCulture), 
                    Double.Parse(line[2], CultureInfo.InvariantCulture), 
                    Double.Parse(line[3], CultureInfo.InvariantCulture)
                );
                line = sr.ReadLine().Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                V = new (
                    Double.Parse(line[1], CultureInfo.InvariantCulture), 
                    Double.Parse(line[2], CultureInfo.InvariantCulture), 
                    Double.Parse(line[3], CultureInfo.InvariantCulture)
                );
                U = Vetor3D.produtoVetorial(N,V - N*(Vetor3D.produtoEscalar(V,N)/Vetor3D.produtoEscalar(N,N)));
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
        } catch (Exception e){
            Console.WriteLine("Arquivo não pode ser lido");
            Console.WriteLine(e.Message);
        }
    }

    private Ponto3D[] ConverterCoordMundoVista(Ponto3D[] pontos, Camera cam){
        
        for (int i = 0; i < pontos.Length; i++){
            Vetor3D vetorDir = pontos[i]  - cam.C; 
            pontos[i] = new(
                Vetor3D.produtoEscalar(cam.U, vetorDir),
                Vetor3D.produtoEscalar(cam.V, vetorDir),
                Vetor3D.produtoEscalar(cam.N, vetorDir)
            );
        }
        return pontos;
    }
    private Ponto3D[] ProjetarPerspectiva(Ponto3D[] pontos, Camera cam){
        for (int i = 0; i < pontos.Length; i++){
            pontos[i] = new(
                d*(pontos[i].X/pontos[i].Z),
                d*(pontos[i].Y/pontos[i].Z),
                pontos[i].Z
            );
        }
        return pontos;
    }
    private Ponto3D[] ConverterCoordNormalizadas(Ponto3D[] pontos, Camera cam){
        for (int i = 0; i < pontos.Length; i++){
            pontos[i] = new(
                pontos[i].X/hx,
                pontos[i].Y/hy,
                pontos[i].Z
            );
        }
        return pontos;
    }
    private Ponto3D[] ConverterCoordNormalTela(Ponto3D[] pontos, int largura, int altura){
        for (int i = 0; i < pontos.Length; i++){
            pontos[i] = new(
                ((pontos[i].X + 1)/2)*largura + 0.5,
                ((1 - pontos[i].Y)/2)*altura + 0.5,
                pontos[i].Z
            );
        }
        return pontos;
    }

    public Ponto3D[] PipelineGrafica(Malha malha, Camera cam, int LarguraTela, int AlturaTela){
        Ponto3D[] pontos = (Ponto3D[])malha.vertices.Clone();

        pontos = ConverterCoordMundoVista(pontos, cam);
        pontos = ProjetarPerspectiva(pontos, cam);
        pontos = ConverterCoordNormalizadas(pontos, cam);
        pontos = ConverterCoordNormalTela(pontos, LarguraTela, AlturaTela);

        return pontos;
    }

    public Pixel[] RenderFrame(Malha malha, Camera cam, int LarguraTela, int AlturaTela)
{
    Ponto3D[] pontos = PipelineGrafica(malha, cam, LarguraTela, AlturaTela);
    Pixel[] frame = new Pixel[LarguraTela * AlturaTela];
    
    double[] zBuffer = new double[LarguraTela * AlturaTela];
    for (int i = 0; i < zBuffer.Length; i++) {
        zBuffer[i] = double.MaxValue;
    }

    for (int i = 0; i < malha.triangulos.Length; i++)
    {
        Triangulo triOrd = malha.triangulos[i];

        Ponto3D p1 = pontos[triOrd.V1];
        Ponto3D p2 = pontos[triOrd.V2];
        Ponto3D p3 = pontos[triOrd.V3];

        // Ordenação vertical (p1 no topo, p3 na base)
        if (p1.Y > p2.Y) { (p2, p1) = (p1, p2); }
        if (p1.Y > p3.Y) { (p3, p1) = (p1, p3); }
        if (p2.Y > p3.Y) { (p3, p2) = (p2, p3); }

        // Taxas de variação vertical para X e Z
        double taxaCurta  = (p1.Y - p2.Y != 0) ? (p1.X - p2.X) / (p1.Y - p2.Y) : 0;
        double taxaLonga  = (p3.Y - p1.Y != 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

        double taxaCurtaZ = (p1.Y - p2.Y != 0) ? (p1.Z - p2.Z) / (p1.Y - p2.Y) : 0;
        double taxaLongaZ = (p3.Y - p1.Y != 0) ? (p3.Z - p1.Z) / (p3.Y - p1.Y) : 0;

        double currentX1 = p1.X; // Aresta curta
        double currentX2 = p1.X; // Aresta longa

        double currentZ1 = p1.Z;
        double currentZ2 = p1.Z;

        // Metade Superior (p1 até p2)
        for (int y = (int)p1.Y; y < (int)p2.Y; y++){
            double xEsq = currentX1 < currentX2 ? currentX1 : currentX2;
            double xDir = currentX1 < currentX2 ? currentX2 : currentX1;
            double zEsq = currentX1 < currentX2 ? currentZ1 : currentZ2;
            double zDir = currentX1 < currentX2 ? currentZ2 : currentZ1;

            // Declive horizontal de Z
            double decliveHorZ = (xDir - xEsq != 0) ? (zDir - zEsq) / (xDir - xEsq) : 0;

            for (int j = (int)xEsq; j <= (int)xDir; j++)
            {
                if (j >= 0 && j < LarguraTela && y >= 0 && y < AlturaTela)
                {
                    int indice = j + LarguraTela * y;
                    double currentZ = zEsq + (j - (int)xEsq) * decliveHorZ;

                    // Teste de profundidade Z-Buffer
                    if (currentZ < zBuffer[indice])
                    {
                        zBuffer[indice] = currentZ;
                        frame[indice] = new Pixel(255, 255, 255);
                    }
                }
            }

            currentX1 += taxaCurta;
            currentX2 += taxaLonga;
            currentZ1 += taxaCurtaZ;
            currentZ2 += taxaLongaZ;
        }

        // Metade Inferior (p2 até p3)
        taxaCurta  = (p2.Y - p3.Y != 0) ? (p2.X - p3.X) / (p2.Y - p3.Y) : 0;
        taxaCurtaZ = (p2.Y - p3.Y != 0) ? (p2.Z - p3.Z) / (p2.Y - p3.Y) : 0;
        
        currentX1 = p2.X;
        currentZ1 = p2.Z;

        for (int y = (int)p2.Y; y < (int)p3.Y; y++){
            double xEsq = currentX1 < currentX2 ? currentX1 : currentX2;
            double xDir = currentX1 < currentX2 ? currentX2 : currentX1;
            double zEsq = currentX1 < currentX2 ? currentZ1 : currentZ2;
            double zDir = currentX1 < currentX2 ? currentZ2 : currentZ1;

            double decliveHorZ = (xDir - xEsq != 0) ? (zDir - zEsq) / (xDir - xEsq) : 0;

            for (int j = (int)xEsq; j <= (int)xDir; j++){
                if (j >= 0 && j < LarguraTela && y >= 0 && y < AlturaTela)
                {
                    int indice = j + LarguraTela * y;
                    double currentZ = zEsq + (j - (int)xEsq) * decliveHorZ;

                    // Teste de profundidade Z-Buffer
                    if (currentZ < zBuffer[indice])
                    {
                        zBuffer[indice] = currentZ;
                        frame[indice] = new Pixel(255, 255, 255);
                    }
                }
            }

            currentX1 += taxaCurta;
            currentX2 += taxaLonga;
            currentZ1 += taxaCurtaZ;
            currentZ2 += taxaLongaZ;
        }
    }

    return frame;
}

}