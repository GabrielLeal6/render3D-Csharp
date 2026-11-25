using System.Globalization;

public class Iluminacao
{
    public CorRGB Iamb { get; private set; }
    public double Ka { get; private set; }
    public CorRGB Il { get; private set; }
    public Ponto3D Pl { get; private set; }
    public Vetor3D Kd { get; private set; }
    public Vetor3D Od { get; private set; }
    public double Ks { get; private set; }
    public double Eta { get; private set; }

    public Iluminacao(string path)
    {
        Load(path);
    }

    public void Load(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                bool temRotulo = char.IsLetter(parts[0][0]);
                string chave = temRotulo ? parts[0].ToLower() : i.ToString();
                int offset = temRotulo ? 1 : 0;

                switch (chave)
                {
                    case "iamb":
                    case "0":
                        Iamb = new CorRGB(
                            double.Parse(parts[offset], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 1], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 2], CultureInfo.InvariantCulture)
                        );
                        break;

                    case "ka":
                    case "1":
                        Ka = double.Parse(parts[offset], CultureInfo.InvariantCulture);
                        break;

                    case "il":
                    case "2":
                        Il = new CorRGB(
                            double.Parse(parts[offset], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 1], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 2], CultureInfo.InvariantCulture)
                        );
                        break;

                    case "pl":
                    case "3":
                        Pl = new Ponto3D(
                            double.Parse(parts[offset], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 1], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 2], CultureInfo.InvariantCulture)
                        );
                        break;

                    case "kd":
                    case "4":
                        Kd = new Vetor3D(
                            double.Parse(parts[offset], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 1], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 2], CultureInfo.InvariantCulture)
                        );
                        break;

                    case "od":
                    case "5":
                        Od = new Vetor3D(
                            double.Parse(parts[offset], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 1], CultureInfo.InvariantCulture),
                            double.Parse(parts[offset + 2], CultureInfo.InvariantCulture)
                        );
                        break;

                    case "ks":
                    case "6":
                        Ks = double.Parse(parts[offset], CultureInfo.InvariantCulture);
                        break;

                    case "eta":
                    case "η":
                    case "7":
                        Eta = double.Parse(parts[offset], CultureInfo.InvariantCulture);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro ao ler o arquivo de iluminação: " + e.Message);
        }
    }
    public Pixel CalcularPhong(Ponto3D pontoMundo, Vetor3D normal, Ponto3D posCamera)
    {
        Vetor3D N = normal.Normalizar();
        Vetor3D L = (Pl - pontoMundo).Normalizar();
        Vetor3D V = (posCamera - pontoMundo).Normalizar();

        double nDotL = Vetor3D.produtoEscalar(N, L);

        // Componente Ambiente
        double r = Iamb.R * Ka;
        double g = Iamb.G * Ka;
        double b = Iamb.B * Ka;

        if (nDotL > 0)
        {
            // Componente Difusa
            r += Il.R * Kd.X * Od.X * nDotL;
            g += Il.G * Kd.Y * Od.Y * nDotL;
            b += Il.B * Kd.Z * Od.Z * nDotL;

            // Componente Especular: R = 2 * (N . L) * N - L
            Vetor3D R = ((N * (2.0 * nDotL)) - L).Normalizar();
            double rDotV = Vetor3D.produtoEscalar(R, V);

            if (rDotV > 0)
            {
                double fatorEspecular = Ks * Math.Pow(rDotV, Eta);
                r += Il.R * fatorEspecular;
                g += Il.G * fatorEspecular;
                b += Il.B * fatorEspecular;
            }
        }

        return new Pixel(
            (byte)Math.Clamp(r, 0, 255),
            (byte)Math.Clamp(g, 0, 255),
            (byte)Math.Clamp(b, 0, 255)
        );
    }
}