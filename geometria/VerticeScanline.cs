public struct VerticeScanline
{
    public Ponto3D Tela { get; }
    public Ponto3D Mundo { get; }
    public Vetor3D Normal { get; }

    public VerticeScanline(Ponto3D tela, Ponto3D mundo, Vetor3D normal)
    {
        Tela = tela;
        Mundo = mundo;
        Normal = normal;
    }

    public static VerticeScanline operator +(VerticeScanline a, VerticeScanline b)
    {
        return new VerticeScanline(
            new Ponto3D(a.Tela.X + b.Tela.X, a.Tela.Y + b.Tela.Y, a.Tela.Z + b.Tela.Z),
            new Ponto3D(a.Mundo.X + b.Mundo.X, a.Mundo.Y + b.Mundo.Y, a.Mundo.Z + b.Mundo.Z),
            a.Normal + b.Normal
        );
    }

    public static VerticeScanline operator -(VerticeScanline a, VerticeScanline b)
    {
        return new VerticeScanline(
            new Ponto3D(a.Tela.X - b.Tela.X, a.Tela.Y - b.Tela.Y, a.Tela.Z - b.Tela.Z),
            new Ponto3D(a.Mundo.X - b.Mundo.X, a.Mundo.Y - b.Mundo.Y, a.Mundo.Z - b.Mundo.Z),
            a.Normal - b.Normal
        );
    }

    public static VerticeScanline operator *(VerticeScanline v, double escalar)
    {
        return new VerticeScanline(
            new Ponto3D(v.Tela.X * escalar, v.Tela.Y * escalar, v.Tela.Z * escalar),
            new Ponto3D(v.Mundo.X * escalar, v.Mundo.Y * escalar, v.Mundo.Z * escalar),
            v.Normal * escalar
        );
    }
}