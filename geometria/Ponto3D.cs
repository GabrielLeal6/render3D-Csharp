public struct Ponto3D{
    public double X {get;}
    public double Y {get;}
    public double Z {get;}
    public Ponto3D(double x, double y, double z){
        X = x;
        Y = y;
        Z = z;
    }
    public static Vetor3D operator -(Ponto3D a, Ponto3D b){
        return new Vetor3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    public static Ponto3D operator +(Ponto3D ponto, Vetor3D vetor){
        return new Ponto3D(ponto.X + vetor.X, ponto.Y + vetor.Y, ponto.Z + vetor.Z);
    }
    public static Ponto3D operator +(Vetor3D vetor, Ponto3D ponto){
        return new Ponto3D(ponto.X + vetor.X, ponto.Y + vetor.Y, ponto.Z + vetor.Z);
    }
}
