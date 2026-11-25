public struct Triangulo{
    public int V1 {get;}
    public int V2 {get;}
    public int V3 {get;}
    public Triangulo(int v1, int v2, int v3){
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }
    
    public readonly Vetor3D CalcularNormal(Ponto3D[] vertices){
        Vetor3D aresta1 = vertices[V2] - vertices[V1];
        Vetor3D aresta2 = vertices[V3] - vertices[V1];
        return Vetor3D.produtoVetorial(aresta1, aresta2);
    }
}