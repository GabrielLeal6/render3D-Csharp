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
        Ponto3D[] pontos = malha.vertices;

        pontos = ConverterCoordMundoVista(pontos, cam);
        pontos = ProjetarPerspectiva(pontos, cam);
        pontos = ConverterCoordNormalizadas(pontos, cam);
        pontos = ConverterCoordNormalTela(pontos, LarguraTela, AlturaTela);

        return pontos;
    }

    public Pixel[] RenderFrame(Malha malha, Camera cam, int LarguraTela, int AlturaTela){
        Ponto3D[] pontos = PipelineGrafica(malha, cam, LarguraTela, AlturaTela);
        Pixel[] frame = new Pixel[LarguraTela * AlturaTela];

        for (int i = 0; i < malha.triangulos.Length; i++){
            Triangulo triOrd = malha.triangulos[i];
            double v1y = pontos[triOrd.V1].Y;
            double v2y = pontos[triOrd.V2].Y;
            double v3y = pontos[triOrd.V3].Y;

            Ponto3D p1 = pontos[triOrd.V1];
            Ponto3D p2 = pontos[triOrd.V2];
            Ponto3D p3 = pontos[triOrd.V3];

            if (v1y > v2y){(p2, p1) = (p1, p2);}
            if (v1y > v3y){(p3, p1) = (p1, p3);}
            if (v2y > v3y){(p3, p2) = (p2, p3);}

            

        }
        
        

        return frame;
    }

}