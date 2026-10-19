using System.Drawing;
using System.Windows.Forms;

namespace MesaPartesDigital;

public sealed class MainForm : Form
{
    private Expediente[] _expedientes = new Expediente[10];
    private int _cantidad;
    private readonly TextBox _txtSolicitante = new();
    private readonly TextBox _txtDni = new();
    private readonly TextBox _txtAsunto = new();
    private readonly ComboBox _cmbTipo = new();
    private readonly DateTimePicker _fecha = new();
    private readonly DataGridView _tabla = new();
    private readonly Label _lblCodigo = new();

    public MainForm()
    {
        Text = "Mesa de Partes Digital"; StartPosition = FormStartPosition.CenterScreen; Size = new Size(1000, 650);
        ConstruirInterfaz(); ActualizarCodigo();
    }

    private void ConstruirInterfaz()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 3, ColumnCount = 1 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 210)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(panel);
        var datos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
        _lblCodigo.AutoSize = true; datos.Controls.Add(new Label { Text = "Código:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }); datos.Controls.Add(_lblCodigo);
        AgregarCampo(datos, "Solicitante:", _txtSolicitante, 220); AgregarCampo(datos, "DNI:", _txtDni, 120);
        datos.Controls.Add(new Label { Text = "Tipo:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        _cmbTipo.Items.AddRange(new object[] { "Solicitud", "Reclamo", "Petición", "Oficio", "Otro" }); _cmbTipo.SelectedIndex = 0; datos.Controls.Add(_cmbTipo);
        AgregarCampo(datos, "Asunto:", _txtAsunto, 300);
        datos.Controls.Add(new Label { Text = "Fecha:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }); datos.Controls.Add(_fecha);
        panel.Controls.Add(datos, 0, 0);
        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill }; var registrar = new Button { Text = "Registrar", Width = 110 };
        registrar.Click += (_, _) => RegistrarExpediente(); botones.Controls.Add(registrar); panel.Controls.Add(botones, 0, 1);
        _tabla.Dock = DockStyle.Fill; _tabla.ReadOnly = true; _tabla.AllowUserToAddRows = false; _tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        foreach (var columna in new[] { ("Codigo", "Código"), ("Solicitante", "Solicitante"), ("DNI", "DNI"), ("Tipo", "Tipo"), ("Asunto", "Asunto"), ("Fecha", "Fecha") }) _tabla.Columns.Add(columna.Item1, columna.Item2);
        panel.Controls.Add(_tabla, 0, 2);
    }

    private static void AgregarCampo(FlowLayoutPanel panel, string etiqueta, TextBox control, int ancho)
    {
        panel.Controls.Add(new Label { Text = etiqueta, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }); control.Width = ancho; panel.Controls.Add(control);
    }

    private bool ValidarDatos()
    {
        if (string.IsNullOrWhiteSpace(_txtSolicitante.Text) || string.IsNullOrWhiteSpace(_txtAsunto.Text))
        { MessageBox.Show("Completa el solicitante y el asunto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        if (_txtDni.Text.Length != 8 || !_txtDni.Text.All(char.IsDigit))
        { MessageBox.Show("El DNI debe contener exactamente 8 dígitos numéricos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        return true;
    }

    private void RegistrarExpediente()
    {
        if (!ValidarDatos()) return;
        AsegurarCapacidad();
        var nuevo = new Expediente(GenerarCodigo(), _txtSolicitante.Text.Trim(), _txtDni.Text.Trim(), _cmbTipo.Text, _txtAsunto.Text.Trim(), _fecha.Value.Date);
        _expedientes[_cantidad++] = nuevo;
        ActualizarTabla(); ActualizarCodigo(); _txtSolicitante.Clear(); _txtDni.Clear(); _txtAsunto.Clear();
    }

    private string GenerarCodigo() => $"EXP-{_cantidad + 1:0000}";
    private void ActualizarCodigo() => _lblCodigo.Text = GenerarCodigo();
    private void AsegurarCapacidad()
    {
        if (_cantidad == _expedientes.Length) Array.Resize(ref _expedientes, _expedientes.Length * 2);
    }
    private void ActualizarTabla()
    {
        _tabla.Rows.Clear();
        for (int i = 0; i < _cantidad; i++)
        { var e = _expedientes[i]; _tabla.Rows.Add(e.Codigo, e.Solicitante, e.DNI, e.TipoDocumento, e.Asunto, e.Fecha.ToString("dd/MM/yyyy")); }
    }
}
