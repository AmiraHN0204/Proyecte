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
    public partial class frmVacaciones : Form
    {
        private const string TABLA = "Vacaciones";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmVacaciones(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmVacaciones_Load(object sender, EventArgs e)
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

        // Columnas: ID_Vacaciones (IDENTITY), Fecha_Inicio (date), Fecha_Fin (date),
        //           Estado (combo TipoVacaciones), Personal_ID_Personal (FK)
        private (object IDVacaciones, object FechaInicio, object FechaFin,
                 object Estado, object PersonalID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Vacaciones", TABLA, FormularioPrincipal.ObtenerTipo("ID_Vacaciones", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Inicio", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Inicio", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_Fin", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_Fin", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Estado", TABLA, FormularioPrincipal.ObtenerTipo("Estado", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Personal_ID_Personal", TABLA, FormularioPrincipal.ObtenerTipo("Personal_ID_Personal", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, FechaInicio, FechaFin, Estado, PersonalID) = LeerCampos();

            if (PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Seleccione el Personal.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (FechaInicio == DBNull.Value || FechaInicio == null ||
                FechaFin == DBNull.Value || FechaFin == null)
            {
                MessageBox.Show("Las fechas de inicio y fin son obligatorias.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que Fecha_Fin >= Fecha_Inicio
            if (FechaInicio is DateTime fi && FechaFin is DateTime ff && ff < fi)
            {
                MessageBox.Show("La Fecha Fin no puede ser anterior a la Fecha Inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Vacaciones (Fecha_Inicio, Fecha_Fin, Estado, Personal_ID_Personal)
                           VALUES (@Fecha_Inicio, @Fecha_Fin, @Estado, @Personal_ID_Personal)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", FechaInicio);
                    cmd.Parameters.AddWithValue("@Fecha_Fin", FechaFin);
                    cmd.Parameters.AddWithValue("@Estado", Estado ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Vacaciones registradas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDVacaciones, _, _, _, _) = LeerCampos();

            if (IDVacaciones == DBNull.Value || IDVacaciones == null)
            {
                MessageBox.Show("Seleccione un registro del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar las Vacaciones con ID {IDVacaciones}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Vacaciones WHERE ID_Vacaciones = @ID_Vacaciones", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Vacaciones", IDVacaciones);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Vacaciones eliminadas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDVacaciones, FechaInicio, FechaFin, Estado, PersonalID) = LeerCampos();

            if (IDVacaciones == DBNull.Value || IDVacaciones == null)
            {
                MessageBox.Show("Seleccione un registro del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (FechaInicio is DateTime fi2 && FechaFin is DateTime ff2 && ff2 < fi2)
            {
                MessageBox.Show("La Fecha Fin no puede ser anterior a la Fecha Inicio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Vacaciones
                           SET Fecha_Inicio        = @Fecha_Inicio,
                               Fecha_Fin           = @Fecha_Fin,
                               Estado              = @Estado,
                               Personal_ID_Personal = @Personal_ID_Personal
                           WHERE ID_Vacaciones = @ID_Vacaciones";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Vacaciones", IDVacaciones);
                    cmd.Parameters.AddWithValue("@Fecha_Inicio", FechaInicio ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fecha_Fin", FechaFin ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", Estado ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID ?? (object)DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Vacaciones actualizadas correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Vacaciones", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Vacaciones (ID, Fecha inicio y fin, Estado)");
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

                    // Se convierte el id y las fechas en string para poder utilizar operador LIKE
                    dt.DefaultView.RowFilter =
                        $"CONVERT(ID_Vacaciones, 'System.String') LIKE '%{t}%' " +
                        $"OR Estado LIKE '%{t}%' " +
                        $"OR CONVERT(Fecha_Inicio, 'System.String') LIKE '%{t}%' " +
                        $"OR CONVERT(Fecha_Fin, 'System.String') LIKE '%{t}%'";
                }
            }
        }

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            FormularioPrincipal.LimpiarCampos(TABLA, this);
            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt) dt.DefaultView.RowFilter = "";
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