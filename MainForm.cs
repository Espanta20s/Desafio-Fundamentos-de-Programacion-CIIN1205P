using System.Drawing;
using System.Windows.Forms;

namespace MesaPartesDigital;

public sealed class MainForm : Form
{
    private readonly TextBox _txtSolicitante = new();
    private readonly TextBox _txtDni = new();
    private readonly TextBox _txtAsunto = new();
    private readonly ComboBox _cmbTipo = new();
    private readonly DataGridView _tabla = new();
    private readonly List<Expediente> _expedientes = new();
    private int _correlativo = 1;

    public MainForm()
    {
        Text = "Mesa de Partes Digital";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 650);
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 3, ColumnCount = 1 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(panel);

        var datos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
        datos.Controls.Add(new Label { Text = "Solicitante", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        _txtSolicitante.Width = 220; datos.Controls.Add(_txtSolicitante);
        datos.Controls.Add(new Label { Text = "DNI", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        _txtDni.Width = 130; datos.Controls.Add(_txtDni);
        datos.Controls.Add(new Label { Text = "Tipo de documento", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        _cmbTipo.Items.AddRange(new object[] { "Solicitud", "Reclamo", "Petición", "Oficio", "Otro" });
        _cmbTipo.SelectedIndex = 0; datos.Controls.Add(_cmbTipo);
        datos.Controls.Add(new Label { Text = "Asunto", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        _txtAsunto.Width = 350; datos.Controls.Add(_txtAsunto);
        panel.Controls.Add(datos, 0, 0);

        var registrar = new Button { Text = "Registrar", Width = 110, Height = 32 };
        registrar.Click += (_, _) => RegistrarExpediente();
        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill }; botones.Controls.Add(registrar);
        panel.Controls.Add(botones, 0, 1);

        _tabla.Dock = DockStyle.Fill; _tabla.ReadOnly = true; _tabla.AllowUserToAddRows = false;
        _tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _tabla.Columns.Add("Codigo", "Código");
        _tabla.Columns.Add("Solicitante", "Solicitante");
        _tabla.Columns.Add("DNI", "DNI");
        _tabla.Columns.Add("Tipo", "Tipo de documento");
        _tabla.Columns.Add("Asunto", "Asunto");
        _tabla.Columns.Add("Fecha", "Fecha");
        panel.Controls.Add(_tabla, 0, 2);
    }

    private void RegistrarExpediente()
    {
        var expediente = new Expediente($"EXP-{_correlativo:0000}", _txtSolicitante.Text.Trim(), _txtDni.Text.Trim(),
            _cmbTipo.Text, _txtAsunto.Text.Trim(), DateTime.Today);
        _correlativo++;
        _expedientes.Add(expediente);
        _tabla.Rows.Add(expediente.Codigo, expediente.Solicitante, expediente.DNI, expediente.TipoDocumento, expediente.Asunto, expediente.Fecha.ToString("dd/MM/yyyy"));
        _txtSolicitante.Clear(); _txtDni.Clear(); _txtAsunto.Clear();
    }
}
