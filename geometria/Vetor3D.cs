public struct Vetor3D{
    public double X {get;}
    public double Y {get;}
    public double Z {get;}
    public double Magnitude {get;}
    public Vetor3D(double x, double y, double z){
        X = x;
        Y = y;
        Z = z;
        Magnitude = Math.Sqrt((X*X)+(Y*Y)+(Z*Z));
    }
    public static Vetor3D operator *(Vetor3D vetor, double escalar){
        return new Vetor3D(vetor.X * escalar, vetor.Y * escalar, vetor.Z * escalar);
    }
    public static Vetor3D produtoVetorial(Vetor3D a, Vetor3D b){
        return new Vetor3D(a.Y*b.Z - a.Z*b.Y, a.Z*b.X - a.X*b.Z, a.X*b.Y - a.Y*b.X);
    }
    public static double produtoEscalar(Vetor3D a, Vetor3D b){
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    public Vetor3D normalizar(){
        return new(X/Magnitude, Y/Magnitude, Z/Magnitude);
    }

}
