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
    public partial class frmEmpleados : Form
    {
        // Nombre exacto de la tabla (antes decía "Empleados", incorrecto)
        private const string TABLA = "Empleado";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmEmpleados(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmEmpleados_Load(object sender, EventArgs e)
        {
            this.Tag = TABLA;   // requerido para click-a-campo en el grid

            var (btnInsertar, btnEliminar, btnActualizar, btnConsulta, btnLimpiar) =
                FormularioPrincipal.InicializarModulo(this, TABLA);

            btnInsertar.Click += BtnInsertar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnConsulta.Click += BtnConsulta_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        // Columnas: ID_Empleado (IDENTITY), Turno, Tipo_Empleado (combo), Personal_ID_Personal (FK)
        private (object IDEmpleado, object Turno, object TipoEmpleado, object PersonalID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Empleado", TABLA, FormularioPrincipal.ObtenerTipo("ID_Empleado", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Turno", TABLA, FormularioPrincipal.ObtenerTipo("Turno", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Tipo_Empleado", TABLA, FormularioPrincipal.ObtenerTipo("Tipo_Empleado", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Personal_ID_Personal", TABLA, FormularioPrincipal.ObtenerTipo("Personal_ID_Personal", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, Turno, TipoEmpleado, PersonalID) = LeerCampos();

            if (PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Seleccione el Personal asociado.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Empleado (Turno, Tipo_Empleado, Personal_ID_Personal)
                           VALUES (@Turno, @Tipo_Empleado, @Personal_ID_Personal)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Turno", Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tipo_Empleado", TipoEmpleado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Empleado registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Empleado", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDEmpleado, _, _, _) = LeerCampos();

            if (IDEmpleado == DBNull.Value || IDEmpleado == null)
            {
                MessageBox.Show("Seleccione un empleado del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Empleado con ID {IDEmpleado}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Empleado WHERE ID_Empleado = @ID_Empleado", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Empleado", IDEmpleado);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Empleado eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Empleado", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDEmpleado, Turno, TipoEmpleado, PersonalID) = LeerCampos();

            if (IDEmpleado == DBNull.Value || IDEmpleado == null)
            {
                MessageBox.Show("Seleccione un empleado del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Seleccione el Personal asociado.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Empleado
                           SET Turno               = @Turno,
                               Tipo_Empleado       = @Tipo_Empleado,
                               Personal_ID_Personal = @Personal_ID_Personal
                           WHERE ID_Empleado = @ID_Empleado";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Empleado", IDEmpleado);
                    cmd.Parameters.AddWithValue("@Turno", Turno ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tipo_Empleado", TipoEmpleado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Empleado actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Empleado", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            string termino = MostrarDialogoBusqueda("Buscar en Empleados (ID, Cedula)");
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

                    // Se convierte el id a string para poder utilizar el operador LIKE
                    dt.DefaultView.RowFilter =
                        $"CONVERT(ID_Empleado, 'System.String') LIKE '%{t}%' " +
                        $"OR Turno LIKE '%{t}%' " +
                        $"OR Tipo_Empleado LIKE '%{t}%' " +
                        $"OR CONVERT(Personal_ID_Personal, 'System.String') LIKE '%{t}%'";
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
