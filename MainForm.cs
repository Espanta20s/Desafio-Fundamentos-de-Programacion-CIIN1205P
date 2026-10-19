using System.Drawing;
using System.Windows.Forms;

namespace MesaPartesDigital;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "Mesa de Partes Digital";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 800;
        Height = 500;
        Controls.Add(new Label
        {
            Text = "Mesa de Partes Digital\nPrototipo inicial",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18, FontStyle.Bold)
        });
    }
}
