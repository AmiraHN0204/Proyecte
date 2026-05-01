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
    public partial class frmHorario : Form
    {
        // Antes decía "Horario_Medico" — nombre correcto en BDD y tablasCampos es "Horario"
        private const string TABLA = "Horario";

        Form1 FormularioPrincipal;
        SqlConnection Conexion;

        public frmHorario(Form1 Formulario)
        {
            InitializeComponent();
            FormularioPrincipal = Formulario;
            Conexion = FormularioPrincipal.ObtenerConexion();
        }

        private void frmHorario_Load(object sender, EventArgs e)
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

        // Columnas: ID_Horario (IDENTITY), Dia_Semana (combo), Hora_Inicio (time),
        //           Hora_Fin (time), Medico_ID_Medico (FK)
        private (object IDHorario, object DiaSemana, object HoraInicio,
                 object HoraFin, object MedicoID) LeerCampos()
        {
            return (
                FormularioPrincipal.LeerValorCampo(this, "ID_Horario", TABLA, FormularioPrincipal.ObtenerTipo("ID_Horario", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Dia_Semana", TABLA, FormularioPrincipal.ObtenerTipo("Dia_Semana", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Hora_Inicio", TABLA, FormularioPrincipal.ObtenerTipo("Hora_Inicio", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Hora_Fin", TABLA, FormularioPrincipal.ObtenerTipo("Hora_Fin", TABLA)),
                FormularioPrincipal.LeerValorCampo(this, "Medico_ID_Medico", TABLA, FormularioPrincipal.ObtenerTipo("Medico_ID_Medico", TABLA))
            );
        }

        // ── INSERTAR ──────────────────────────────────────────────────────────
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            var (_, DiaSemana, HoraInicio, HoraFin, MedicoID) = LeerCampos();

            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"INSERT INTO Horario (Dia_Semana, Hora_Inicio, Hora_Fin, Medico_ID_Medico)
                           VALUES (@Dia_Semana, @Hora_Inicio, @Hora_Fin, @Medico_ID_Medico)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@Dia_Semana", DiaSemana ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hora_Inicio", HoraInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hora_Fin", HoraFin ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Horario registrado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("insertar Horario", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ELIMINAR ──────────────────────────────────────────────────────────
        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            var (IDHorario, _, _, _, _) = LeerCampos();

            if (IDHorario == DBNull.Value || IDHorario == null)
            {
                MessageBox.Show("Seleccione un horario del grid para eliminar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"¿Eliminar el Horario con ID {IDHorario}?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Horario WHERE ID_Horario = @ID_Horario", Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Horario", IDHorario);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                FormularioPrincipal.LimpiarCampos(TABLA, this);
                MessageBox.Show("Horario eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("eliminar Horario", ex); }
            finally { if (Conexion.State == ConnectionState.Open) Conexion.Close(); }
        }

        // ── ACTUALIZAR ────────────────────────────────────────────────────────
        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            var (IDHorario, DiaSemana, HoraInicio, HoraFin, MedicoID) = LeerCampos();

            if (IDHorario == DBNull.Value || IDHorario == null)
            {
                MessageBox.Show("Seleccione un horario del grid para actualizar.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MedicoID == DBNull.Value || MedicoID == null)
            {
                MessageBox.Show("Seleccione el Médico.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sql = @"UPDATE Horario
                           SET Dia_Semana       = @Dia_Semana,
                               Hora_Inicio      = @Hora_Inicio,
                               Hora_Fin         = @Hora_Fin,
                               Medico_ID_Medico = @Medico_ID_Medico
                           WHERE ID_Horario = @ID_Horario";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, Conexion))
                {
                    cmd.Parameters.AddWithValue("@ID_Horario", IDHorario);
                    cmd.Parameters.AddWithValue("@Dia_Semana", DiaSemana ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hora_Inicio", HoraInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hora_Fin", HoraFin ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medico_ID_Medico", MedicoID);
                    if (Conexion.State != ConnectionState.Open) Conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                FormularioPrincipal.CargarDatos(TABLA, this);
                MessageBox.Show("Horario actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) { FormularioPrincipal.MostrarError("actualizar Horario", ex); }
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