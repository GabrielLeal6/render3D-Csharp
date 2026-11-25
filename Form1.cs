namespace Rasterizador3D;

public partial class Form1 : Form
{
    int LarguraTela = 800;
    int AlturaTela = 600;
    PictureBox picturebox;
    Camera camera;
    Malha exemplo;
    Iluminacao iluminacao;
    Bitmap canvas;
    string objetoPath;

    private string[] arquivosObjetos;
    private int indiceObjetoAtual = 0;

    public Form1()
    {
        InitializeComponent();

        this.ClientSize = new Size(LarguraTela, AlturaTela);

        picturebox = new PictureBox();
        picturebox.Dock = DockStyle.Fill;
        this.Controls.Add(picturebox);

        CarregarListaObjetos();

        objetoPath = arquivosObjetos[indiceObjetoAtual];
        this.Text = $"Rasterizador 3D - 2ª VA | {Path.GetFileName(objetoPath)}";

        canvas = new Bitmap(LarguraTela, AlturaTela);
        exemplo = new Malha(objetoPath);
        camera = new Camera("camera.txt");
        iluminacao = new Iluminacao("iluminacao.txt");

        this.KeyDown += AtualizarQuadro;

        Renderizar();
    }

    private void CarregarListaObjetos()
    {
        if (Directory.Exists("objetos"))
        {
            var encontrados = Directory
                .GetFiles("objetos")
                .Where(f => f.EndsWith(".byu", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToArray();

            if (encontrados.Length > 0)
            {
                arquivosObjetos = encontrados;

                int idxInicial = Array.FindIndex(
                    arquivosObjetos,
                    f => Path.GetFileName(f).Equals("calice2.byu", StringComparison.OrdinalIgnoreCase)
                );

                indiceObjetoAtual = idxInicial >= 0 ? idxInicial : 0;
                return;
            }
        }

        // Fallback
        arquivosObjetos = new string[]
        {
            "objetos/calice2.byu",
            "objetos/vaso.byu",
            "objetos/maca.byu",
            "objetos/piramide.byu"
        };
        indiceObjetoAtual = 0;
    }

    private void Renderizar()
    {
        Pixel[] frame = camera.RenderFrame(
            exemplo,
            camera,
            iluminacao,
            LarguraTela,
            AlturaTela
        );

        for (int i = 0; i < AlturaTela; i++)
        {
            for (int j = 0; j < LarguraTela; j++)
            {
                Pixel? currentPixel = frame[j + (LarguraTela * i)];

                if (currentPixel is Pixel pixel)
                {
                    canvas.SetPixel(
                        j,
                        i,
                        Color.FromArgb(pixel.R, pixel.G, pixel.B)
                    );
                }
                else
                {
                    canvas.SetPixel(
                        j, i, Color.FromArgb(0, 0, 0)
                    );
                }
            }
        }

        picturebox.Image = canvas;
    }

    private void AtualizarQuadro(object sender, KeyEventArgs e)
    {
        camera.Load("camera.txt");
        iluminacao.Load("iluminacao.txt");
        exemplo = new Malha(objetoPath);

        Renderizar();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (arquivosObjetos != null && arquivosObjetos.Length > 0)
        {
            if (keyData == Keys.Right)
            {
                indiceObjetoAtual = (indiceObjetoAtual + 1) % arquivosObjetos.Length;
                TrocarObjetoAtual();
                return true;
            }
            else if (keyData == Keys.Left)
            {
                indiceObjetoAtual = (indiceObjetoAtual - 1 + arquivosObjetos.Length) % arquivosObjetos.Length;
                TrocarObjetoAtual();
                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void TrocarObjetoAtual()
    {
        objetoPath = arquivosObjetos[indiceObjetoAtual];
        exemplo = new Malha(objetoPath);
        Renderizar();

        this.Text = $"Rasterizador 3D - 2ª VA | {Path.GetFileName(objetoPath)}";
    }
}