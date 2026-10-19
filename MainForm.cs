using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MesaPartesDigital;

public sealed class MainForm : Form
{
    private Expediente[] _expedientes = new Expediente[10];
    private int _cantidad;
    private readonly string _rutaArchivo = Path.Combine(AppContext.BaseDirectory, "expedientes.txt");

    private readonly TextBox _txtCodigo = new();
    private readonly TextBox _txtSolicitante = new();
    private readonly TextBox _txtDni = new();
    private readonly ComboBox _cmbTipoDocumento = new();
    private readonly TextBox _txtAsunto = new();
    private readonly DateTimePicker _dtpFecha = new();
    private readonly TextBox _txtBuscar = new();
    private readonly DataGridView _tabla = new();
    private readonly Label _lblEstado = new();

    public MainForm()
    {
        Text = "Mesa de Partes Digital";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(960, 680);
        Size = new Size(1180, 800);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        BackColor = Color.FromArgb(245, 247, 250);

        ConstruirInterfaz();
        Load += (_, _) =>
        {
            CargarArchivo();
            ActualizarCodigoVisual();
            ActualizarTabla();
            MostrarEstado($"{_cantidad} expediente(s) cargado(s). Archivo: {Path.GetFileName(_rutaArchivo)}");
        };
        FormClosing += (_, _) =>
        {
            try { GuardarArchivo(); }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo guardar el archivo al cerrar.\n{ex.Message}",
                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
    }

    private void ConstruirInterfaz()
    {
        var principal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(18),
            BackColor = BackColor
        };
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        Controls.Add(principal);

        var encabezado = new Panel { Dock = DockStyle.Fill };
        var titulo = new Label
        {
            Text = "MESA DE PARTES DIGITAL",
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 53, 77),
            AutoSize = true,
            Location = new Point(0, 2)
        };
        var subtitulo = new Label
        {
            Text = "Registro, consulta y organización de expedientes",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(85, 103, 120),
            AutoSize = true,
            Location = new Point(2, 39)
        };
        encabezado.Controls.Add(titulo);
        encabezado.Controls.Add(subtitulo);
        principal.Controls.Add(encabezado, 0, 0);

        var grupoDatos = new GroupBox
        {
            Text = "Datos del expediente",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ForeColor = Color.FromArgb(24, 53, 77),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
        };
        var campos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(4, 10, 4, 4)
        };
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 4; i++) campos.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        PrepararEntrada(_txtCodigo, soloLectura: true);
        PrepararEntrada(_txtSolicitante);
        PrepararEntrada(_txtDni);
        PrepararEntrada(_txtAsunto);
        _cmbTipoDocumento.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbTipoDocumento.Items.AddRange(new object[] { "Solicitud", "Reclamo", "Petición", "Oficio", "Otro" });
        _cmbTipoDocumento.SelectedIndex = 0;
        _cmbTipoDocumento.Dock = DockStyle.Fill;
        _cmbTipoDocumento.Margin = new Padding(3, 5, 12, 5);
        _dtpFecha.Format = DateTimePickerFormat.Custom;
        _dtpFecha.CustomFormat = "dd/MM/yyyy";
        _dtpFecha.Dock = DockStyle.Fill;
        _dtpFecha.Margin = new Padding(3, 5, 12, 5);

        AgregarCampo(campos, "Código (auto.)", _txtCodigo, 0, 0);
        AgregarCampo(campos, "Solicitante", _txtSolicitante, 0, 1);
        AgregarCampo(campos, "DNI (8 dígitos)", _txtDni, 1, 0);
        AgregarCampo(campos, "Tipo de documento", _cmbTipoDocumento, 1, 1);
        AgregarCampo(campos, "Fecha", _dtpFecha, 2, 0);
        AgregarCampo(campos, "Asunto", _txtAsunto, 3, 0, 3);

        grupoDatos.Controls.Add(campos);
        principal.Controls.Add(grupoDatos, 0, 1);

        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 9, 0, 0)
        };
        acciones.Controls.Add(CrearBoton("Registrar", Color.FromArgb(23, 112, 105), (_, _) => RegistrarExpediente()));
        acciones.Controls.Add(CrearBoton("Limpiar", Color.FromArgb(95, 112, 128), (_, _) => LimpiarCampos()));
        acciones.Controls.Add(CrearBoton("Guardar", Color.FromArgb(43, 93, 140), (_, _) => GuardarDesdeBoton()));
        acciones.Controls.Add(CrearBoton("Ordenar por código", Color.FromArgb(43, 93, 140), (_, _) => OrdenarExpedientes()));
        acciones.Controls.Add(CrearBoton("Mostrar todos", Color.FromArgb(95, 112, 128), (_, _) => MostrarTodos()));
        var lblBuscar = new Label
        {
            Text = "Buscar código:", AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 9, 0, 0), ForeColor = Color.FromArgb(24, 53, 77)
        };
        acciones.Controls.Add(lblBuscar);
        _txtBuscar.Width = 125;
        _txtBuscar.Margin = new Padding(3, 5, 5, 3);
        acciones.Controls.Add(_txtBuscar);
        acciones.Controls.Add(CrearBoton("Buscar", Color.FromArgb(23, 112, 105), (_, _) => BuscarExpediente()));
        principal.Controls.Add(acciones, 0, 2);

        _tabla.Dock = DockStyle.Fill;
        _tabla.BackgroundColor = Color.White;
        _tabla.BorderStyle = BorderStyle.FixedSingle;
        _tabla.AllowUserToAddRows = false;
        _tabla.AllowUserToDeleteRows = false;
        _tabla.ReadOnly = true;
        _tabla.MultiSelect = false;
        _tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _tabla.RowHeadersVisible = false;
        _tabla.EnableHeadersVisualStyles = false;
        _tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 53, 77);
        _tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        _tabla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 235, 232);
        _tabla.DefaultCellStyle.SelectionForeColor = Color.FromArgb(24, 53, 77);
        _tabla.Columns.Add("Codigo", "Código");
        _tabla.Columns.Add("Solicitante", "Solicitante");
        _tabla.Columns.Add("DNI", "DNI");
        _tabla.Columns.Add("TipoDocumento", "Tipo de documento");
        _tabla.Columns.Add("Asunto", "Asunto");
        _tabla.Columns.Add("Fecha", "Fecha");
        _tabla.Columns[0].FillWeight = 70;
        _tabla.Columns[1].FillWeight = 130;
        _tabla.Columns[2].FillWeight = 70;
        _tabla.Columns[3].FillWeight = 100;
        _tabla.Columns[4].FillWeight = 160;
        _tabla.Columns[5].FillWeight = 75;
        principal.Controls.Add(_tabla, 0, 3);

        _lblEstado.Dock = DockStyle.Fill;
        _lblEstado.TextAlign = ContentAlignment.MiddleLeft;
        _lblEstado.ForeColor = Color.FromArgb(85, 103, 120);
        principal.Controls.Add(_lblEstado, 0, 4);
    }

    private static void PrepararEntrada(TextBox control, bool soloLectura = false)
    {
        control.Dock = DockStyle.Fill;
        control.ReadOnly = soloLectura;
        control.Margin = new Padding(3, 5, 12, 5);
        if (soloLectura) control.BackColor = Color.FromArgb(235, 240, 244);
    }

    private static void AgregarCampo(TableLayoutPanel tabla, string texto, Control control,
        int fila, int pareja, int span = 1)
    {
        int columnaEtiqueta = pareja == 0 ? 0 : 2;
        int columnaControl = pareja == 0 ? 1 : 3;
        var etiqueta = new Label
        {
            Text = texto,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(61, 78, 94),
            Margin = new Padding(3, 3, 3, 3)
        };
        tabla.Controls.Add(etiqueta, columnaEtiqueta, fila);
        tabla.Controls.Add(control, columnaControl, fila);
        if (span > 1) tabla.SetColumnSpan(control, span);
    }

    private static Button CrearBoton(string texto, Color color, EventHandler evento)
    {
        var boton = new Button
        {
            Text = texto,
            Width = texto == "Ordenar por código" ? 142 : 95,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            Margin = new Padding(0, 0, 7, 3),
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.Click += evento;
        return boton;
    }

    private bool ValidarDatos()
    {
        if (string.IsNullOrWhiteSpace(_txtSolicitante.Text))
        {
            MostrarError("Ingrese el nombre del solicitante.");
            _txtSolicitante.Focus();
            return false;
        }

        string dni = _txtDni.Text.Trim();
        if (dni.Length != 8 || !dni.All(char.IsDigit))
        {
            MostrarError("El DNI debe contener exactamente 8 dígitos numéricos.");
            _txtDni.Focus();
            return false;
        }

        if (_cmbTipoDocumento.SelectedIndex < 0)
        {
            MostrarError("Seleccione un tipo de documento.");
            _cmbTipoDocumento.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_txtAsunto.Text))
        {
            MostrarError("Ingrese el asunto del expediente.");
            _txtAsunto.Focus();
            return false;
        }

        return true;
    }

    private void RegistrarExpediente()
    {
        if (!ValidarDatos()) return;

        string codigo = GenerarCodigoSiguiente();
        var nuevo = new Expediente(
            codigo,
            LimpiarSeparador(_txtSolicitante.Text.Trim()),
            _txtDni.Text.Trim(),
            _cmbTipoDocumento.SelectedItem?.ToString() ?? "Otro",
            LimpiarSeparador(_txtAsunto.Text.Trim()),
            _dtpFecha.Value.Date);

        AsegurarCapacidad();
        _expedientes[_cantidad] = nuevo;
        _cantidad++;
        ActualizarCodigoVisual();
        ActualizarTabla();
        LimpiarCampos(mantenerEstado: true);

        try
        {
            GuardarArchivo();
            MostrarEstado($"Expediente {codigo} registrado y guardado correctamente.");
        }
        catch (Exception ex)
        {
            MostrarError($"El expediente se registró en memoria, pero no pudo guardarse en el archivo. {ex.Message}");
        }
    }

    private string GenerarCodigoSiguiente()
    {
        int mayor = 0;
        for (int i = 0; i < _cantidad; i++)
        {
            string codigo = _expedientes[i].Codigo;
            string numeros = new string(codigo.Where(char.IsDigit).ToArray());
            if (int.TryParse(numeros, out int valor) && valor > mayor) mayor = valor;
        }
        return $"EXP-{mayor + 1:D4}";
    }

    private void AsegurarCapacidad()
    {
        if (_cantidad < _expedientes.Length) return;
        Array.Resize(ref _expedientes, _expedientes.Length == 0 ? 10 : _expedientes.Length * 2);
    }

    private void BuscarExpediente()
    {
        string codigoBuscado = _txtBuscar.Text.Trim();
        if (string.IsNullOrWhiteSpace(codigoBuscado))
        {
            MostrarError("Ingrese el código que desea buscar, por ejemplo EXP-0001.");
            _txtBuscar.Focus();
            return;
        }

        int posicion = -1;
        for (int i = 0; i < _cantidad; i++)
        {
            if (string.Equals(_expedientes[i].Codigo, codigoBuscado, StringComparison.OrdinalIgnoreCase))
            {
                posicion = i;
                break;
            }
        }

        if (posicion < 0)
        {
            _tabla.ClearSelection();
            MostrarEstado($"No se encontró el expediente {codigoBuscado}.");
            MessageBox.Show("El expediente no existe en el registro.", "Búsqueda",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _tabla.ClearSelection();
        _tabla.Rows[posicion].Selected = true;
        _tabla.FirstDisplayedScrollingRowIndex = posicion;
        MostrarEstado($"Expediente {codigoBuscado} encontrado.");
    }

    private void OrdenarExpedientes()
    {
        Array.Sort(_expedientes, 0, _cantidad,
            Comparer<Expediente>.Create((a, b) => string.Compare(
                a.Codigo, b.Codigo, StringComparison.OrdinalIgnoreCase)));
        ActualizarTabla();
        try
        {
            GuardarArchivo();
            MostrarEstado("Expedientes ordenados por código y guardados.");
        }
        catch (Exception ex)
        {
            MostrarError($"Se ordenaron los registros en memoria, pero no se pudieron guardar. {ex.Message}");
        }
    }

    private void MostrarTodos()
    {
        ActualizarTabla();
        _tabla.ClearSelection();
        MostrarEstado($"Se muestran los {_cantidad} expediente(s) registrados.");
    }

    private void ActualizarTabla()
    {
        _tabla.Rows.Clear();
        for (int i = 0; i < _cantidad; i++)
        {
            Expediente e = _expedientes[i];
            _tabla.Rows.Add(e.Codigo, e.Solicitante, e.DNI, e.TipoDocumento,
                e.Asunto, e.Fecha.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-PE")));
        }
    }

    private void GuardarDesdeBoton()
    {
        try
        {
            GuardarArchivo();
            MostrarEstado($"Archivo guardado. Registros: {_cantidad}.");
        }
        catch (Exception ex)
        {
            MostrarError($"No se pudo guardar el archivo. {ex.Message}");
        }
    }

    private void GuardarArchivo()
    {
        var lineas = new List<string>(_cantidad);
        for (int i = 0; i < _cantidad; i++)
        {
            Expediente e = _expedientes[i];
            lineas.Add(string.Join(";", new[]
            {
                LimpiarSeparador(e.Codigo), LimpiarSeparador(e.Solicitante),
                LimpiarSeparador(e.DNI), LimpiarSeparador(e.TipoDocumento),
                LimpiarSeparador(e.Asunto), e.Fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            }));
        }
        File.WriteAllLines(_rutaArchivo, lineas, new UTF8Encoding(false));
    }

    private void CargarArchivo()
    {
        if (!File.Exists(_rutaArchivo)) return;

        foreach (string linea in File.ReadAllLines(_rutaArchivo, Encoding.UTF8))
        {
            string[] partes = linea.Split(';');
            if (partes.Length != 6) continue;
            if (!DateTime.TryParseExact(partes[5], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime fecha)) continue;
            if (string.IsNullOrWhiteSpace(partes[0])) continue;

            bool codigoRepetido = false;
            for (int i = 0; i < _cantidad; i++)
            {
                if (string.Equals(_expedientes[i].Codigo, partes[0], StringComparison.OrdinalIgnoreCase))
                {
                    codigoRepetido = true;
                    break;
                }
            }
            if (codigoRepetido) continue;

            AsegurarCapacidad();
            _expedientes[_cantidad] = new Expediente(
                partes[0], partes[1], partes[2], partes[3], partes[4], fecha);
            _cantidad++;
        }
    }

    private void ActualizarCodigoVisual() => _txtCodigo.Text = GenerarCodigoSiguiente();

    private void LimpiarCampos(bool mantenerEstado = false)
    {
        _txtSolicitante.Clear();
        _txtDni.Clear();
        _txtAsunto.Clear();
        _cmbTipoDocumento.SelectedIndex = 0;
        _dtpFecha.Value = DateTime.Today;
        _txtSolicitante.Focus();
        if (!mantenerEstado) MostrarEstado("Formulario listo para un nuevo expediente.");
    }

    private static string LimpiarSeparador(string valor) => valor.Replace(';', ',').Replace('\r', ' ').Replace('\n', ' ');

    private void MostrarError(string mensaje)
    {
        MostrarEstado(mensaje);
        MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void MostrarEstado(string mensaje) => _lblEstado.Text = mensaje;
}
