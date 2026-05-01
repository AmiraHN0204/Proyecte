// ═══════════════════════════════════════════════════════════════════════════════
//  Form1.cs  —  Contenedor principal
// ═══════════════════════════════════════════════════════════════════════════════
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ReVita
{
    public enum TipoCampo
    {
        Texto,      // TextBox
        ID,         // ComboBox (FK)
        Fecha,      // DateTimePicker fecha corta
        Hora,       // DateTimePicker con flechas (sin calendario)
        DiaSemana,  // ComboBox con días fijos
        TipoVacaciones,
        TipoEmpleado
    }

    public partial class Form1 : Form
    {
        // ── Conexión compartida ───────────────────────────────────────────────────
        SqlConnection Conexion = new SqlConnection(
            @"server=.\SQLEXPRESS;database=HOSPITAL;integrated security=true;");

        // ── Variables para la animación del menú lateral ─────────────────────────
        private Timer timerMenuLateral;
        private bool expandirMenu = false;
        private const int ANCHO_EXPANDIDO = 200;
        private const int ANCHO_CONTRAIDO = 10;
        private const int margen = 10;

        // ── Panel contenedor principal ────────────────────────────────────────────
        Panel PanelCuerpoPrincipal;

        // ── Variables para la barra de título personalizada ───────────────────────
        private Panel pnlTitleBar;
        private bool isDragging = false;
        private Point dragStart;


        // ── Variables para el Carrusel del Inicio ────────────────────────────────
        private Timer timerCarrusel;
        private int imagenActual = 1;
        private PictureBox picCarrusel;
        private Label lblIndicadores;

        // ════════════════════════════════════════════════════════════════════════════
        //  METADATOS — describen cada tabla de la BDD
        // ════════════════════════════════════════════════════════════════════════════
        private readonly Dictionary<string, string[]> tablasCampos = new Dictionary<string, string[]>
{
    { "Personal",    new[] { "ID_Personal", "Nombre", "Direccion", "Telefono", "Poblacion", "Provincia", "Codigo_Postal", "NSS" } },
    { "Vacaciones",  new[] { "ID_Vacaciones", "Fecha_Inicio", "Fecha_Fin", "Estado", "Personal_ID_Personal" } },
    { "Empleado",    new[] { "ID_Empleado", "Turno", "Tipo_Empleado", "Personal_ID_Personal" } },
    { "Medico",      new[] { "ID_Medico", "Cedula", "Personal_ID_Personal" } },
    { "Titular",     new[] { "Medico_ID_Medico", "Consultorio_Principal" } },
    { "Interino",    new[] { "Medico_ID_Medico", "Fecha_FinContrato" } },
    { "Sustituto",   new[] { "Medico_ID_Medico" } },
    { "Sustitucion", new[] { "ID_Sustitucion", "Fecha_Alta", "Fecha_Baja", "Sustituto_Medico_ID_Medico" } },
    { "Horario",     new[] { "ID_Horario", "Dia_Semana", "Hora_Inicio", "Hora_Fin", "Medico_ID_Medico" } },
    { "Paciente",    new[] { "ID_Paciente", "Nombre_Pac", "Direccion_Pac", "Telefono_Pac", "CodigoP_Pac", "NSS_Pac", "Medico_ID_Medico" } }
};

        private readonly Dictionary<string, string> tablasPK = new Dictionary<string, string>
{
    { "Personal",    "ID_Personal" },
    { "Vacaciones",  "ID_Vacaciones" },
    { "Empleado",    "ID_Empleado" },
    { "Medico",      "ID_Medico" },
    { "Titular",     "Medico_ID_Medico" },
    { "Interino",    "Medico_ID_Medico" },
    { "Sustituto",   "Medico_ID_Medico" },
    { "Sustitucion", "ID_Sustitucion" },
    { "Horario",     "ID_Horario" },
    { "Paciente",    "ID_Paciente" }
};

        // El valor de esta tabla se muestra en el ComboBox de selección cuando un campo es FK
        public readonly Dictionary<string, string> tablasDisplayCol = new Dictionary<string, string>
{
    { "Personal",    "Nombre" },
    { "Medico",      "Cedula" },
    { "Empleado",    "Tipo_Empleado" },
    { "Sustituto",   "Medico_ID_Medico" },
    { "Paciente",    "Nombre_Pac" },
};

        private readonly Dictionary<string, byte[]> fotoBytesPorTabla = new Dictionary<string, byte[]>();

        // ════════════════════════════════════════════════════════════════════════════
        //  REGIÓN: Tipos de campo
        // ════════════════════════════════════════════════════════════════════════════
        #region Tipos de campo

        private TipoCampo InferirTipoCampo(string campo, string nombreTabla)
        {
            if (campo == "Dia_Semana") return TipoCampo.DiaSemana;
            if (campo.Contains("Fecha")) return TipoCampo.Fecha;
            if (campo == "Hora_Inicio" || campo == "Hora_Fin") return TipoCampo.Hora;
            if (campo == "Estado") return TipoCampo.TipoVacaciones;
            if (campo == "Tipo_Empleado") return TipoCampo.TipoEmpleado;

            // Detectar Llaves Foráneas exactas para generar ComboBox
            if (campo == "Personal_ID_Personal" || campo == "Medico_ID_Medico" || campo == "Sustituto_Medico_ID_Medico")
                return TipoCampo.ID;

            return TipoCampo.Texto;
        }

        private string TablaOrigenDeFK(string campoFK)
        {
            if (campoFK == "Personal_ID_Personal") return "Personal";
            if (campoFK == "Medico_ID_Medico") return "Medico";
            if (campoFK == "Sustituto_Medico_ID_Medico") return "Sustituto";
            return null;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ════════════════════════════════════════════════════════════════════════════
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;

            // ── Configurar menú ───────────────────────────────────────────────────
            ToolStripManager.VisualStylesEnabled = false;
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                .SetValue(mnuBarra, true, null);

            mnuBarra.Renderer = new FadeRenderer();
            mnuBarra.BackColor = ColorTranslator.FromHtml("#2F6F5E");
            mnuBarra.ForeColor = Color.White;
            mnuBarra.GripStyle = ToolStripGripStyle.Hidden;
            mnuBarra.Padding = new Padding(10, 5, 10, 5);

            var imagenOriginal = Properties.Resources.HeaderLOGO;
            var imagenRedimensionada = new Bitmap(imagenOriginal, new Size(120, 60));

            iNICIOToolStripMenuItem.Image = imagenRedimensionada;
            iNICIOToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iNICIOToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            iNICIOToolStripMenuItem.Click += (s, e) =>
            {
                ConfigurarInicio();
                var lbl = pnlTitleBar?.Controls.OfType<Label>().FirstOrDefault();
                if (lbl != null) lbl.Text = "Inicio";
            };

            EstiloPanelesModulos();
            this.Controls.Add(PanelCuerpoPrincipal);

            ConfigurarAnimacionMenu();
            GenerarMenuTablas();
            CrearBarraTitulo();
            ConfigurarInicio();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  REGIÓN: Infraestructura — paneles y menú
        // ════════════════════════════════════════════════════════════════════════════
        #region Infraestructura — paneles y menú

        private void EstiloPanelesModulos()
        {
            int anchoDisponible = this.ClientSize.Width - ANCHO_CONTRAIDO - margen * 2;
            int altoDisponible = this.ClientSize.Height - 32 - margen * 2;

            if (PanelCuerpoPrincipal == null)
            {
                // Primera vez: crear el panel
                PanelCuerpoPrincipal = new Panel
                {
                    Size = new Size(anchoDisponible, altoDisponible),
                    Location = new Point(ANCHO_CONTRAIDO + margen, 32 + margen)
                };
            }
            else
            {
                // Redimensionar si ya existe (resize del form)
                PanelCuerpoPrincipal.Size = new Size(anchoDisponible, altoDisponible);
                PanelCuerpoPrincipal.Location = new Point(ANCHO_CONTRAIDO + margen, 32 + margen);
            }
        }

        private void ConfigurarAnimacionMenu()
        {
            mnuBarra.Dock = DockStyle.Left;
            mnuBarra.Location = new Point(0, 32);
            mnuBarra.AutoSize = false;
            mnuBarra.Width = ANCHO_CONTRAIDO;

            timerMenuLateral = new Timer();
            timerMenuLateral.Interval = 10;
            timerMenuLateral.Tick += TimerMenuLateral_Tick;

            mnuBarra.MouseEnter += MnuBarra_MouseEnter;
            mnuBarra.MouseLeave += MnuBarra_MouseLeave;
        }

        private void TimerMenuLateral_Tick(object sender, EventArgs e)
        {
            const int velocidad = 15;
            if (expandirMenu)
            {
                if (mnuBarra.Width < ANCHO_EXPANDIDO) mnuBarra.Width += velocidad;
                else timerMenuLateral.Stop();
            }
            else
            {
                if (mnuBarra.Width > ANCHO_CONTRAIDO) mnuBarra.Width -= velocidad;
                else timerMenuLateral.Stop();
            }
            this.Invalidate(new Rectangle(0, 32, ANCHO_EXPANDIDO + 30, this.ClientSize.Height - 32));
            this.Update();
        }

        private void MnuBarra_MouseEnter(object sender, EventArgs e)
        {
            expandirMenu = true;
            timerMenuLateral.Start();
        }

        private void MnuBarra_MouseLeave(object sender, EventArgs e)
        {
            if (!mnuBarra.ClientRectangle.Contains(mnuBarra.PointToClient(Cursor.Position)))
            {
                expandirMenu = false;
                timerMenuLateral.Start();
            }
        }

        private void GenerarMenuTablas()
        {
            string[] tablasBDD =
            {
                "Personal", "Vacaciones", "Empleado", "Medico", "Titular",
                "Interino", "Sustituto", "Sustitucion", "Horario", "Paciente"
            };

            foreach (string nombreTabla in tablasBDD)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(nombreTabla)
                {
                    Name = "mnu" + nombreTabla.Replace(" ", ""),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Padding = new Padding(10, 5, 10, 5)
                };
                menuItem.Click += MenuItemTabla_Click;
                mnuBarra.Items.Add(menuItem);
            }
        }

        private void MenuItemTabla_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item)) return;

            switch (item.Text)
            {
                case "Personal": AbrirFormModulo(new frmPersonal(this)); break;
                case "Vacaciones": AbrirFormModulo(new frmVacaciones(this)); break;
                case "Empleado": AbrirFormModulo(new frmEmpleados(this)); break;
                case "Medico": AbrirFormModulo(new frmMedicos(this)); break;
                case "Titular": AbrirFormModulo(new frmTitular(this)); break;
                case "Interino": AbrirFormModulo(new frmInterino(this)); break;
                case "Sustituto": AbrirFormModulo(new frmSustituto(this)); break;
                case "Sustitucion": AbrirFormModulo(new frmSustitucion(this)); break;
                case "Horario": AbrirFormModulo(new frmHorario(this)); break;
                case "Paciente": AbrirFormModulo(new frmPacientes(this)); break;
            }
        }

        public void AbrirFormModulo(Form nombreForm)
        {
            if (PanelCuerpoPrincipal == null) return;

            PanelCuerpoPrincipal.Controls.Clear();
            nombreForm.TopLevel = false;
            nombreForm.FormBorderStyle = FormBorderStyle.None;
            nombreForm.Dock = DockStyle.Fill;
            PanelCuerpoPrincipal.Controls.Add(nombreForm);
            nombreForm.Show(); // ← esto dispara el Load del form hijo
        }

        private void frmInicio_Resize(object sender, EventArgs e) => EstiloPanelesModulos();

        private void CrearBarraTitulo()
        {
            pnlTitleBar = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#2F6F5E"),
                Height = 32,
                Dock = DockStyle.Top
            };
            this.Controls.Add(pnlTitleBar);
            pnlTitleBar.BringToFront();

            Label lblTitulo = new Label
            {
                Text = "Inicio",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 5)
            };
            pnlTitleBar.Controls.Add(lblTitulo);

            foreach (ToolStripMenuItem item in mnuBarra.Items)
                if (item.Name.StartsWith("mnu"))
                    item.Click += (s, e) => lblTitulo.Text = item.Text;

            Button btnCerrar = FabricarBotonTitleBar("✕", Color.FromArgb(200, 80, 60));
            btnCerrar.Location = new Point(pnlTitleBar.Width - 46, 0);
            btnCerrar.Click += (s, e) => Application.Exit();
            pnlTitleBar.Controls.Add(btnCerrar);

            Button btnMaximizar = FabricarBotonTitleBar("□", ColorTranslator.FromHtml("#3D8A74"));
            btnMaximizar.Location = new Point(pnlTitleBar.Width - 92, 0);
            btnMaximizar.Click += (s, e) =>
                this.WindowState = this.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal : FormWindowState.Maximized;
            pnlTitleBar.Controls.Add(btnMaximizar);

            Button btnMinimizar = FabricarBotonTitleBar("—", ColorTranslator.FromHtml("#3D8A74"));
            btnMinimizar.Location = new Point(pnlTitleBar.Width - 138, 0);
            btnMinimizar.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            pnlTitleBar.Controls.Add(btnMinimizar);

            pnlTitleBar.Resize += (s, e) =>
            {
                btnCerrar.Location = new Point(pnlTitleBar.Width - 46, 0);
                btnMaximizar.Location = new Point(pnlTitleBar.Width - 92, 0);
                btnMinimizar.Location = new Point(pnlTitleBar.Width - 138, 0);
            };

            Action<Control> habilitarArrastre = ctrl =>
            {
                ctrl.MouseDown += (s, e) => { isDragging = true; dragStart = e.Location; };
                ctrl.MouseMove += (s, e) =>
                {
                    if (isDragging)
                        this.Location = new Point(
                            this.Left + e.X - dragStart.X,
                            this.Top + e.Y - dragStart.Y);
                };
                ctrl.MouseUp += (s, e) => isDragging = false;
            };
            habilitarArrastre(pnlTitleBar);
            habilitarArrastre(lblTitulo);
        }

        private Button FabricarBotonTitleBar(string texto, Color hoverColor)
        {
            var b = new Button
            {
                Text = texto,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(46, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hoverColor;
            return b;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REGIÓN: InicializarModulo
        // ════════════════════════════════════════════════════════════════════════════
        #region InicializarModulo

        /// <summary>
        /// Construye la UI completa del módulo dentro del form hijo y devuelve
        /// los 6 botones CRUD para que cada form conecte sus propios eventos.
        /// </summary>
        public (Button insertar, Button eliminar, Button actualizar,
                Button consulta, Button limpiar)
            InicializarModulo(Form nombreForm, string nombreTabla)
        {
            Panel panelDestino = new Panel
            {
                Dock = DockStyle.Fill,
                Tag = nombreTabla
            };

            Color colorVerde = ColorTranslator.FromHtml("#2F6F5E");
            Color colorLabel = Color.FromArgb(47, 111, 94);
            Font fntLabel = new Font("Segoe UI", 8, FontStyle.Bold);
            Font fntInput = new Font("Segoe UI", 10, FontStyle.Regular);
            Font fntInputBig = new Font("Segoe UI", 12, FontStyle.Regular);

            // ── Panel de botones (derecha) ────────────────────────────────────────
            Panel pnlBotones = new Panel
            {
                Dock = DockStyle.Right,
                Width = 80,
                BackColor = colorVerde,
                Padding = new Padding(8)
            };

            int x = 8, h = 60, w = 60, gap = 8;
            Button btnInsertar = FabricarBoton(null, colorVerde, new Point(x, 10), w, h, Properties.Resources.Altas);
            Button btnEliminar = FabricarBoton(null, colorVerde, new Point(x, 10 + (h + gap)), w, h, Properties.Resources.Bajas);
            Button btnActualizar = FabricarBoton(null, colorVerde, new Point(x, 10 + (h + gap) * 2), w, h, Properties.Resources.Modificaciones);
            Button btnConsulta = FabricarBoton(null, colorVerde, new Point(x, 10 + (h + gap) * 3), w, h, Properties.Resources.Consulta);
            Button btnLimpiar = FabricarBoton(null, colorVerde, new Point(x, 10 + (h + gap) * 4), w, h, Properties.Resources.Limpieza);

            foreach (Button b in new[] { btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar })
                b.Tag = nombreTabla;

            pnlBotones.Controls.AddRange(new Control[]
                { btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar });

            // ── DataGridView (abajo) ──────────────────────────────────────────────
            Panel pnlTabla = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 220,
                BackColor = colorVerde,
                Padding = new Padding(8)
            };
            DataGridView dgvGrilla = new DataGridView
            {
                Name = "dgv" + nombreTabla,
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FloralWhite,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 28 },
                Tag = nombreTabla
            };
            dgvGrilla.ColumnHeadersDefaultCellStyle.BackColor = colorVerde;
            dgvGrilla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvGrilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvGrilla.ColumnHeadersHeight = 35;
            dgvGrilla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 244);

            dgvGrilla.CellClick += Grid_CellClick_Generico;

            pnlTabla.Controls.Add(dgvGrilla);

            // ── Panel de campos con scroll (centro) ───────────────────────────────
            Panel pnlScrollCampos = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FloralWhite,
                Padding = new Padding(8)
            };

            string[] campos = tablasCampos[nombreTabla];
            int yPos = 10;

            foreach (string campo in campos)
            {
                TipoCampo tipo = InferirTipoCampo(campo, nombreTabla);
                string etiqueta = campo.Replace("_", " ");

                Label lbl = new Label
                {
                    Text = etiqueta,
                    Font = fntLabel,
                    ForeColor = colorLabel,
                    Location = new Point(20, yPos),
                    AutoSize = true
                };
                pnlScrollCampos.Controls.Add(lbl);

                switch (tipo)
                {
                    case TipoCampo.ID:
                        {
                            var cmb = new ComboBox
                            {
                                Name = "cmb" + campo,
                                Font = fntInput,
                                Width = 250,
                                Location = new Point(20, yPos + 20),
                                BackColor = Color.OldLace,
                                DropDownStyle = ComboBoxStyle.DropDownList
                            };
                            pnlScrollCampos.Controls.Add(cmb);
                            CargarComboFK(cmb, campo);
                            yPos += 62;
                            break;
                        }
                    case TipoCampo.Fecha:
                        {
                            var dtp = new DateTimePicker
                            {
                                Name = "dtp" + campo,
                                Format = DateTimePickerFormat.Short,
                                Font = fntInput,
                                Width = 200,
                                Location = new Point(20, yPos + 20),
                                CalendarMonthBackground = Color.FloralWhite,
                                CalendarTitleBackColor = colorVerde,
                                CalendarTitleForeColor = Color.White,
                                CalendarForeColor = Color.Black,
                                CalendarTrailingForeColor = Color.Silver
                            };
                            pnlScrollCampos.Controls.Add(dtp);
                            yPos += 62;
                            break;
                        }
                    case TipoCampo.Hora:
                        {
                            var dtpHora = new DateTimePicker
                            {
                                Name = "dtp" + campo,
                                Format = DateTimePickerFormat.Time,
                                ShowUpDown = true,
                                Font = fntInput,
                                Width = 160,
                                Location = new Point(20, yPos + 20)
                            };
                            pnlScrollCampos.Controls.Add(dtpHora);
                            yPos += 62;
                            break;
                        }
                    case TipoCampo.DiaSemana:
                        {
                            var cmbDia = new ComboBox
                            {
                                Name = "cmb" + campo,
                                Font = fntInput,
                                Width = 200,
                                Location = new Point(20, yPos + 20),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                BackColor = Color.OldLace
                            };
                            cmbDia.Items.AddRange(new object[]
                                { "Lunes", "Martes", "Miércoles", "Jueves",
                              "Viernes", "Sábado", "Domingo" });
                            pnlScrollCampos.Controls.Add(cmbDia);
                            yPos += 62;
                            break;
                        }
                    case TipoCampo.TipoEmpleado:
                        {
                            var cmbDia = new ComboBox
                            {
                                Name = "cmb" + campo,
                                Font = fntInput,
                                Width = 200,
                                Location = new Point(20, yPos + 20),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                BackColor = Color.OldLace
                            };
                            cmbDia.Items.AddRange(new object[]
                                { "Auxiliar de zona", "Auxiliar de enfermería", "Guardia de seguridad", "Administrativo" });
                            pnlScrollCampos.Controls.Add(cmbDia);
                            yPos += 62;
                            break;
                        }
                    case TipoCampo.TipoVacaciones:
                        {
                            var cmbDia = new ComboBox
                            {
                                Name = "cmb" + campo,
                                Font = fntInput,
                                Width = 200,
                                Location = new Point(20, yPos + 20),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                BackColor = Color.OldLace
                            };
                            cmbDia.Items.AddRange(new object[]
                                { "Planificadas", "Disfrutadas"});
                            pnlScrollCampos.Controls.Add(cmbDia);
                            yPos += 62;
                            break;
                        }
                    default:
                        {
                            var txt = new TextBox
                            {
                                Name = "txt" + campo,
                                Font = fntInputBig,
                                Width = 320,
                                Location = new Point(30, yPos + 23),
                                BackColor = Color.White,
                                BorderStyle = BorderStyle.None
                            };
                            var picFondo = new PictureBox
                            {
                                Location = new Point(20, txt.Top - 8),
                                Size = new Size(340, txt.Height + 10),
                                BorderStyle = BorderStyle.None,
                                BackgroundImage = Properties.Resources.texbox,
                                BackgroundImageLayout = ImageLayout.Stretch,
                                BackColor = Color.Transparent
                            };
                            pnlScrollCampos.Controls.Add(picFondo);
                            pnlScrollCampos.Controls.Add(txt);
                            txt.BringToFront();
                            yPos += 68;
                            break;
                        }
                }
            }

            // Logo semitransparente de fondo
            pnlScrollCampos.Paint += (s2, pe) =>
            {
                try
                {
                    Image logo = Properties.Resources.HeaderLOGO;
                    if (logo == null) return;
                    Panel pnl = (Panel)s2;
                    int scrollY = -pnl.AutoScrollPosition.Y;
                    const int margenCampos = 420;
                    int anchoLibre = pnl.ClientSize.Width - margenCampos;
                    int altoVisible = pnl.ClientSize.Height;
                    if (anchoLibre < 120 || altoVisible < 80) return;
                    float factorW = (anchoLibre * 0.70f) / logo.Width;
                    float factorH = (altoVisible * 0.70f) / logo.Height;
                    float factor = Math.Min(factorW, factorH);
                    int logoW = (int)(logo.Width * factor);
                    int logoH = (int)(logo.Height * factor);
                    int logoX = margenCampos + (anchoLibre - logoW) / 2;
                    int logoY = scrollY + (altoVisible - logoH) / 2;
                    var cm = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.28f };
                    using (var ia = new System.Drawing.Imaging.ImageAttributes())
                    {
                        ia.SetColorMatrix(cm);
                        pe.Graphics.DrawImage(logo,
                            new Rectangle(logoX, logoY, logoW, logoH),
                            0, 0, logo.Width, logo.Height,
                            GraphicsUnit.Pixel, ia);
                    }
                }
                catch { }
            };

            nombreForm.Controls.Add(panelDestino);
            panelDestino.Controls.Add(pnlBotones);       
            panelDestino.Controls.Add(pnlTabla);         
            panelDestino.Controls.Add(pnlScrollCampos);  
            pnlScrollCampos.BringToFront();

            // Agregar al form hijo
            nombreForm.Controls.Add(panelDestino);
            panelDestino.BringToFront();

            // Carga inicial del grid pasando el panel directamente
            CargarDatos(nombreTabla, nombreForm);

            //Devolver los botones para que cada form conecte sus eventos
            return (btnInsertar, btnEliminar, btnActualizar,
                    btnConsulta, btnLimpiar);
        }

        private void BtnSubirFoto_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string nombreTabla = btn?.Tag as string;
            if (nombreTabla == null) return;

            // Buscar el PictureBox subiendo por el árbol de controles
            Control padre = btn.Parent;
            PictureBox pic = null;
            while (padre != null)
            {
                pic = padre.Controls.Find("picFoto", true).FirstOrDefault() as PictureBox;
                if (pic != null) break;
                padre = padre.Parent;
            }
            if (pic == null) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar imagen";
                ofd.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    fotoBytesPorTabla[nombreTabla] = bytes;
                    using (var ms = new MemoryStream(bytes))
                        pic.Image = Image.FromStream(ms);
                    pic.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la foto: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REGIÓN: Helpers compartidos (usados por los forms hijos)
        // ════════════════════════════════════════════════════════════════════════════
        #region Helpers compartidos

        /// <summary>Recarga el DataGridView del módulo desde la BDD.</summary>
        public void CargarDatos(string nombreTabla, Form FormModulo)
        {
            var dgvRef = FormModulo?.Controls.Find("dgv" + nombreTabla, true)
                                       .FirstOrDefault() as DataGridView;
            if (dgvRef == null) return;

            try
            {
                if (Conexion.State == ConnectionState.Closed) Conexion.Open();
                var da = new SqlDataAdapter($"SELECT * FROM {nombreTabla}", Conexion);
                var dt = new DataTable();
                da.Fill(dt);
                dgvRef.DataSource = dt;
            }
            catch (Exception ex) { MostrarError("cargar datos de " + nombreTabla, ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        /// <summary>Lee el valor actual del control asociado al campo.</summary>
        public object LeerValorCampo(Form FormModulo, string campo, string nombreTabla, TipoCampo tipo)
        {
            switch (tipo)
            {
                case TipoCampo.ID:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        if (cmb == null || cmb.SelectedValue == null) return DBNull.Value;
                        return cmb.SelectedValue;
                    }
                case TipoCampo.DiaSemana:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        return cmb?.SelectedItem?.ToString() ?? (object)DBNull.Value;
                    }
                case TipoCampo.Fecha:
                    {
                        var dtp = CampoControl(FormModulo, campo, tipo) as DateTimePicker;
                        return dtp != null ? (object)dtp.Value.Date : DBNull.Value;
                    }
                case TipoCampo.Hora:
                    {
                        var dtp = CampoControl(FormModulo, campo, tipo) as DateTimePicker;
                        return dtp != null ? (object)dtp.Value.TimeOfDay : DBNull.Value;
                    }
                case TipoCampo.TipoEmpleado:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        return cmb?.SelectedItem?.ToString() ?? (object)DBNull.Value;
                    }
                case TipoCampo.TipoVacaciones:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        return cmb?.SelectedItem?.ToString() ?? (object)DBNull.Value;
                    }
                default:
                    {
                        var txt = CampoControl(FormModulo, campo, tipo) as TextBox;
                        string s = txt?.Text?.Trim() ?? "";
                        return string.IsNullOrEmpty(s) ? (object)DBNull.Value : s;
                    }
            }
        }

        /// <summary>Asigna al control del campo el valor leído desde la BDD.</summary>
        public void EscribirValorCampo(Form FormModulo, string campo, string nombreTabla, TipoCampo tipo, object valor)
        {
            bool esNull = valor == null || valor == DBNull.Value;
            switch (tipo)
            {
                case TipoCampo.ID:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        if (cmb == null) return;
                        if (esNull) { cmb.SelectedIndex = -1; break; }
                        try { cmb.SelectedValue = valor; } catch { cmb.SelectedIndex = -1; }
                        break;
                    }
                case TipoCampo.DiaSemana:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        if (cmb == null) return;
                        if (esNull) cmb.SelectedIndex = -1;
                        else { int idx = cmb.FindStringExact(valor.ToString()); cmb.SelectedIndex = idx; }
                        break;
                    }
                case TipoCampo.Fecha:
                case TipoCampo.Hora:
                    {
                        var dtp = CampoControl(FormModulo, campo, tipo) as DateTimePicker;
                        if (dtp == null) return;
                        if (esNull) { dtp.Value = DateTime.Today; break; }
                        if (valor is TimeSpan ts) dtp.Value = DateTime.Today.Add(ts);
                        else dtp.Value = Convert.ToDateTime(valor);
                        break;
                    }
                case TipoCampo.TipoEmpleado:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        if (cmb == null) return;
                        if (esNull) cmb.SelectedIndex = -1;
                        else { int idx = cmb.FindStringExact(valor.ToString()); cmb.SelectedIndex = idx; }
                        break;
                    }
                case TipoCampo.TipoVacaciones:
                    {
                        var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                        if (cmb == null) return;
                        if (esNull) cmb.SelectedIndex = -1;
                        else { int idx = cmb.FindStringExact(valor.ToString()); cmb.SelectedIndex = idx; }
                        break;
                    }
                default:
                    {
                        var txt = CampoControl(FormModulo, campo, tipo) as TextBox;
                        if (txt == null) return;
                        txt.Text = esNull ? "" : valor.ToString();
                        break;
                    }
            }
        }

        /// <summary>Limpia todos los controles del panel.</summary>
        public void LimpiarCampos(string nombreTabla, Form FormModulo)
        {
            if (FormModulo == null) return;
            foreach (string campo in tablasCampos[nombreTabla])
            {
                TipoCampo tipo = InferirTipoCampo(campo, nombreTabla);
                switch (tipo)
                {
                    case TipoCampo.ID:
                    case TipoCampo.DiaSemana:
                        {
                            var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                            if (cmb != null) cmb.SelectedIndex = -1;
                            break;
                        }
                    case TipoCampo.Fecha:
                    case TipoCampo.Hora:
                        {
                            var dtp = CampoControl(FormModulo, campo, tipo) as DateTimePicker;
                            if (dtp != null) dtp.Value = DateTime.Today;
                            break;
                        }
                    case TipoCampo.TipoEmpleado:
                        {
                            var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                            if (cmb != null) cmb.SelectedIndex = -1;
                            break;
                        }
                    case TipoCampo.TipoVacaciones:
                        {
                            var cmb = CampoControl(FormModulo, campo, tipo) as ComboBox;
                            if (cmb != null) cmb.SelectedIndex = -1;
                            break;
                        }
                    default:
                        {
                            var txt = CampoControl(FormModulo, campo, tipo) as TextBox;
                            if (txt != null) txt.Text = "";
                            break;
                        }
                }
            }
        }

        /// <summary>Devuelve el control del campo dentro del panel.</summary>
        public Control CampoControl(Form FormModulo, string campo, TipoCampo tipo)
        {
            string prefijo;
            switch (tipo)
            {
                case TipoCampo.ID:
                case TipoCampo.DiaSemana: prefijo = "cmb"; break;
                case TipoCampo.TipoEmpleado: prefijo = "cmb"; break;
                case TipoCampo.TipoVacaciones: prefijo = "cmb"; break;
                case TipoCampo.Fecha:
                case TipoCampo.Hora: prefijo = "dtp"; break;
                default: prefijo = "txt"; break;
            }
            return FormModulo.Controls.Find(prefijo + campo, true).FirstOrDefault();
        }

        /// <summary>Expone los metadatos de campos para que los forms hijos puedan usarlos.</summary>
        public string[] ObtenerCampos(string nombreTabla) => tablasCampos[nombreTabla];
        public string ObtenerPK(string nombreTabla) => tablasPK[nombreTabla];
        public TipoCampo ObtenerTipo(string campo, string tabla) => InferirTipoCampo(campo, tabla);
        public byte[] ObtenerFotoBytes(string tabla) => fotoBytesPorTabla.ContainsKey(tabla) ? fotoBytesPorTabla[tabla] : null;
        public SqlConnection ObtenerConexion() => Conexion;

        public void CargarComboFK(ComboBox cmb, string campoFK)
        {
            string tablaOrigen = TablaOrigenDeFK(campoFK);
            if (tablaOrigen == null) return;
            string pk = tablasPK[tablaOrigen];
            string display = tablasDisplayCol.ContainsKey(tablaOrigen) ? tablasDisplayCol[tablaOrigen] : pk;
            bool yaAbierta = Conexion.State == ConnectionState.Open;
            try
            {
                if (!yaAbierta) Conexion.Open();
                var da = new SqlDataAdapter($"SELECT {pk}, {display} FROM {tablaOrigen}", Conexion);
                var dt = new DataTable();
                da.Fill(dt);
                cmb.DataSource = dt;
                cmb.DisplayMember = display;
                cmb.ValueMember = pk;
                cmb.SelectedIndex = -1;
            }
            catch (Exception ex) { Debug.WriteLine($"FK {campoFK}: {ex.Message}"); }
            finally { if (!yaAbierta) Conexion.Close(); }
        }

        public void MostrarError(string accion, Exception ex) =>
            MessageBox.Show($"Error al {accion}:\n{ex.Message}", "Error SQL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        private Button FabricarBoton(string texto, Color fondo, Point pos, int w, int h, Image imagen)
        {
            var b = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FloralWhite,
                BackColor = fondo,
                FlatStyle = FlatStyle.Flat,
                Location = pos,
                Width = w,
                Height = h,
                Cursor = Cursors.Hand,
                BackgroundImage = imagen,
                BackgroundImageLayout = ImageLayout.Stretch
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(fondo, 0.15f);
            return b;
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  CellClick genérico — busca el panel subiendo por el árbol de controles
        // ════════════════════════════════════════════════════════════════════════════
        private void Grid_CellClick_Generico(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridView dgvSender = sender as DataGridView;
            string tabla = dgvSender?.Tag as string;
            if (tabla == null) return;

            Control padre = dgvSender.Parent;
            Form pnl = null;
            while (padre != null)
            {
                if (padre is Form p && p.Tag?.ToString() == tabla)
                {
                    pnl = p;
                    break;
                }
                padre = padre.Parent;
            }
            if (pnl == null) return;

            DataGridViewRow fila = dgvSender.Rows[e.RowIndex];
            foreach (string campo in tablasCampos[tabla])
            {
                if (!dgvSender.Columns.Contains(campo)) continue;
                object v = fila.Cells[campo].Value;
                EscribirValorCampo(pnl, campo, tabla, InferirTipoCampo(campo, tabla), v);
            }
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  REGIÓN: Configuración del Dashboard (Inicio)
        // ════════════════════════════════════════════════════════════════════════════
        public void ConfigurarInicio()
        {
            Panel pnlInicio = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FloralWhite
            };

            pnlInicio.Controls.Clear();
            PanelCuerpoPrincipal.Controls.Add(pnlInicio);

            // --- TÍTULO ---
            Label lblBienvenida = new Label
            {
                Text = "ReVita - Excelencia médica a tu alcance.",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#2F6F5E"),
                AutoSize = true,
                Location = new Point(25, 60)
            };
            pnlInicio.Controls.Add(lblBienvenida);


            // --- DIBUJAR TARJETAS DE RESUMEN (5) ---
            // Paleta ReVita
            Color vR = ColorTranslator.FromHtml("#2F6F5E"); Color vC = ColorTranslator.FromHtml("#4E9C81");
            Color aC = ColorTranslator.FromHtml("#1F618D"); Color nH = ColorTranslator.FromHtml("#5D6D7E");
            Color dR = ColorTranslator.FromHtml("#C7A942");

            // --- DIBUJAR TABLA DE ÚLTIMOS PACIENTES REGISTRADOS ---
            //DibujarSeccionUltimosPacientes(pnlInicio, 440); // Y = 


            // --- DIBUJAR LOGO EN GRAN FORMATO + CARRUSEL DE INSTALACIONES ---
            DibujarLogoGranFormato(pnlInicio, 930, 30);
            DibujarCarruselInstalaciones(pnlInicio, 880, 290);

        }

        // Método para dibujar la sección de "Últimos Pacientes Registrados" con un DataGridView estilizado.
        /*private void DibujarSeccionUltimosPacientes(Panel contenedor, int yInicial)
        {
            Label lblTabla = new Label
            {
                Text = "Últimos Pacientes Registrados",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(30, yInicial)
            };
            contenedor.Controls.Add(lblTabla);

            // Configuración del DataGridView
            DataGridView dgvBreve = new DataGridView
            {
                Name = "dgvHomePacientes",
                Location = new Point(25, yInicial + 40),
                Size = new Size(820, 180),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false
            };
            dgvBreve.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2F6F5E");
            dgvBreve.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBreve.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Fuente tabla 10

            contenedor.Controls.Add(dgvBreve);
        }*/

        // Método para dibujar el logo en gran formato dentro del dashboard.
        private void DibujarLogoGranFormato(Panel contenedor, int posX, int yInicial)
        {
            PictureBox picLogo = new PictureBox
            {
                Name = "picLogoGrande",
                Location = new Point(posX, yInicial),
                Size = new Size(320, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.HeaderLOGO,
                BackColor = Color.Transparent
            };
            contenedor.Controls.Add(picLogo);
        }

        // Método para dibujar el carrusel de instalaciones. 
        private void DibujarCarruselInstalaciones(Panel contenedor, int posX, int yInicial)
        {
            int anchoPic = 420;
            int altoPic = 236;

            Label lblTituloGaleria = new Label
            {
                Text = "Nuestras Instalaciones",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(posX, yInicial - 50)
            };
            contenedor.Controls.Add(lblTituloGaleria);

            // Marco para la foto del carrusel
            picCarrusel = new PictureBox
            {
                Name = "picCarruselInicio",
                Location = new Point(posX, yInicial),
                Size = new Size(anchoPic, altoPic),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            contenedor.Controls.Add(picCarrusel);


            // Botón Izquierdo (Retroceder)
            Button btnAtras = new Button
            {
                Text = "◄",
                Font = new Font("Segoe UI", 18),
                Size = new Size(45, 45),
                Location = new Point(posX - 15, yInicial + (altoPic / 2) - 22),
                BackColor = ColorTranslator.FromHtml("#2F6F5E"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAtras.FlatAppearance.BorderSize = 0;
            btnAtras.Click += (s, e) => CambiarImagenCarrusel(-1);


            // Botón Derecho (Avanzar)
            Button btnAdelante = new Button
            {
                Text = "►",
                Font = new Font("Segoe UI", 18),
                Size = new Size(45, 45),
                Location = new Point(posX + anchoPic - 30, yInicial + (altoPic / 2) - 22),
                BackColor = ColorTranslator.FromHtml("#2F6F5E"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAdelante.FlatAppearance.BorderSize = 0;
            btnAdelante.Click += (s, e) => CambiarImagenCarrusel(1);

            contenedor.Controls.Add(btnAtras);
            contenedor.Controls.Add(btnAdelante);
            btnAtras.BringToFront();
            btnAdelante.BringToFront();


            // Puntos indicadores debajo del carrusel
            lblIndicadores = new Label
            {
                Font = new Font("Arial", 20),
                ForeColor = ColorTranslator.FromHtml("#3D8B75"),
                AutoSize = false,
                Size = new Size(anchoPic, 40),
                Location = new Point(posX, yInicial + altoPic + 5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contenedor.Controls.Add(lblIndicadores);


            // Llamamos al motor para mostrar la primera imagen y configurar el timer de cambio automático cada 4 segundos
            CambiarImagenCarrusel(0);

            if (timerCarrusel != null) timerCarrusel.Stop();
            timerCarrusel = new Timer();
            timerCarrusel.Interval = 4000;
            timerCarrusel.Tick += (s, e) => CambiarImagenCarrusel(1);
            timerCarrusel.Start();
        }

        // ── Motor que controla la lógica de cambio de foto y puntos ──
        private void CambiarImagenCarrusel(int direccion)
        {
            imagenActual += direccion;

            // (Bucle infinito
            if (imagenActual > 6) imagenActual = 1;
            if (imagenActual < 1) imagenActual = 6;

            // Asignar la foto correspondiente
            switch (imagenActual)
            {
                case 1: picCarrusel.Image = Properties.Resources.Loc1; break;
                case 2: picCarrusel.Image = Properties.Resources.Loc2; break;
                case 3: picCarrusel.Image = Properties.Resources.Loc3; break;
                case 4: picCarrusel.Image = Properties.Resources.Loc4; break;
                case 5: picCarrusel.Image = Properties.Resources.Loc5; break;
                case 6: picCarrusel.Image = Properties.Resources.Loc6; break;
            }

            // Dibujamos los puntitos (● = Activo, ○ = Inactivo)
            string puntos = "";
            for (int i = 1; i <= 6; i++)
            {
                puntos += (i == imagenActual) ? "● " : "○ ";
            }
            lblIndicadores.Text = puntos.Trim();

            // Si el usuario picó el botón manualmente, reiniciamos el reloj para que 
            // no le cambie la foto 1 segundo después
            if (timerCarrusel != null && timerCarrusel.Enabled && direccion != 0)
            {
                timerCarrusel.Stop();
                timerCarrusel.Start();
            }
        }
    }
}