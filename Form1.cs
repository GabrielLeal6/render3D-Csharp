namespace Rasterizador3D;

public partial class Form1 : Form
{
    int LarguraTela = 800;
    int AlturaTela = 600;
    PictureBox picturebox;
    Camera camera;
    Malha exemplo;
    Bitmap canvas;
    String objetoPath;

    public Form1()
    {
        InitializeComponent();

        this.ClientSize = new Size(LarguraTela, AlturaTela);
        this.Text = "Rasterizador 3D - 1ª VA";

        picturebox = new PictureBox();
        picturebox.Dock = DockStyle.Fill;
        this.Controls.Add(picturebox);

        objetoPath = "objetos/calice2.byu";
        canvas = new Bitmap(LarguraTela, AlturaTela);
        exemplo = new Malha(objetoPath);
        camera = new Camera("camera.txt");

        this.KeyDown += AtualizarQuadro;

        Renderizar();
    }

    private void Renderizar()
    {
        Pixel[] frame = camera.RenderFrame(
            exemplo,
            camera,
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
        camera.load("camera.txt");
        exemplo = new Malha(objetoPath);

        Renderizar();
    }
}