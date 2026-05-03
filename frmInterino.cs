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
// HOLA AMIGOS DE YOUTUBE ESTAMOS AQUI EN UN NUEVO VIDEO DE REPARACION DE CODIGO EN VISUAL STUDIO, EN ESTE CASO TENEMOS EL FORMULARIO DE INTERINO, QUE ES UNA TABLA QUE RELACIONA A LOS MEDICOS CON SUS CONTRATOS INTERINOS, ASI QUE VAMOS A VER COMO FUNCIONA ESTE
// FORMULARIO Y COMO SE REALIZAN LAS OPERACIONES BASICAS DE CRUD (CREAR, LEER, ACTUALIZAR Y ELIMINAR) EN ESTA TABLA. ASI QUE SIN MAS PREAMBULOS, VAMOS A EMPEZAR CON EL CODIGO.
namespace ReVita
{
    public partial class frmInterino : Form
    {
        private const string TABLA = "Interino";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmInterino(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmInterino_Load(object sender, EventArgs e)
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

        // Columnas: Medico_ID_Medico (PK + FK → Medico), Fecha_FinContrato (date)
        private (object MedicoID, object FechaFin) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Fecha_FinContrato", TABLA, FormularioPrincipal.ObtenerTipo("Fecha_FinContrato", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (MedicoID, FechaFin) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Interino (Medico_ID_Medico, Fecha_FinContrato)
                           VALUES (@Medico_ID_Medico, @Fecha_FinContrato)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    cmd.Parameters.AddWithValue("@Fecha_FinContrato", FechaFin ?? DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Interino registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (MedicoID, _) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione un interino del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Interino del Médico {MedicoID}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Interino WHERE Medico_ID_Medico = @Medico_ID_Medico", Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Interino eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR (solo Fecha_FinContrato) ───────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (MedicoID, FechaFin) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione un interino del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Interino
                           SET Fecha_FinContrato = @Fecha_FinContrato
                           WHERE Medico_ID_Medico = @Medico_ID_Medico";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    cmd.Parameters.AddWithValue("@Fecha_FinContrato", FechaFin ?? DBNull.Value);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Interino actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Interino", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── CONSULTA ─────────────────────────────────────────────────────────
        private void BtnConsulta_Click(object sender, EventArgs e)
        {
            // Recarga mostrando todos (útil para quitar filtros)
            FormularioPrincipal.CargarDatos(TABLA, this);
        }

        // ── LIMPIAR ───────────────────────────────────────────────────────────
        private void BtnLimpiar_Click(object sender, EventArgs e) =>
            FormularioPrincipal.LimpiarCampos(TABLA, this);
    }
}
