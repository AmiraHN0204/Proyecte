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
    public partial class frmMedicos : Form
    {
        // Nombre exacto de la tabla en la BDD y en tablasCampos de Form1
        private const string TABLA = "Medico";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmMedicos(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void FormDoctores_Load(object sender, EventArgs e)
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

        // Columnas: ID_Medico (IDENTITY), Cedula, Personal_ID_Personal (FK)
        private (object IDMedico, object Cedula, object PersonalID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("ID_Medico", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Cedula", TABLA, FormularioPrincipal.ObtenerTipo("Cedula", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Personal_ID_Personal", TABLA, FormularioPrincipal.ObtenerTipo("Personal_ID_Personal", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, Cedula, PersonalID) = LeerCampos();

            if (Cedula == DBNull.Value || Cedula == null ||
                PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Cédula y Personal son obligatorios.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ID_Medico es IDENTITY → no se incluye
            string sql = @"INSERT INTO Medico (Cedula, Personal_ID_Personal)
                           VALUES (@Cedula, @Personal_ID_Personal)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Cedula", Cedula);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Médico registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Médico", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDMedico, _, _) = LeerCampos();

            if (IDMedico == DBNull.Value || IDMedico == null)
            {
                MessageBox.Show("Seleccione un médico del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Médico con ID {IDMedico}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Medico WHERE ID_Medico = @ID_Medico", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Medico", IDMedico);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Médico eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Médico", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDMedico, Cedula, PersonalID) = LeerCampos();

            if (IDMedico == DBNull.Value || IDMedico == null)
            {
                MessageBox.Show("Seleccione un médico del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Cedula == DBNull.Value || Cedula == null ||
                PersonalID == DBNull.Value || PersonalID == null)
            {
                MessageBox.Show("Cédula y Personal son obligatorios.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Medico
                           SET Cedula               = @Cedula,
                               Personal_ID_Personal = @Personal_ID_Personal
                           WHERE ID_Medico = @ID_Medico";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Medico", IDMedico);
                    cmd.Parameters.AddWithValue("@Cedula", Cedula);
                    cmd.Parameters.AddWithValue("@Personal_ID_Personal", PersonalID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Médico actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Médico", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA / FILTRAR ────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
        }

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            FormularioPrincipal.LimpiarCampos(TABLA, this);
            var dgv = this.Controls.Find("dgv" + TABLA, true).FirstOrDefault() as DataGridView;
            if (dgv?.DataSource is DataTable dt) dt.DefaultView.RowFilter = "";
        }

        
    }
}
