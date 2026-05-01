using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReVita
{
    public partial class frmPersonal : Form
    {
        private const string TABLA = "Personal";
        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmPersonal(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmPersonal_Load(object sender, EventArgs e)
        {
            // Tag requerido para que Grid_CellClick_Generico ubique este form
            this.Tag = TABLA;

            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, TABLA);

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        // ── Leer todos los campos del formulario ──────────────────────────────
        private (object IDPersonal, object Nombre, object Direccion, object Telefono,
                 object Poblacion, object Provincia, object CodPostal, object NSS) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Personal", TABLA, FormularioPrincipal.ObtenerTipo("ID_Personal", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Nombre", TABLA, FormularioPrincipal.ObtenerTipo("Nombre", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Direccion", TABLA, FormularioPrincipal.ObtenerTipo("Direccion", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Telefono", TABLA, FormularioPrincipal.ObtenerTipo("Telefono", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Poblacion", TABLA, FormularioPrincipal.ObtenerTipo("Poblacion", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Provincia", TABLA, FormularioPrincipal.ObtenerTipo("Provincia", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Codigo_Postal", TABLA, FormularioPrincipal.ObtenerTipo("Codigo_Postal", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "NSS", TABLA, FormularioPrincipal.ObtenerTipo("NSS", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, Nombre, Direccion, Telefono, Poblacion, Provincia, CodPostal, NSS) = LeerCampos();

            if (Nombre == DBNull.Value || Nombre == null)
            {
                MessageBox.Show("El campo Nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ID_Personal es IDENTITY → no se incluye en el INSERT
            string sql = @"INSERT INTO Personal
                               (Nombre, Direccion, Telefono, Poblacion, Provincia, Codigo_Postal, NSS)
                           VALUES
                               (@Nombre, @Direccion, @Telefono, @Poblacion, @Provincia, @Codigo_Postal, @NSS)";

            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Nombre", Nombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Poblacion", Poblacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Provincia", Provincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Codigo_Postal", CodPostal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NSS", NSS ?? DBNull.Value);

                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Registro insertado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar en Personal", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, _, _, _, _, _, _, _) = LeerCampos();

            if (IDPersonal == DBNull.Value || IDPersonal == null)
            {
                MessageBox.Show("Seleccione un registro del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Personal con ID {IDPersonal}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Personal WHERE ID_Personal = @ID_Personal", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Registro eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar de Personal", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDPersonal, Nombre, Direccion, Telefono, Poblacion, Provincia, CodPostal, NSS) = LeerCampos();

            if (IDPersonal == DBNull.Value || IDPersonal == null)
            {
                MessageBox.Show("Seleccione un registro del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Nombre == DBNull.Value || Nombre == null)
            {
                MessageBox.Show("El campo Nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Personal
                           SET Nombre        = @Nombre,
                               Direccion     = @Direccion,
                               Telefono      = @Telefono,
                               Poblacion     = @Poblacion,
                               Provincia     = @Provincia,
                               Codigo_Postal = @Codigo_Postal,
                               NSS           = @NSS
                           WHERE ID_Personal = @ID_Personal";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Personal", IDPersonal);
                    cmd.Parameters.AddWithValue("@Nombre", Nombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Poblacion", Poblacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Provincia", Provincia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Codigo_Postal", CodPostal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NSS", NSS ?? DBNull.Value);

                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Registro modificado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Personal", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Personal (Nombre, NSS, Población…)");
            if (termino == null) return;   // canceló

            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt)
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    string t = termino.Replace("'", "''");
                    dt.DefaultView.RowFilter =
                        $"Nombre LIKE '%{t}%' OR NSS LIKE '%{t}%' OR " +
                        $"Poblacion LIKE '%{t}%' OR Provincia LIKE '%{t}%'";
                }
            }
        }

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            FormularioPrincipal.LimpiarCampos(TABLA, this);

            // Quitar cualquier filtro del grid
            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt)
                dt.DefaultView.RowFilter = "";
        }

        // ── Helper: mini-diálogo de búsqueda ─────────────────────────────────
        private string MostrarDialogoBusqueda(string instruccion)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Consulta";
                dlg.Size = new Size(380, 140);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false; dlg.MinimizeBox = false;
                dlg.BackColor = Color.FloralWhite;

                Label lbl = new Label { Text = instruccion, Location = new Point(12, 14), AutoSize = true };
                TextBox txt = new TextBox { Location = new Point(12, 38), Width = 340 };
                Button btnOk = new Button
                {
                    Text = "Buscar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(200, 68),
                    Width = 80
                };
                Button btnClear = new Button
                {
                    Text = "Ver todos",
                    DialogResult = DialogResult.No,
                    Location = new Point(290, 68),
                    Width = 70
                };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnClear });
                dlg.AcceptButton = btnOk;

                DialogResult res = dlg.ShowDialog(this);
                if (res == DialogResult.No) return "";          // mostrar todos
                if (res == DialogResult.OK) return txt.Text;
                return null;                                       // canceló
            }
        }
    }
}
