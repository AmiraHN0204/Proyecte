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
    public partial class frmSustitucion : Form
    {
        private const string TABLA = "Sustitucion";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmSustitucion(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmSustitucion_Load(object sender, EventArgs e)
        {
            this.Tag = TABLA;

            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, TABLA);

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        // Columnas: ID_Sustitucion (IDENTITY), Fecha_Alta (datetime),
        //           Fecha_Baja (datetime nullable), Sustituto_Medico_ID_Medico (FK → Sustituto)
        private (object IDSustitucion, object FechaAlta, object FechaBaja, object SustitutoID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Sustitucion", TABLA, FormularioPrincipal.ObtenerTipo("ID_Sustitucion", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Alta", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Alta", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Baja", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Baja", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Sustituto_Medico_ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("Sustituto_Medico_ID_Medico", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, FechaAlta, FechaBaja, SustitutoID) = LeerCampos();

            if (SustitutoID == DBNull.Value || SustitutoID == null)
            {
                MessageBox.Show("Seleccione el Sustituto.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (FechaAlta == DBNull.Value || FechaAlta == null)
            {
                MessageBox.Show("La Fecha de Alta es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Sustitucion (Fecha_Alta, Fecha_Baja, Sustituto_Medico_ID_Medico)
                           VALUES (@Fecha_Alta, @Fecha_Baja, @Sustituto_Medico_ID_Medico)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Fecha_Alta", FechaAlta);
                    cmd.Parameters.AddWithValue("@Fecha_Baja", FechaBaja ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sustituto_Medico_ID_Medico", SustitutoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Sustitución registrada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Sustitución", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDSustitucion, _, _, _) = LeerCampos();

            if (IDSustitucion == DBNull.Value || IDSustitucion == null)
            {
                MessageBox.Show("Seleccione una sustitución del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar la Sustitución con ID {IDSustitucion}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Sustitucion WHERE ID_Sustitucion = @ID_Sustitucion", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Sustitucion", IDSustitucion);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Sustitución eliminada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Sustitución", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDSustitucion, FechaAlta, FechaBaja, SustitutoID) = LeerCampos();

            if (IDSustitucion == DBNull.Value || IDSustitucion == null)
            {
                MessageBox.Show("Seleccione una sustitución del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Sustitucion
                           SET Fecha_Alta                 = @Fecha_Alta,
                               Fecha_Baja                 = @Fecha_Baja,
                               Sustituto_Medico_ID_Medico = @Sustituto_Medico_ID_Medico
                           WHERE ID_Sustitucion = @ID_Sustitucion";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Sustitucion", IDSustitucion);
                    cmd.Parameters.AddWithValue("@Fecha_Alta", FechaAlta ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha_Baja", FechaBaja ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sustituto_Medico_ID_Medico", SustitutoID ?? (object)DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Sustitución actualizada correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Sustitución", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA ─────────────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Sustitucion (Id Sustitucion, Id Medico)");
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
                    //al ser valores de tipo numerico no podemos utilizar el LIKE, asi que se tienen que convertir para hacer uso de esta funcion
                   dt.DefaultView.RowFilter = $"CONVERT(Sustituto_Medico_ID_Medico, 'System.String') LIKE '%{t}%' OR CONVERT(ID_Sustitucion, 'System.String') LIKE '%{t}%'";
                }
            }
        }
        //Mini dialogo de busqueda
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

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e) =>
            FormularioPrincipal.LimpiarCampos(TABLA, this);
    }
}
