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
            using (StreamReader sr = new StreamReader(path))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] parts = line.Split(new char[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    string chave = parts[0].ToLower();

                    switch (chave)
                    {
                        case "iamb":
                            Iamb = new CorRGB(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)
                            );
                            break;

                        case "ka":
                            Ka = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            break;

                        case "il":
                            Il = new CorRGB(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)
                            );
                            break;

                        case "pl":
                            Pl = new Ponto3D(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)
                            );
                            break;

                        case "kd":
                            Kd = new Vetor3D(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)
                            );
                            break;

                        case "od":
                            Od = new Vetor3D(
                                double.Parse(parts[1], CultureInfo.InvariantCulture),
                                double.Parse(parts[2], CultureInfo.InvariantCulture),
                                double.Parse(parts[3], CultureInfo.InvariantCulture)
                            );
                            break;

                        case "ks":
                            Ks = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            break;

                        case "eta":
                        case "η":
                            Eta = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro ao ler o arquivo de iluminação: " + e.Message);
        }
    }
}